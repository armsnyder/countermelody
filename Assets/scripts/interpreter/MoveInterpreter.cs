using UnityEngine;
using System.Collections.Generic;
using Frictionless;

public class MoveInterpreter : Interpreter {

	private MessageRouter MessageRouter;
	private bool IsAcceptingActions;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<EnterBeatWindowMessage> (OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
		MessageRouter.AddHandler<UnitActionMessage> (OnUnitAction);
	}

	protected override void OnButtonDown(ButtonDownMessage m) {
		base.OnButtonDown (m);
		switch (m.Button) {
		case InputButton.STRUM:
			
			break;
		}
	}

	private void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		IsAcceptingActions = true;
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		if (IsAcceptingActions) {
//			MessageRouter.RaiseMessage(new UnitActionMessage() {Pla
		}
	}

	private void OnUnitAction(UnitActionMessage m) {
		IsAcceptingActions = false;
	}
}
