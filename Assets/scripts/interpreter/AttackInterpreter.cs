using UnityEngine;
using System.Collections;
using Frictionless;

public class AttackInterpreter : MonoBehaviour {

	private MessageRouter MessageRouter;
	private bool IsAcceptingActions;

	void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<EnterBeatWindowMessage>(OnEnterBeatWindow);
	}

	private void OnButtonDown(ButtonDownMessage m) {
		if (IsAcceptingActions) {
			MessageRouter.RaiseMessage(new UnitActionMessage() {
				Color = m.Button,
				PlayerNumber = m.PlayerNumber,
				ActionType = UnitActionMessageType.ATTACK
			});
		} else {
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
}