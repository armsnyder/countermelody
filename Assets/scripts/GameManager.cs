using UnityEngine;
using System.Collections;
using Frictionless;

public class GameManager : MonoBehaviour {

	void Awake() {
		// Register MessageRouter (the event BUS) as a singleton so that it can be referenced anywhere
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
	}
}
