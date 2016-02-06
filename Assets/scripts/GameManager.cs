using UnityEngine;
using System.Collections;
using Frictionless;

public class SwitchPlayerMessage {
	public int PlayerNumber { get; set; }
}

public class GameManager : MonoBehaviour {

	[SerializeField]
	private int NumberOfPlayers = 2;

	[SerializeField]
	private int BeatsPerTurn = 4; // TODO: Get this information from the Song

	private int BeatCounter;
	private int CurrentPlayer;

	private MessageRouter MessageRouter;

	void Awake() {
		// Register MessageRouter (the event BUS) as a singleton so that it can be referenced anywhere
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
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
			MessageRouter.RaiseMessage (new SwitchPlayerMessage () { PlayerNumber = CurrentPlayer });
			BeatCounter = 0;
		}
	}
}
