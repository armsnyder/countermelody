using UnityEngine;
using System.Collections;
using System;
using Frictionless;

public class dummyGameManager : MonoBehaviour {

	private GameObject Metronome;
	private int CurrentPlayer;

	// Use this for initialization
	void Start () {
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<SwitchPlayerMessage> (LogCurrentPlayer);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<UnitActionMessage> (LogAttack);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<EnterBeatWindowMessage> (OnEnterBeatWindow);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		Metronome = GameObject.CreatePrimitive (PrimitiveType.Cube);
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

	void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		Metronome.GetComponent<Renderer> ().material.color = Color.white;
	}

	void OnExitBeatWindow(ExitBeatWindowMessage m) {
		Metronome.GetComponent<Renderer> ().material.color = CurrentPlayer == 0 ? Color.yellow : Color.red;
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		CurrentPlayer = m.PlayerNumber;
	}
}
