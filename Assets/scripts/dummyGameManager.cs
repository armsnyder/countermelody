using UnityEngine;
using System.Collections;
using System;
using Frictionless;

public class dummyGameManager : MonoBehaviour {

	void Awake() {
		// Register MessageRouter (the event BUS) as a singleton so that it can be referenced anywhere
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
	}

	// Use this for initialization
	void Start () {
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<ButtonDownMessage> (LogAThing);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<ButtonUpMessage> (LogAThing);
	}

	void LogAThing(ButtonInputMessage e) {
		String logString = e.PlayerNumber + " ";
		logString += e.Button.ToString () + " ";
		logString += (e is ButtonDownMessage) ? "DOWN" : "UP";
		Debug.Log(logString);
    }
}
