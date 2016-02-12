using UnityEngine;
using System.Collections;
using Frictionless;

/// <summary>
/// Sent slightly ahead of BeatCenterMessage
/// </summary>
public class EnterBeatWindowMessage {}

/// <summary>
/// Sent slightly behind BeatCenterMessage
/// </summary>
public class ExitBeatWindowMessage {}

/// <summary>
/// Sent on each beat of the song
/// </summary>
public class BeatCenterMessage {
	/// <summary>
	/// The index of the beat in a musical measure. For a typical song, this will be in the range [0, 3]
	/// </summary>
	public int BeatNumber { get; set; }

	/// <summary>
	/// The number of beats in a musical measure. Usually 4. Helpful to be able to tell when to switch player turns.
	/// </summary>
	public int BeatsPerMeasure { get; set; }

	/// <summary>
	/// The tempo of the music in beats per minute. Can be used to calculate when the next beat is coming.
	/// </summary>
	public float BeatsPerMinute { get; set; }
}

public class Song : MonoBehaviour {

	public float bpm = 80f;
	public int beatsPerMeasure = 4;
	public float window = 0.5f;
	public AudioClip songFile;
	public AudioClip[] instrumentFiles;
	public float offset = 0f;

	private MessageRouter MessageRouter;
	private AudioSource player;
	private AudioSource[] instrumentPlayers;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<NoteHitMessage> (OnNoteHit);
		MessageRouter.AddHandler<NoteMissMessage> (OnNoteMiss);
		player = gameObject.AddComponent<AudioSource> ();
		player.clip = songFile;
		player.loop = true;
		instrumentPlayers = new AudioSource[instrumentFiles.Length];
		for (int i = 0; i < instrumentPlayers.Length; i++) {
			instrumentPlayers[i] = gameObject.AddComponent<AudioSource> ();
			instrumentPlayers [i].clip = instrumentFiles [i];
			instrumentPlayers [i].loop = true;
			// TODO: Ensure that all playing audio files stay perfectly in sync
			instrumentPlayers [i].mute = true;
		}
		player.Play ();
		for (int i = 0; i < instrumentPlayers.Length; i++) {
			instrumentPlayers [i].Play ();
		}
		StartCoroutine ("BeatCoroutine");
	}
	
	void OnDisable() {
		StopCoroutine ("BeatCoroutine");
	}

	private IEnumerator BeatCoroutine() {
		int beatsCounter = 0;
		float previousTime = 0f;
		float currentTime = 0f;
		while (true) {
			currentTime = player.time;
			if (currentTime < previousTime) {
				beatsCounter = 0;
			}
			previousTime = currentTime;
			if (currentTime > 60f / bpm * beatsCounter) {
				MessageRouter.RaiseMessage (new BeatCenterMessage () {
					BeatNumber = beatsCounter % beatsPerMeasure, 
					BeatsPerMeasure = beatsPerMeasure, 
					BeatsPerMinute = bpm
				});
				beatsCounter++;
				yield return new WaitForSeconds (60f / bpm * window / 2);
				MessageRouter.RaiseMessage (new ExitBeatWindowMessage ());
				yield return new WaitForSeconds (60f / bpm * (1 - window));
				MessageRouter.RaiseMessage (new EnterBeatWindowMessage ());
			}
			yield return null;
		}
	}

	void OnEnterBattle(EnterBattleMessage m) {
		for (int i = 0; i < instrumentPlayers.Length; i++) {
			instrumentPlayers [i].mute = false;
		}
	}

	void OnExitBattle(ExitBattleMessage m) {
		for (int i = 0; i < instrumentPlayers.Length; i++) {
			instrumentPlayers [i].mute = true;
		}
	}

	void OnNoteHit(NoteHitMessage m) {
		Debug.Assert (instrumentPlayers.Length > m.InstrumentID);
		instrumentPlayers [m.InstrumentID].mute = false;
	}

	void OnNoteMiss(NoteMissMessage m) {
		Debug.Assert (instrumentPlayers.Length > m.InstrumentID);
		instrumentPlayers [m.InstrumentID].mute = true;
	}
}
