using UnityEngine;
using System.Collections;
using Frictionless;

public class SwitchPlayerMessage {
	public int PlayerNumber { get; set; }
}

public class GameManager : MonoBehaviour {

	[SerializeField]
	public int NumberOfPlayers = 2;

	[SerializeField]
	public int MeasuresPerTurn = 1;

	private int _CurrentPlayer;
	public int CurrentPlayer { get { return _CurrentPlayer; } }

	private MessageRouter MessageRouter;

	void Awake() {
		// Register MessageRouter (the event BUS) as a singleton so that it can be referenced anywhere
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
		// Register GameManager as a singleton so we can get access to things like Number of Players elsewhere
		ServiceFactory.Instance.RegisterSingleton<GameManager>(this);
		_CurrentPlayer = 0;
	}

	void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		// Gets info about turn length / whose turn it is from Song.cs (helps game remain in sync)
		// TODO: Make sure correct player turn after battle
		int BeatsPerTurn = m.BeatsPerMeasure * MeasuresPerTurn;
		if (m.BeatNumber % BeatsPerTurn == BeatsPerTurn - 1) { // If it's the last beat of a player's turn...
			_CurrentPlayer = (_CurrentPlayer + 1) % NumberOfPlayers;
			StartCoroutine ("SwitchPlayerCoroutine");
		}
	}

	private IEnumerator SwitchPlayerCoroutine() {
		yield return null;
		MessageRouter.RaiseMessage (new SwitchPlayerMessage () { PlayerNumber = _CurrentPlayer });
	}
}
