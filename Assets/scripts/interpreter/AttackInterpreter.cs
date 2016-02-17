using UnityEngine;
using System.Collections;
using Frictionless;

public class AttackInterpreter : Interpreter {

	private bool IsAcceptingActions;

	protected override void Start () {
		base.Start ();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<EnterBeatWindowMessage>(OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage>(OnExitBeatWindow);
	}

	protected override void OnButtonDown(ButtonDownMessage m) {
		base.OnButtonDown (m);
		if (!enabled)
			return;
		if (m.Button == InputButton.WHAMMY) // So that we don't immediately send a rejection upon entering state
			return;
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
		if (!enabled)
			return;
		IsAcceptingActions = false;
	}

	private void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		if (!enabled)
			return;
		IsAcceptingActions = true;
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		if (!enabled)
			return;
		IsAcceptingActions = false;
	}
}