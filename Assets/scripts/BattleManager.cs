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
	public BattleType battleType;
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
	public BattleType battleType;
}

/// <summary>
/// Sent when a player wishes to change their difficulty level (Guitar Hero - like)
/// </summary>
public class BattleDifficultyChangeMessage {
	public int PlayerNumber {get; set;}
	public int Difficulty {get; set;}
}

/// <summary>
/// Battle encounters can have different types depending on what kind of skill-based action is being performed.
/// </summary>
public enum BattleType {
	ATTACK, HEAL
}

public class BattleManager : MonoBehaviour {

	/// <summary>
	/// Player data that persists outside of the context of the battle
	/// </summary>
	private class AllPlayersData {
		public int difficulty;
		public int playerNumber;
		public bool hitLastNote;
	}

	/// <summary>
	/// Ephemeral data for a player currently in-battle, either on the left or right of the screen
	/// </summary>
	private class PlayerBattleData {
		public bool isOnLeft; // How to tell which side of the screen the player is on for this battle
		public int playerNumber;
		public bool isAttacker; // How to tell which player is the attacker / defender
		public int instrumentID { get; set; }
		public Note[] battleNotes { get; set; }
		public int[] battleNoteStates { get; set; }
		public MelodyUnit unit {get; set;}
		public GameObject battleSprite { get; set; }
	}

	private Dictionary<int, AllPlayersData> allPlayers; // Track all players in the game
	private List<PlayerBattleData> playersInBattle; // Temporary list that is refreshed at the start of each battle
	private MessageRouter messageRouter;
	private bool isInBattle;
	private BattleType battleType;
	private int battleProgressInMeasures; // Number of measures into the battle
	private MeshRenderer targetLine; // Renderer for note targets. Currently a temporary black line.
	private GameObject divider; // Divider between player's notes
	public  GameObject targetPrefab; // Single target object
	private GameObject[] targets;

	private float UNIT_MARGIN;

	public int battleMeasures = 1;
	public Camera parentCam;
	public GameObject notePrefab;
	public Vector2 velocityRange = new Vector2 (0.12f, 0.2f);

	//Constants
	private const float SPAWN_DEPTH = 13f;

	void Awake() {
		allPlayers = new Dictionary<int, AllPlayersData> ();
		playersInBattle = new List<PlayerBattleData> (2);
	}

	void Start() {
		ServiceFactory.Instance.RegisterSingleton<BattleManager> (this);
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<EnterBattleMessage> (OnStartBattle);
		messageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		messageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		messageRouter.AddHandler<BeatCenterMessage> (OnBeatCenter);
		messageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
		messageRouter.AddHandler<BattleDifficultyChangeMessage> (OnBattleDifficultyChange);
		messageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
		targetLine = GameObject.Find ("Temp Battle Target Line").GetComponent<MeshRenderer> ();
		targetLine.enabled = false;
		targetLine.transform.localPosition = new Vector3(0, -5, SPAWN_DEPTH);

		//Add the divider
		divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
		divider.layer = LayerMask.NameToLayer ("BattleLayer");
		divider.transform.localScale = new Vector3(0.1f, 100f, 1f);
		divider.transform.position = parentCam.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, SPAWN_DEPTH));
		divider.GetComponent<MeshRenderer>().enabled = false;

		// Add targets
		targets = new GameObject[10];
		for (int i = 0; i < 2; i++) {
			for (int j = 0; j < 5; j++) {
				GameObject t = GameObjectUtil.Instantiate (targetPrefab);
				t.transform.parent = targetLine.transform;
				Target tc = t.GetComponent<Target> ();
				tc.player = i;
				tc.color = (InputButton)j;
				targets [i * 5 + j] = t;
			}
		}
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		messageRouter.RemoveHandler<EnterBattleMessage> (OnStartBattle);
		messageRouter.RemoveHandler<ButtonDownMessage> (OnButtonDown);
		messageRouter.RemoveHandler<ButtonUpMessage> (OnButtonUp);
		messageRouter.RemoveHandler<BeatCenterMessage> (OnBeatCenter);
		messageRouter.RemoveHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
		messageRouter.RemoveHandler<BattleDifficultyChangeMessage> (OnBattleDifficultyChange);
		messageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnStartBattle(EnterBattleMessage m) {

		// Constants
		float CENTER_MARGIN = Screen.width * 2 / 27f;
		UNIT_MARGIN = Screen.width * 3.5f / 27f;
		float FRET_RANGE = Screen.width / 3f; //TODO: Change based on number of players
		float SPAWN_HEIGHT = Screen.height;

		Song song = ServiceFactory.Instance.Resolve<Song>();

		// Reposition targets
		float targetXPos = UNIT_MARGIN + FRET_RANGE/10;
		for (int i = 0; i < 2; i++, targetXPos += CENTER_MARGIN) {
			for (int j = 0; j < 5; j++, targetXPos += FRET_RANGE/5) {
				GameObject t = targets [i * 5 + j];
				Vector3 screenPosition = parentCam.ScreenToWorldPoint (new Vector3 (targetXPos, 0, SPAWN_DEPTH));
				t.transform.position = new Vector3 (screenPosition.x, targetLine.transform.position.y, screenPosition.z);
			}
		}

		targetLine.enabled = true;
		divider.GetComponent<MeshRenderer>().enabled = true;

		// If we are not aware of these players, start tracking them:
		if (!allPlayers.ContainsKey (m.AttackingUnit.PlayerNumber)) { // Track attacking unit
			allPlayers.Add (m.AttackingUnit.PlayerNumber, new AllPlayersData () {
				playerNumber = m.AttackingUnit.PlayerNumber,
				difficulty = 2,
				hitLastNote = true
			});
		}
		if (!allPlayers.ContainsKey (m.DefendingUnit.PlayerNumber)) { // Track defending unit
			allPlayers.Add (m.DefendingUnit.PlayerNumber, new AllPlayersData () {
				playerNumber = m.DefendingUnit.PlayerNumber,
				difficulty = 2,
				hitLastNote = true
			});
		}

		// Prepare battle data per player
		playersInBattle.Clear();
		playersInBattle.Add (new PlayerBattleData () { // Add attacking unit
			isOnLeft = m.AttackingUnit.PlayerNumber == m.DefendingUnit.PlayerNumber ? 
				m.AttackingUnit.PlayerNumber == 0 : 
				m.AttackingUnit.PlayerNumber < m.DefendingUnit.PlayerNumber, 
			playerNumber = m.AttackingUnit.PlayerNumber,
			isAttacker = true,
			instrumentID = 0,
			battleNotes = song.GetNextBattleNotes (battleMeasures, 0, allPlayers [m.AttackingUnit.PlayerNumber].difficulty),
			unit = m.AttackingUnit
		});
		playersInBattle.Add (new PlayerBattleData () { // Add defending unit
			isOnLeft = m.AttackingUnit.PlayerNumber == m.DefendingUnit.PlayerNumber ? 
				m.DefendingUnit.PlayerNumber != 0 : 
				m.DefendingUnit.PlayerNumber < m.AttackingUnit.PlayerNumber, 
			playerNumber = m.DefendingUnit.PlayerNumber,
			isAttacker = false,
			instrumentID = 1,
			battleNotes = m.battleType == BattleType.HEAL ? new Note[]{ } :
				song.GetNextBattleNotes (battleMeasures, 1, allPlayers [m.DefendingUnit.PlayerNumber].difficulty),
			unit = m.DefendingUnit
		});
		foreach (PlayerBattleData d in playersInBattle) {
			d.battleNoteStates = new int[d.battleNotes.Length];
		}

		// Add units to sides of battle:
		RenderBattleSprites();

		float currentMusicTime = song.playerPosition;
		// Spawn notes:
		foreach (PlayerBattleData player in playersInBattle) {
			// Decide X offset based on which side the player being looped is on:
			float PlayerXPos = player.isOnLeft ? UNIT_MARGIN - FRET_RANGE / 10 : UNIT_MARGIN + FRET_RANGE * 9 / 10 + CENTER_MARGIN;

			float velocity = Math.Abs (velocityRange.x + (velocityRange.y - velocityRange.x) * allPlayers [player.playerNumber].difficulty / 2);
			foreach (Note note in player.battleNotes) {
				GameObject spawnedNote = GameObjectUtil.Instantiate(notePrefab);
				spawnedNote.transform.parent = parentCam.transform;
				spawnedNote.transform.position = parentCam.ScreenToWorldPoint(
					new Vector3(PlayerXPos + ((note.fretNumber+1) * FRET_RANGE / 5), SPAWN_HEIGHT, SPAWN_DEPTH));


				NoteObject NoteObject = spawnedNote.GetComponent<NoteObject> ();
				NoteObject.SetNoteColor(note);

				NoteObject.velocity = new Vector3 (0, -velocity, 0);

				float heightOffset = (note.getPositionTime(song.bpm) - currentMusicTime);
				while (heightOffset < 0) {
					heightOffset += song.totalSeconds;
				}

				Vector3 DistanceToTarget = new Vector3(0f, spawnedNote.transform.position.y - targetLine.transform.position.y, 0f);
				Vector3 StartingOffset = ((1 / Time.fixedDeltaTime) * heightOffset * NoteObject.velocity * -1)
					- DistanceToTarget - NoteObject.centerOfObject; //MATH BITCHES
				spawnedNote.transform.position += StartingOffset;

			}
		}

		isInBattle = true;
		battleType = m.battleType;
		battleProgressInMeasures = 0;
	}

	void Update() {
		if (isInBattle) {
			foreach (PlayerBattleData d in playersInBattle) {
				MarkPassedNotes (d);
			}
		}
	}

	void OnExitBeatWindow(ExitBeatWindowMessage m) {
		//this is where we will make a sprite in the fighting scene dance
		//attacker_unit.GetComponentInChildren<Animator>().SetTrigger("beat");
		//defender_unit.GetComponentInChildren<Animator>().SetTrigger("beat");
		if (isInBattle && m.BeatNumber == m.BeatsPerMeasure - 1) {
			battleProgressInMeasures++;
		}
	}

	void OnBeatCenter(BeatCenterMessage m) {
		// After the final beat of a battle sequence is played, trigger EndBattle
		if (isInBattle && m.BeatNumber == m.BeatsPerMeasure - 1 && battleProgressInMeasures == battleMeasures) {
			// Delay by half beat to allow any final sixteenth notes to be played
			StartCoroutine(EndBattleCoroutine(60f / m.BeatsPerMinute));
		}
	}

	IEnumerator EndBattleCoroutine(float delay) {
		// Delay by half beat to allow any final eigth notes to be played
		// TODO: Figure out if we can penalize spamming strum to hit every note
		yield return new WaitForSeconds(delay);
		targetLine.enabled = false;
		isInBattle = false;
		divider.GetComponent<MeshRenderer>().enabled = false;

		// Remove side-sprites:
		foreach (PlayerBattleData d in playersInBattle) {
			GameObjectUtil.Destroy (d.battleSprite);
		}

		// Count up hit notes:
		Dictionary<int, int> hitCount = new Dictionary<int, int> ();
		foreach (PlayerBattleData d in playersInBattle) {
			if (!hitCount.ContainsKey (d.playerNumber)) {
				hitCount.Add (d.playerNumber, 0);
			}
			foreach (int i in d.battleNoteStates) {
				if (i == 1) {
					hitCount [d.playerNumber]++;
				}
			}
		}

		// Send end battle message:
		PlayerBattleData attacker = playersInBattle.Find (c => c.isAttacker);
		PlayerBattleData defender = playersInBattle.Find (c => !c.isAttacker);
		messageRouter.RaiseMessage (new ExitBattleMessage () {
			AttackingUnit = attacker.unit,
			DefendingUnit = defender.unit,
			AttackerHitPercent = attacker.battleNotes.Length == 0 ? 0 : 
				(float) hitCount[attacker.playerNumber] / attacker.battleNotes.Length,
			DefenderHitPercent = defender.battleNotes.Length == 0 ? 0 : 
				(float) hitCount[defender.playerNumber] / defender.battleNotes.Length,
			battleType = battleType
		});
	}

	protected void OnButtonDown(ButtonDownMessage m) {
		// Check if when the player strums that they actually hit something
		if (!isInBattle)
			return;
		int maxFret = int.MinValue;
		switch (m.Button) {
		case InputButton.STRUM:
			tryHitNote (true, m.PlayerNumber);
			break;
		case InputButton.GREEN:
			maxFret = 0;
			break;
		case InputButton.RED:
			maxFret = 1;
			break;
		case InputButton.YELLOW:
			maxFret = 2;
			break;
		case InputButton.BLUE:
			maxFret = 3;
			break;
		case InputButton.ORANGE:
			maxFret = 4;
			break;
		default:
			break;
		}
		if (maxFret >= 0 && allPlayers [m.PlayerNumber].hitLastNote) {
			InputButton[] frets = Interpreter.HeldFrets.ContainsKey (m.PlayerNumber) ? 
				Interpreter.HeldFrets [m.PlayerNumber].ToArray () : new InputButton[]{ };
			foreach (InputButton b in frets) {
				if ((int)b > maxFret) {
					return;
				}
			}
			if (!tryHitNote (false, m.PlayerNumber, maxFret)) {
				allPlayers [m.PlayerNumber].hitLastNote = false;
			}
		}
	}

	void OnButtonUp(ButtonUpMessage m) {
		// Check if when the player strums that they actually hit something
		if (!isInBattle)
			return;
		if (!allPlayers [m.PlayerNumber].hitLastNote)
			return;
		switch (m.Button) {
		case InputButton.GREEN:
		case InputButton.RED:
		case InputButton.YELLOW:
		case InputButton.BLUE:
		case InputButton.ORANGE:
			int maxFret = int.MinValue;
			InputButton[] frets = Interpreter.HeldFrets.ContainsKey (m.PlayerNumber) ? 
				Interpreter.HeldFrets [m.PlayerNumber].ToArray () : new InputButton[]{ };
			foreach (InputButton b in frets) {
				if (b != m.Button && (int)b > maxFret) {
					maxFret = (int)b;
				}
			}
			if (maxFret >= 0) {
				tryHitNote (false, m.PlayerNumber, maxFret);
			}
			break;
		default:
			break;
		}
	}

	bool tryHitNote(bool isStrum, int playerNumber, int hopoValue = -1) {
		PlayerBattleData player = playersInBattle.Find (c => c.playerNumber == playerNumber);
		if (player == null)
			return false;
		InputButton[] frets = !isStrum ? new InputButton[]{ (InputButton)hopoValue } : 
			Interpreter.HeldFrets.ContainsKey (playerNumber) ? 
			Interpreter.HeldFrets [playerNumber].ToArray () : new InputButton[]{ };
		Note[] hitNotes = ServiceFactory.Instance.Resolve<Song> ()
			.GetHitNotes (player.instrumentID, allPlayers [playerNumber].difficulty, frets);
		bool noteWasHit = false;
		// Go through the possible notes that the player could have been trying to hit
		GameObject[] noteObjects = GameObject.FindGameObjectsWithTag("noteObject");
		foreach (Note n in hitNotes) { // Warning! O(n^2)
			for (int i = 0; i < player.battleNotes.Length; i++) {
				if (n.Equals (player.battleNotes [i])) {
					if (player.battleNoteStates [i] == 0) {
						if (!isStrum && !n.isHOPO)
							continue;
						player.battleNoteStates [i] = 1;
						noteWasHit = true;
						allPlayers [playerNumber].hitLastNote = true;
						messageRouter.RaiseMessage (new NoteHitMessage () {
							PlayerNumber = playerNumber,
							InstrumentID = player.instrumentID
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
		if (!noteWasHit && isStrum) {
			allPlayers [playerNumber].hitLastNote = false;
			messageRouter.RaiseMessage (new NoteMissMessage () {
				PlayerNumber = playerNumber,
				InstrumentID = player.instrumentID
			});
		}
		return noteWasHit;
	}

	void OnBattleDifficultyChange(BattleDifficultyChangeMessage m) {
		if (!allPlayers.ContainsKey (m.PlayerNumber)) {
			allPlayers.Add (m.PlayerNumber, new AllPlayersData () {
				playerNumber = m.PlayerNumber,
				difficulty = m.Difficulty,
				hitLastNote = true
			});
		} else {
			allPlayers [m.PlayerNumber].difficulty = m.Difficulty;
		}
	}

	void MarkPassedNotes(PlayerBattleData player) {
		Note[] passedNotes = ServiceFactory.Instance.Resolve<Song> ().GetPassedNotes (player.instrumentID, 
			                     allPlayers [player.playerNumber].difficulty, Time.deltaTime);

		foreach (Note n in passedNotes) {
			int i = Array.IndexOf(player.battleNotes, n);
			if (i >= 0 && player.battleNoteStates[i] != 1) {
				player.battleNoteStates[i] = -1;
				allPlayers [player.playerNumber].hitLastNote = false;
				messageRouter.RaiseMessage (new NoteMissMessage () {
					PlayerNumber = player.playerNumber,
					InstrumentID = player.instrumentID
				});
			}
		}
	}

	public int getPlayerDifficulty(int playerNumber) {
		return allPlayers [playerNumber].difficulty;
	}
	
	void RenderBattleSprites() {
		foreach (PlayerBattleData player in playersInBattle) {
			player.battleSprite = GameObjectUtil.Instantiate (player.unit.transform.FindChild ("RotatedVisual").gameObject);
			player.battleSprite.layer = LayerMask.NameToLayer("BattleLayer");
			player.battleSprite.transform.parent = parentCam.transform;
			if (player.isOnLeft) {
				player.battleSprite.transform.position = 
					parentCam.ScreenToWorldPoint (new Vector3 (UNIT_MARGIN / 2, Screen.height / 2, SPAWN_DEPTH / 2));
			} else {
				player.battleSprite.transform.position = 
					parentCam.ScreenToWorldPoint (new Vector3 (Screen.width - (UNIT_MARGIN / 2), Screen.height / 2, 
					SPAWN_DEPTH / 2));
			}
			player.battleSprite.transform.eulerAngles = new Vector3(0, 180);
		}
	}
}
