using UnityEngine;
using System.Collections;

public class NoteObject : MonoBehaviour {

	public Note NoteData;

	private void Start() {
		if (NoteData != null) {
			SetNoteColor();
		}
	}

	void SetNoteColor(Note note = null) {
		if (note != null) {
			NoteData = note;
		}
		Debug.Assert(NoteData != null, "NoteData must be provided to SetNoteColor or NoteData must be set before setting color");
		Color color = NoteData.fretNumber == 0 ? Color.green :
			NoteData.fretNumber == 1 ? Color.red :
			NoteData.fretNumber == 2 ? Color.yellow :
			NoteData.fretNumber == 3 ? Color.green : new Color(255, 123, 0, 1);
			GetComponent<Renderer>().material.color = color;
	}
}
