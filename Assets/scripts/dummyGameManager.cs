using UnityEngine;
using System.Collections;
using System;
using Frictionless;

public class dummyGameManager : MonoBehaviour {

	private GameObject Metronome;
	private int CurrentPlayer;

	public GameObject NoteThing;

	// Use this for initialization
	void Start () {
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<SwitchPlayerMessage> (LogCurrentPlayer);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<UnitActionMessage> (LogAttack);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<BeatCenterMessage> (OnEnterBeatWindow);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<RejectActionMessage> (OnRejectAction);
		Metronome = GameObject.CreatePrimitive (PrimitiveType.Cube);
		Metronome.transform.localScale = new Vector3(1000, 1000, 1);
		Metronome.transform.position = new Vector3(0, 0, 100);
	}

	void spawnNote(int i) {
		GameObject Note = Instantiate(NoteThing);
		Note.GetComponent<NoteObject>().SetNoteColor(new Note {
			fretNumber = i
		});
		Note.transform.localPosition = new Vector3(i, 0, -10);
	}

	void LogAThing(ButtonInputMessage e) {
		String logString = e.PlayerNumber + " ";
		logString += e.Button.ToString () + " ";
		logString += (e is ButtonDownMessage) ? "DOWN" : "UP";
		Debug.Log(logString);
    }

	void LogCurrentPlayer(SwitchPlayerMessage m) {
		Debug.Log ("Player: "+m.PlayerNumber);
	}

	void LogAttack(UnitActionMessage m) {
		Debug.Log ("Action: "+m.ActionType.ToString());
	}

	void OnEnterBeatWindow(BeatCenterMessage m) {
		Metronome.GetComponent<Renderer> ().material.color = Color.white;
		StartCoroutine ("OnExitBeatWindow");
	}

	IEnumerator OnExitBeatWindow() {
		yield return new WaitForSeconds (0.05f);
		Metronome.GetComponent<Renderer> ().material.color = CurrentPlayer == 0 ? new Color (0.7f, 0.8f, 0.75f) : new Color (0.1f, 0.1f, 0.1f);
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		CurrentPlayer = m.PlayerNumber;
	}

	void OnRejectAction(RejectActionMessage m) {
		Debug.Log ("Reject: "+m.ActionType.ToString());
	}
}
