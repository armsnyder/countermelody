using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using System.IO;
using System;

public abstract class GenericBeatMessage {
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

/// <summary>
/// Sent slightly ahead of BeatCenterMessage
/// </summary>
public class EnterBeatWindowMessage : GenericBeatMessage {}

/// <summary>
/// Sent slightly behind BeatCenterMessage
/// </summary>
public class ExitBeatWindowMessage : GenericBeatMessage {}

/// <summary>
/// Sent on each beat of the song
/// </summary>
public class BeatCenterMessage : GenericBeatMessage {
	
}

public class Note {
	public int position { get; set; } // Number of MIDI ticks into the song the note occurs
	public int instrumentID { get; set; } // Which musical track the note belongs to
	public int difficulty { get; set; } // Easy = 0, medium = 1, hard = 2
	public int duration { get; set; } // Number of position units the note lasts (120 = 16th note)
	public int velocity { get; set; } // Extraneous MIDI information we might use late on, but for now is always 127
	public int fretNumber { get; set; } // Number of fret (0 = green, 4 = orange)
	public InputButton fretColor
	{
		get
		{
			return fretNumber == 0 ? InputButton.GREEN : fretNumber == 1 ?
				InputButton.RED : fretNumber == 2 ? InputButton.YELLOW : fretNumber == 3 ?
				InputButton.BLUE : InputButton.ORANGE;
		}
	} // Color of fret as InputButton
	public bool isHOPO { get { return velocity < 127; } } // If true, strum not required to hit note
	public float getPositionTime(float bpm) {
		return (float) position / ppq / bpm * 60f;
	}
	public int ppq {get; set;} // A technical MIDI term for ticks per beat
}

public class Song : MonoBehaviour {

	public float bpm = 80f;
	public int beatsPerMeasure = 4;
	public float window = 0.75f; // Percent of a beat where the player is allowed to perform an action
	public float battleWindow = 0.5f; // Percent of a beat where player can play a note during battle
	public AudioClip songFile;
	public AudioClip[] instrumentFiles;
	public float offset = 0f;
	public TextAsset songData;

	public float playerPosition { get { return player.time; } }
	public int totalBeats { get { return (int)Math.Round (player.clip.length / 60f * bpm); } }
	public float totalSeconds { get { return player.clip.length; } }

	private MessageRouter MessageRouter;
	private AudioSource player;
	private AudioSource[] instrumentPlayers;
	private Note[] notes;
	private List<List<List<Note>>> sortedNotes;  // Sorted by instrument, then difficulty, then time
	private bool startMusicNextMeasure;  // If true, battle is about to begin

	// Use this for initialization
	void Start () {
		ServiceFactory.Instance.RegisterSingleton<Song> (this); // Any other class can reference this one
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<NoteHitMessage> (OnNoteHit);
		MessageRouter.AddHandler<NoteMissMessage> (OnNoteMiss);
		MessageRouter.AddHandler<BeatCenterMessage> (OnBeatCenterMessage);
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
		sortedNotes = PresortNotes (notes);
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
				// Continue to increment beat until we have caught up
				while (currentTime > 60f / bpm * beatsCounter) {
					beatsCounter++;
				}
				MessageRouter.RaiseMessage (new BeatCenterMessage () {
					BeatNumber = (beatsCounter - 1) % beatsPerMeasure, 
					BeatsPerMeasure = beatsPerMeasure, 
					BeatsPerMinute = bpm
				});
				yield return new WaitForSeconds (60f / bpm * window / 2);
				MessageRouter.RaiseMessage (new ExitBeatWindowMessage () {
					BeatNumber = (beatsCounter - 1) % beatsPerMeasure, 
					BeatsPerMeasure = beatsPerMeasure, 
					BeatsPerMinute = bpm
				});
				yield return new WaitForSeconds (60f / bpm * (1 - window));
				MessageRouter.RaiseMessage (new EnterBeatWindowMessage () {
					BeatNumber = (beatsCounter) % beatsPerMeasure, 
					BeatsPerMeasure = beatsPerMeasure, 
					BeatsPerMinute = bpm
				});
			}
			yield return null;
		}
	}

	void OnEnterBattle(EnterBattleMessage m) {
		startMusicNextMeasure = true;
	}

	void OnExitBattle(ExitBattleMessage m) {
		for (int i = 0; i < instrumentPlayers.Length; i++) {
			instrumentPlayers [i].mute = true;
		}
	}

	void OnBeatCenterMessage(BeatCenterMessage m) {
		if (startMusicNextMeasure) {
			// If battle about to start and a new measure begins, start playing music
			if (m.BeatNumber == 0) {
				startMusicNextMeasure = false;
				for (int i = 0; i < instrumentPlayers.Length; i++) {
					// TODO: when we can detect missed notes, uncomment this line:
//					instrumentPlayers [i].mute = false;
				}
			}
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
	/// Gets the highest held fret.
	/// </summary>
	/// <returns>The max fret.</returns>
	/// <param name="frets">Frets.</param>
	public static InputButton GetMaxFret(InputButton[] frets) {
		InputButton max = InputButton.NONE;
		foreach (InputButton cur in frets) {
			switch (cur) {
			case InputButton.GREEN:
				if (max == InputButton.NONE)
					max = cur;
				break;
			case InputButton.RED:
				if (max == InputButton.NONE || max == InputButton.GREEN)
					max = cur;
				break;
			case InputButton.YELLOW:
				if (max != InputButton.BLUE && max != InputButton.ORANGE)
					max = cur;
				break;
			case InputButton.BLUE:
				if (max != InputButton.ORANGE)
					max = cur;
				break;
			case InputButton.ORANGE:
				max = cur;
				break;
			default:
				break;
			}
		}
		return max;
	}

	/// <summary>
	/// Gets the hit note or notes if a note was hit
	/// </summary>
	/// <returns>The hit note or notes. Null if no note was hit</returns>
	/// <param name="instrumentID">Instrument ID.</param>
	/// <param name="difficulty">Difficulty.</param>
	/// <param name="frets">List of frets held</param>
	public Note[] GetHitNotes(int instrumentID, int difficulty, InputButton[] frets) {
		float now = player.time;
		float startTime = now - (battleWindow / 2f / bpm * 60f);
		float endTime = now + (battleWindow / 2f / bpm * 60f);
		Note[] notesInWindow = GetNotes (instrumentID, difficulty, startTime, endTime);
		List<Note> ret = new List<Note> ();
		InputButton maxFret = GetMaxFret (frets);
		foreach (Note n in notesInWindow) {
			//TODO: Support chords
			if (n.fretColor == maxFret) {
				ret.Add (n);
			}
		}
		return ret.ToArray ();
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
		if (startTime >= endTime)
			return new Note[0];
		if (startTime >= 0f && endTime <= totalSeconds) {
			List<Note> goodNotes = sortedNotes [instrumentID] [difficulty];
			List<Note> ret = new List<Note> ();
			if (goodNotes.Count == 0)
				return new Note[0];
			int startIndex = 0;
			int endIndex = goodNotes.Count;
			int index = 0;
			while (endIndex > startIndex) {
				index = (startIndex + endIndex) / 2;
				if (goodNotes [index].getPositionTime (bpm) >= startTime) {
					endIndex = index;
				} else {
					startIndex = index + 1;
				}
			}
			for (int i = startIndex; i < goodNotes.Count && goodNotes [i].getPositionTime (bpm) < endTime; i++) {
				ret.Add (goodNotes [i]);
			}
			return ret.ToArray ();
		} else if (startTime < 0) {
			Note[] x = GetNotes (instrumentID, difficulty, totalSeconds + startTime, totalSeconds);
			Note[] y = GetNotes (instrumentID, difficulty, 0, endTime);
			Note[] ret = new Note[x.Length + y.Length];
			x.CopyTo (ret, 0);
			y.CopyTo (ret, x.Length);
			return ret;
		} else {
			Note[] x = GetNotes (instrumentID, difficulty, startTime, totalSeconds);
			Note[] y = GetNotes (instrumentID, difficulty, 0, endTime - totalSeconds);
			Note[] ret = new Note[x.Length + y.Length];
			x.CopyTo (ret, 0);
			y.CopyTo (ret, x.Length);
			return ret;
		}
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
		if (startBeat >= endBeat)
			return new Note[0];
		if (startBeat >= 0 && endBeat <= totalBeats) {
			List<Note> goodNotes = sortedNotes [instrumentID] [difficulty];
			if (goodNotes.Count == 0)
				return new Note[0];
			List<Note> ret = new List<Note> ();
			int startIndex = 0;
			int endIndex = goodNotes.Count;
			int index = 0;
			while (endIndex > startIndex) {
				index = (startIndex + endIndex) / 2;
				if (goodNotes [index].position >= startBeat * goodNotes [index].ppq) {
					endIndex = index;
				} else {
					startIndex = index + 1;
				}
			}
			for (int i = startIndex; i < goodNotes.Count && goodNotes [i].position < endBeat * goodNotes [i].ppq; i++) {
				ret.Add (goodNotes [i]);
			}
			return ret.ToArray ();
		} else if (startBeat < 0) {
			Note[] x = GetNotes (instrumentID, difficulty, totalBeats + startBeat, totalBeats);
			Note[] y = GetNotes (instrumentID, difficulty, 0, endBeat);
			Note[] ret = new Note[x.Length + y.Length];
			x.CopyTo (ret, 0);
			y.CopyTo (ret, x.Length);
			return ret;
		} else {
			Note[] x = GetNotes (instrumentID, difficulty, startBeat, totalBeats);
			Note[] y = GetNotes (instrumentID, difficulty, 0, endBeat - totalBeats);
			Note[] ret = new Note[x.Length + y.Length];
			x.CopyTo (ret, 0);
			y.CopyTo (ret, x.Length);
			return ret;
		}
	}

	/// <summary>
	/// Gets a list of notes for a particular part
	/// </summary>
	/// <returns>The notes.</returns>
	/// <param name="instrumentID">Instrument</param>
	/// <param name="difficulty">Difficulty (0-2)</param>
	public Note[] GetNotes(int instrumentID, int difficulty) {
		return sortedNotes [instrumentID] [difficulty].ToArray ();
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
	/// Gets the next sequence of notes starting at the next measure for a specified number of measures
	/// </summary>
	/// <returns>The next battle notes.</returns>
	/// <param name="numberOfMeasures">Number of measures.</param>
	/// <param name="instrumentID">Instrument</param>
	/// <param name="difficulty">Difficulty (0-2)</param>
	public Note[] GetNextBattleNotes(int numberOfMeasures, int instrumentID, int difficulty) {
		int startMeasure = (int) Math.Ceiling (player.time / 60f * bpm / beatsPerMeasure);
		int endMeasure = startMeasure + numberOfMeasures;
		return GetNotes (instrumentID, difficulty, startMeasure * beatsPerMeasure, endMeasure * beatsPerMeasure);
	}

	/// <summary>
	/// Given a MIDI file, return a list of Note objects contained in the MIDI file (see Note, defined above)
	/// </summary>
	/// <returns>The song data, as a list of Note objects</returns>
	/// <param name="songData">Song data, as a MIDI file asset</param>
	private Note[] LoadSongData(TextAsset songData) {
		List<Note> ret = new List<Note> (1000);
		byte[] buffer = new byte[2048];
		int ppq = 0; // Ticks per beat
		using (Stream source = new MemoryStream(songData.bytes)) {
			int timeCode = 0;
			int statusCode = 0;
			int dataIndex = 0;
			bool awaitingStatus = true;
			int deltaTime = 0; // buffer for delta time bytes that can arise between events
			Note[] activeNotes = new Note[127]; // buffer for notes that are awaiting a "note off"
			byte currentNote = 0; // Note value of last note read
			byte timeComponent = 0;
			int bytesRead = 0;

			// Start by reading the header information in the MIDI file:
			int headerSize = 14;
			bytesRead = source.Read (buffer, 0, headerSize);
			Debug.Assert (buffer.Length >= headerSize, "Buffer must be at least 14 Bytes");
			Debug.Assert (bytesRead == headerSize, "File must be at least as long as a header, duh.");
			// Verify it is indeed a MIDI file:
			byte[] filetype = new byte[4];
			Array.Copy (buffer, filetype, 4);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(filetype);
			Debug.Assert (BitConverter.ToInt32 (filetype, 0) == 0x4d546864, "Must be a MIDI file");
			// Get the PPQ (ticks per beat) from header:
			byte[] ppqArray = new byte[2];
			Array.Copy (buffer, 12, ppqArray, 0, 2);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(ppqArray);
			ppq = BitConverter.ToInt16 (ppqArray, 0);

			// Read actual MIDI events:
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
								activeNotes [currentNote].ppq = ppq;
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

	List<List<List<Note>>> PresortNotes (Note[] notes) {
		List<List<List<Note>>> ret = new List<List<List<Note>>> ();
		foreach (Note n in notes) {
			while (ret.Count - 1 < n.instrumentID) {
				ret.Add (new List<List<Note>> ());
			}
			while (ret [n.instrumentID].Count - 1 < n.difficulty) {
				ret [n.instrumentID].Add (new List<Note> ());
			}
			ret [n.instrumentID] [n.difficulty].Add (n);
		}
		return ret;
	}
}
