using UnityEngine;
using System.Collections;
using Frictionless;

public class AttackUnitMessage {
	public InputButton Color { get; set; }
	public int PlayerNumber { get; set; }
}

public class AttackInterpreter : MonoBehaviour {

	private MessageRouter MessageRouter;
	private bool IsInBeatWindow;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
		MessageRouter.AddHandler<EnterBeatWindowMessage>(OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage>(OnExitBeatWindow);
	}

	private void OnButtonDown(ButtonDownMessage m) {

	}

	private void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		Debug.Log("Beat Window Entered");
		IsInBeatWindow = true;
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		Debug.Log("Beat Window Exited");
		IsInBeatWindow = false;
	}
	
}
