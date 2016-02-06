using UnityEngine;
using System.Collections;
using Frictionless;

public class AttackInterpreter : Interpreter {

	private MessageRouter MessageRouter;
	private bool IsAcceptingActions;

	protected override void Start() {
		base.Start ();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<EnterBeatWindowMessage>(OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage>(OnExitBeatWindow);
	}

	protected override void OnButtonDown(ButtonDownMessage m) {
		base.OnButtonDown (m);
		if (IsAcceptingActions && m.PlayerNumber == CurrentPlayer) {
			MessageRouter.RaiseMessage(new UnitActionMessage() {
				Color = m.Button,
				PlayerNumber = m.PlayerNumber,
				ActionType = UnitActionMessageType.ATTACK
			});
		} else { // TODO: Do we want to penalize them if it isn't their turn?
			MessageRouter.RaiseMessage(new RejectActionMessage(){
				PlayerNumber = m.PlayerNumber
			});
		}
	}

	private void OnUnitAction(UnitActionMessage m) {
		IsAcceptingActions = false;
	}

	private void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		IsAcceptingActions = true;
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		IsAcceptingActions = false;
	}
}