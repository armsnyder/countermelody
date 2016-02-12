using UnityEngine;
using System.Collections;
using Frictionless;

public class SwitchPlayerMessage {
	public int PlayerNumber { get; set; }
}

public class GameManager : MonoBehaviour {


	public CMCellGrid GameBoard;
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
			GameBoard.Units.FindAll(u => (u as MelodyUnit).PlayerNumber.Equals(CurrentPlayer)).ForEach(u => { (u as MelodyUnit).ActionPoints = (u as MelodyUnit).GetActionPoints(); });
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
