using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using System.IO;
using System;

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
	public DefaultAsset songData;

	private MessageRouter MessageRouter;
	private AudioSource player;
	private AudioSource[] instrumentPlayers;
	private Note[] notes;

	// Use this for initialization
	void Start () {
		ServiceFactory.Instance.RegisterSingleton<Song> (this); // Any other class can reference this one
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
		notes = LoadSongData (songData);
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

	/// <summary>
	/// Gets a list of notes for a particular part that fall between two timecodes in the song
	/// </summary>
	/// <returns>The notes.</returns>
	/// <param name="instrumentID">Instrument</param>
	/// <param name="difficulty">Difficulty (0-2)</param>
	/// <param name="startTime">Start time (seconds)</param>
	/// <param name="endTime">End time (seconds)</param>
	public Note[] GetNotes(int instrumentID, int difficulty, float startTime, float endTime) {
		// TODO: Can be majorly optimized by using a better data structure or search method
		List<Note> ret = new List<Note>();
		foreach (Note cur in notes) {
			// note that 360 is the MIDI ticks for a quarter note
			float time = cur.position / 360f / bpm * 60f;
			if (time >= startTime && time < endTime && cur.difficulty == difficulty && 
				cur.instrumentID == instrumentID) {
				ret.Add (cur);
			}
		}
		return ret.ToArray ();
	}

	/// <summary>
	/// Gets a list of notes for a particular part that fall between two beat indices in the song
	/// </summary>
	/// <returns>The notes.</returns>
	/// <param name="instrumentID">Instrument</param>
	/// <param name="difficulty">Difficulty (0-2)</param>
	/// <param name="startBeat">Start beat (beats since beginning of song)</param>
	/// <param name="endBeat">End beat (beats since beginning of song)</param>
	public Note[] GetNotes(int instrumentID, int difficulty, int startBeat, int endBeat) {
		// TODO: Can be majorly optimized by using a better data structure or search method
		List<Note> ret = new List<Note>();
		foreach (Note cur in notes) {
			// note that 360 is the MIDI ticks for a quarter note
			int beat = cur.position / 360;
			if (beat >= startBeat && beat < endBeat && cur.difficulty == difficulty && 
				cur.instrumentID == instrumentID) {
				ret.Add (cur);
			}
		}
		return ret.ToArray ();
	}

	/// <summary>
	/// Gets a list of notes for a particular part
	/// </summary>
	/// <returns>The notes.</returns>
	/// <param name="instrumentID">Instrument</param>
	/// <param name="difficulty">Difficulty (0-2)</param>
	public Note[] GetNotes(int instrumentID, int difficulty) {
		// TODO: Can be majorly optimized by using a better data structure
		List<Note> ret = new List<Note>();
		foreach (Note cur in notes) {
			if (cur.difficulty == difficulty && cur.instrumentID == instrumentID) {
				ret.Add (cur);
			}
		}
		return ret.ToArray ();
	}

	/// <summary>
	/// Gets a list of all notes for all parts and difficulties
	/// </summary>
	/// <returns>The notes.</returns>
	public Note[] GetNotes() {
		Note[] ret = new Note[notes.Length];
		Array.Copy (notes, ret, notes.Length);
		return ret;
	}

	/// <summary>
	/// Given a MIDI file, return a list of Note objects contained in the MIDI file (see Note, defined above)
	/// </summary>
	/// <returns>The song data, as a list of Note objects</returns>
	/// <param name="songData">Song data, as a MIDI file asset</param>
	private Note[] LoadSongData(DefaultAsset songData) {
		List<Note> ret = new List<Note> (1000);
		string dataPath = AssetDatabase.GetAssetPath (songData);
		byte[] buffer = new byte[2048];
		using (Stream source = File.OpenRead (dataPath)) {
			int timeCode = 0;
			int statusCode = 0;
			int dataIndex = 0;
			bool awaitingStatus = true;
			int deltaTime = 0; // buffer for delta time bytes that can arise between events
			Note[] activeNotes = new Note[127]; // buffer for notes that are awaiting a "note off"
			byte currentNote = 0; // Note value of last note read
			byte timeComponent = 0;
			int bytesRead = 0;
			while ((bytesRead = source.Read (buffer, 0, buffer.Length)) > 0) {
				for (int i = 0; i < bytesRead; i++) {
					if ((buffer [i] & 0x80) > 0 && awaitingStatus) { // Byte is a status message
						statusCode = buffer[i];
						dataIndex = 0;
						awaitingStatus = false;
					} else { // Byte is data
						awaitingStatus = false;
						switch (statusCode & 0xF0) {
						case 0x90: // Note On
							switch (dataIndex) {
							case 0: // Note value
								currentNote = buffer [i];
								activeNotes [currentNote] = InterpretMIDINote (currentNote);
								activeNotes [currentNote].position = timeCode;
								ret.Add (activeNotes [currentNote]);
								break;
							case 1: // Note velocity
								activeNotes [currentNote].velocity = buffer [i];
								break;
							default: // Delta time
								timeComponent = buffer [i];
								deltaTime <<= 7;
								deltaTime |= timeComponent & 0x7F;
								if ((timeComponent & 0x80) == 0) { // Terminal byte
									dataIndex = -1; // Would be 0, but is incremented outside switch. Hacky much.
									awaitingStatus = true;
									timeCode += deltaTime;
									deltaTime = 0;
								}
								break;
							}
							break;
						case 0x80: // Note Off
							switch (dataIndex) {
							case 0: // Note value
								currentNote = buffer [i];
								activeNotes [currentNote].duration = timeCode - activeNotes [currentNote].position;
								break;
							case 1: // Note release velocity
								break;
							default: // Delta time
								timeComponent = buffer [i];
								deltaTime <<= 7;
								deltaTime |= timeComponent & 0x7F;
								if ((timeComponent & 0x80) == 0) { // Terminal byte
									dataIndex = -1; // Would be 0, but is incremented outside switch. Hacky much.
									awaitingStatus = true;
									timeCode += deltaTime;
									deltaTime = 0;
								}
								break;
							}
							break;
						default: // A status that we don't care about. On to the next!
							awaitingStatus = true;
							break;
						}
						dataIndex++;
					}

				}
			}
		}
		return ret.ToArray ();
	}

	Note InterpretMIDINote(int noteValue) {
		int noteColor = noteValue % 5;
		int row = noteValue / 5;
		int difficulty = row % 3;
		int instrument = row / 3;
		return new Note () {
			fretNumber = noteColor,
			instrumentID = instrument,
			difficulty = difficulty
		};  // position, duration and velocity are unknown at this point
	}
}
