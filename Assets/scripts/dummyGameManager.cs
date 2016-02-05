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
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<ButtonInputMessage> (LogAThing);
	}

	void LogAThing(ButtonInputMessage e) {
		Debug.Log(e.Button.ToString());
    }
}
