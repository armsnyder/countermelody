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
	private int BeatsPerTurn = 4; // TODO: Get this information from the Song

	private int BeatCounter;
	private int CurrentPlayer;

	private MessageRouter MessageRouter;

	void Awake() {
		// Register MessageRouter (the event BUS) as a singleton so that it can be referenced anywhere
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
		// Register GameManager as a singleton so we can get access to things like Number of Players elsewhere
		ServiceFactory.Instance.RegisterSingleton<GameManager>(this);
		BeatCounter = 0;
		CurrentPlayer = 0;
	}

	void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		BeatCounter++;
		if (BeatCounter == BeatsPerTurn) {
			CurrentPlayer = (CurrentPlayer + 1) % NumberOfPlayers;
			BeatCounter = 0;
			StartCoroutine ("SwitchPlayerCoroutine");
		}
	}

	private IEnumerator SwitchPlayerCoroutine() {
		yield return null;
		MessageRouter.RaiseMessage (new SwitchPlayerMessage () { PlayerNumber = CurrentPlayer });
	}
}
