using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;

/// <summary>
/// Sent when a player hits a note
/// </summary>
public class NoteHitMessage {
	public int PlayerNumber{ get; set; }
	public int InstrumentID{ get; set; }  // Used by Song.cs to play/mute player's instrument track
}

/// <summary>
/// Sent when a player misses a note
/// </summary>
public class NoteMissMessage {
	public int PlayerNumber{ get; set; }
	public int InstrumentID{ get; set; }  // Used by Song.cs to play/mute player's instrument track
}

/// <summary>
/// Sent when a battle begins
/// </summary>
public class EnterBattleMessage {
	/// Right now this is just used by Song.cs to cue unmuting instrument tracks.
}

/// <summary>
/// Sent when a battle ends
/// </summary>
public class ExitBattleMessage {
	/// Right now this is just used by Song.cs to cue muting instrument tracks.
}

public class BattleManager : MonoBehaviour {

	private class PlayerBattleData {
		public int instrumentID { get; set; }
		public int difficulty { get; set; }
		public Note[] battleNotes { get; set; }
		public int[] battleNoteStates { get; set; }
	}

	private Dictionary<int, PlayerBattleData> players;
	private MessageRouter messageRouter;
	private bool isInBattle;

	public int battleMeasures = 1;

	void Awake() {
		players = new Dictionary<int, PlayerBattleData> ();
	}

	void Start() {
		ServiceFactory.Instance.RegisterSingleton<BattleManager> (this);
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<EnterBattleMessage> (OnStartBattle);
		messageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
	}

	/// <summary>
	/// Register a new player or update a player's difficulty / instrument setting
	/// </summary>
	/// <param name="playerNumber">Player number.</param>
	/// <param name="instrumentID">Instrument ID.</param>
	/// <param name="difficulty">Difficulty.</param>
	public void RegisterPlayer(int playerNumber, int instrumentID, int difficulty) {
		if (players.ContainsKey(playerNumber)) {
			players [playerNumber].instrumentID = instrumentID;
			players [playerNumber].difficulty = difficulty;
		} else {
			players.Add (playerNumber, new PlayerBattleData (){ instrumentID = instrumentID, difficulty = difficulty });
		}
	}

	/// <summary>
	/// Sets the player instrument.
	/// </summary>
	/// <param name="playerNumber">Player number.</param>
	/// <param name="instrumentID">Instrument I.</param>
	public void SetPlayerInstrument(int playerNumber, int instrumentID) {
		Debug.Assert (players.ContainsKey (playerNumber));

	}

	void OnStartBattle(EnterBattleMessage m) {
		StartBattle ();
	}

	/// <summary>
	/// Starts the battle. Can be called externally or will be called automatically when a EnterBattleMessage is raised
	/// </summary>
	public void StartBattle() {
		//TODO: Make sure song doesn't unmute / send notes before measure actually starts
		Song song = ServiceFactory.Instance.Resolve<Song>();
		float startTime = song.playerPosition;
		float battleLength = song.beatsPerMeasure / song.bpm * 60f * battleMeasures;
		float endTime = startTime + battleLength;
		foreach (PlayerBattleData p in players.Values) {
			p.battleNotes = song.GetNotes (p.instrumentID, p.difficulty, startTime, endTime);
			p.battleNoteStates = new int[p.battleNotes.Length];
		}
		// TODO: Spawn note objects here
		StartCoroutine (EndBattle (battleLength));
		isInBattle = true;
	}

	private IEnumerator EndBattle(float secondsToWait) {
		yield return new WaitForSeconds (secondsToWait);
		// TODO: Add logic to determine winner, damage points, etc
		isInBattle = false;
		messageRouter.RaiseMessage (new ExitBattleMessage ());
	}

	protected void OnButtonDown(ButtonDownMessage m) {
		// Check if when the player strums that they actually hit something
		if (!isInBattle)
			return;
		switch (m.Button) {
		case InputButton.STRUM:
			InputButton[] frets = Interpreter.HeldFrets.ContainsKey (m.PlayerNumber) ? 
				Interpreter.HeldFrets [m.PlayerNumber].ToArray () : new InputButton[]{ };
			Note[] hitNotes = ServiceFactory.Instance.Resolve<Song> ()
				.GetHitNotes (players [m.PlayerNumber].instrumentID, players [m.PlayerNumber].difficulty, frets);
			// TODO: Add support for HOPOs
			bool noteWasHit = false;
			// Go through the possible notes that the player could have been trying to hit
			foreach (Note n in hitNotes) { // Warning! O(n^2)
				if (noteWasHit)
					break;
				for (int i = 0; i < players [m.PlayerNumber].battleNotes.Length; i++) {
					if (n.Equals (players [m.PlayerNumber].battleNotes [i])) {
						if (players [m.PlayerNumber].battleNoteStates [i] == 0) {
							players [m.PlayerNumber].battleNoteStates [i] = 1;
							noteWasHit = true;
							messageRouter.RaiseMessage (new NoteHitMessage () {
								PlayerNumber = m.PlayerNumber,
								InstrumentID = players [m.PlayerNumber].instrumentID
							});
							break;
						}
					}
				}
			}
			if (!noteWasHit) {
				messageRouter.RaiseMessage (new NoteMissMessage () {
					PlayerNumber = m.PlayerNumber,
					InstrumentID = players [m.PlayerNumber].instrumentID
				});
			}
			break;
		default:
			break;
		}
	}
}
