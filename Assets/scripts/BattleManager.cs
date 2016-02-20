using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using System.Linq;
using System;

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
	public MelodyUnit AttackingUnit {get; set;}
	public MelodyUnit DefendingUnit {get; set;}
}

/// <summary>
/// Sent when a battle ends
/// </summary>
public class ExitBattleMessage {
	/// Right now this is just used by Song.cs to cue muting instrument tracks.
	public MelodyUnit AttackingUnit {get; set;}
	public MelodyUnit DefendingUnit {get; set;}
	public float AttackerHitPercent{ get; set; }
	public float DefenderHitPercent{ get; set; }
}

/// <summary>
/// Sent when a player wishes to change their difficulty level (Guitar Hero - like)
/// </summary>
public class BattleDifficultyChangeMessage {
	public int PlayerNumber {get; set;}
	public int Difficulty {get; set;}
}

public class BattleManager : MonoBehaviour {

	private class PlayerBattleData {
		public int instrumentID { get; set; }
		public int difficulty { get; set; }
		public Note[] battleNotes { get; set; }
		public int[] battleNoteStates { get; set; }
		public MelodyUnit unit {get; set;}
		public int playerNumber { get; set; }
	}

	private Dictionary<int, PlayerBattleData> players;
	private MessageRouter messageRouter;
	private bool isInBattle;
	private PlayerBattleData attacker;
	private PlayerBattleData defender;
	private int battleProgressInMeasures; // Number of measures into the battle
	private MeshRenderer targetLine; // Renderer for note targets. Currently a temporary black line.
	private GameObject divider; // Divider between player's notes

	public int battleMeasures = 1;
	public Camera parentCam;
	public GameObject notePrefab;

	//Constants
	private const float SPAWN_DEPTH = 13f;

	void Awake() {
		players = new Dictionary<int, PlayerBattleData> ();
	}

	void Start() {
		ServiceFactory.Instance.RegisterSingleton<BattleManager> (this);
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<EnterBattleMessage> (OnStartBattle);
		messageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		messageRouter.AddHandler<BeatCenterMessage> (OnBeatCenter);
		messageRouter.AddHandler<BattleDifficultyChangeMessage> (OnBattleDifficultyChange);
		targetLine = GameObject.Find ("Temp Battle Target Line").GetComponent<MeshRenderer> ();
		targetLine.enabled = false;
		targetLine.transform.localPosition = new Vector3(0, -2, SPAWN_DEPTH);

		//Add the divider
		divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
		divider.transform.position = parentCam.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, SPAWN_DEPTH));
		divider.transform.localScale = new Vector3(0.1f, 100f, 1f);
		divider.GetComponent<MeshRenderer>().enabled = false;
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

		Song song = ServiceFactory.Instance.Resolve<Song>();

		float CENTER_MARGIN = Screen.width * 2 / 27f;
		float UNIT_MARGIN = Screen.width * 3.5f / 27f;
		float FRET_RANGE = Screen.width / 3f; //TODO: Change based on number of players
		float SPAWN_HEIGHT = Screen.height;

		targetLine.enabled = true;
		divider.GetComponent<MeshRenderer>().enabled = true;
		Debug.Assert (players.ContainsKey (m.AttackingUnit.PlayerNumber));
		Debug.Assert (players.ContainsKey (m.DefendingUnit.PlayerNumber));
		// Prepare battle data per player
		attacker = players [m.AttackingUnit.PlayerNumber];
		defender = players [m.DefendingUnit.PlayerNumber];

		attacker.playerNumber = m.AttackingUnit.PlayerNumber;
		defender.playerNumber = m.DefendingUnit.PlayerNumber;

		attacker.unit = m.AttackingUnit;
		defender.unit = m.DefendingUnit;

		attacker.instrumentID = 0; // Edgy synth
		defender.instrumentID = 1; // Smoother synth

		attacker.battleNotes = song.GetNextBattleNotes (battleMeasures, attacker.instrumentID, attacker.difficulty);
		defender.battleNotes = song.GetNextBattleNotes (battleMeasures, defender.instrumentID, defender.difficulty);

		attacker.battleNoteStates = new int[attacker.battleNotes.Length];
		defender.battleNoteStates = new int[defender.battleNotes.Length];

		List<int> OrderedPlayers = players.Keys.ToList();
		OrderedPlayers.Sort();

		float PlayerXPos = UNIT_MARGIN;

		float currentMusicTime = song.playerPosition;
		// Spawn notes:
		foreach (int playerNumber in OrderedPlayers) {
			foreach (Note note in players[playerNumber].battleNotes) {
				GameObject spawnedNote = GameObjectUtil.Instantiate(notePrefab);
				spawnedNote.transform.parent = parentCam.transform;
				spawnedNote.transform.position = parentCam.ScreenToWorldPoint(
					new Vector3(PlayerXPos + ((note.fretNumber+1) * FRET_RANGE / 5), SPAWN_HEIGHT, SPAWN_DEPTH));


				NoteObject NoteObject = spawnedNote.GetComponent<NoteObject> ();
				NoteObject.SetNoteColor(note);

				float heightOffset = (note.getPositionTime(song.bpm) - currentMusicTime);
				while (heightOffset < 0) {
					heightOffset += song.totalSeconds;
				}

				Vector3 DistanceToTarget = new Vector3(0f, spawnedNote.transform.position.y - targetLine.transform.position.y, 0f);
				Vector3 StartingOffset = ((1 / Time.fixedDeltaTime) * heightOffset * NoteObject.velocity * -1)
					- DistanceToTarget - NoteObject.centerOfObject; //MATH BITCHES
				spawnedNote.transform.position += StartingOffset;

			}
			PlayerXPos += FRET_RANGE + CENTER_MARGIN;
		}

		isInBattle = true;
		battleProgressInMeasures = 0;
	}

	void Update() {
		if (isInBattle) {
			MarkPassedNotes(attacker);
			MarkPassedNotes(defender);
		}
	}

	void OnBeatCenter(BeatCenterMessage m) {
		// After the final beat of a battle sequence is played, trigger EndBattle
		if (isInBattle && m.BeatNumber == 0) {
			battleProgressInMeasures++;
		}
		if (isInBattle && m.BeatNumber == m.BeatsPerMeasure - 1 && battleProgressInMeasures == battleMeasures) {
			// Delay by half beat to allow any final sixteenth notes to be played
			StartCoroutine(EndBattleCoroutine(60f / m.BeatsPerMinute * 7 / 8));
		}
	}

	IEnumerator EndBattleCoroutine(float delay) {
		// Delay by half beat to allow any final eigth notes to be played
		// TODO: Figure out if we can penalize spamming strum to hit every note
		yield return new WaitForSeconds(delay);
		targetLine.enabled = false;
		divider.GetComponent<MeshRenderer>().enabled = false;
		isInBattle = false;
		int attackerHitCount = 0;
		foreach (int i in attacker.battleNoteStates) {
			if (i == 1)
				attackerHitCount++;
		}
		int defenderHitCount = 0;
		foreach (int i in defender.battleNoteStates) {
			if (i == 1)
				defenderHitCount++;
		}
		messageRouter.RaiseMessage (new ExitBattleMessage () {
			AttackingUnit = attacker.unit,
			DefendingUnit = defender.unit,
			AttackerHitPercent = (float) attackerHitCount / attacker.battleNotes.Length,
			DefenderHitPercent = (float) defenderHitCount / defender.battleNotes.Length
		});
	}

	protected void OnButtonDown(ButtonDownMessage m) {
		// Check if when the player strums that they actually hit something
		if (!isInBattle)
			return;

		switch (m.Button) {
		case InputButton.STRUM:
			InputButton[] frets = Interpreter.HeldFrets.ContainsKey (m.PlayerNumber) ? 
				Interpreter.HeldFrets [m.PlayerNumber].ToArray () : new InputButton[]{ };
			Note[] hitNotes = ServiceFactory.Instance.Resolve<Song>().GetHitNotes (players [m.PlayerNumber].instrumentID, players [m.PlayerNumber].difficulty, frets);
			// TODO: Add support for HOPOs
			bool noteWasHit = false;
			// Go through the possible notes that the player could have been trying to hit
			GameObject[] noteObjects = GameObject.FindGameObjectsWithTag("noteObject");
			foreach (Note n in hitNotes) { // Warning! O(n^2)
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
				if (noteWasHit) {
					// TODO: Make this less computationally expensive. Bad search.
					foreach (GameObject no in noteObjects) {
						NoteObject noc = no.GetComponent<NoteObject> ();
						if (noc.NoteData.Equals(n)) {
							GameObjectUtil.Destroy (no);
						}
					}
					break;
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

	void OnBattleDifficultyChange(BattleDifficultyChangeMessage m) {
		if (!players.ContainsKey (m.PlayerNumber)) {
			players.Add (m.PlayerNumber, new PlayerBattleData ());
		}
		players [m.PlayerNumber].difficulty = m.Difficulty;
	}

	void MarkPassedNotes(PlayerBattleData player) {
		Note[] passedNotes = ServiceFactory.Instance.Resolve<Song>().GetPassedNotes(player.instrumentID, player.difficulty, Time.deltaTime);

		foreach (Note n in passedNotes) {
			int i = Array.IndexOf(players[player.playerNumber].battleNotes, n);
			if (i >= 0 && players[player.playerNumber].battleNoteStates[i] != 1) {
				players[player.playerNumber].battleNoteStates[i] = -1;
			}
		}
	}
}
