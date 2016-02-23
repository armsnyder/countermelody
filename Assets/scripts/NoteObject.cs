using UnityEngine;
using System.Collections;

public class NoteObject : MonoBehaviour {

	public Note NoteData { get { return _NoteData; } }
	private Note _NoteData;
	public float destroyY = -10f; // Y coordinate under which object will self destruct
	public Vector3 velocity = new Vector3(0, -0.22f, 0);
	public Vector3 centerOfObject = new Vector3(.5f, 0f);

	public void SetNoteColor(Note note) {
		_NoteData = note;
		Color color = _NoteData.fretNumber == 0 ? Color.green :
			_NoteData.fretNumber == 1 ? Color.red :
			_NoteData.fretNumber == 2 ? Color.yellow :
			_NoteData.fretNumber == 3 ? Color.blue : 
			new Color(1, 0.5f, 0, 1);
		gameObject.transform.FindChild ("Color").GetComponent<Renderer> ().material.color = color;
		Material rimMaterial = note.isHOPO ? gameObject.transform.FindChild ("Cap").GetComponent<Renderer> ().material : 
			gameObject.transform.FindChild ("Border").GetComponent<Renderer> ().material;
		gameObject.transform.FindChild ("Rim").GetComponent<Renderer> ().material = rimMaterial;
	}

	void Update() {
		if (transform.localPosition.y < destroyY) {
			GameObjectUtil.Destroy (this.gameObject);
		}
	}

	void FixedUpdate() {
		transform.position += velocity; // Notes constantly fall
	}
}
