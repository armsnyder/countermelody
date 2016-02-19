using UnityEngine;
using System.Collections.Generic;
using Frictionless;

public class BoardInterpreter : Interpreter {

	private bool IsAcceptingActions;

	protected override void Start() {
		base.Start ();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<EnterBeatWindowMessage>(OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage>(OnExitBeatWindow);
	}

	protected override void OnButtonDown(ButtonDownMessage m) {
		base.OnButtonDown (m);
		if (!enabled)
			return;
		switch (m.Button) {
			case InputButton.STRUM:
				if (m.PlayerNumber == CurrentPlayer && IsAcceptingActions) {
					if (HeldFrets.ContainsKey(m.PlayerNumber) && HeldFrets [CurrentPlayer].Count > 0) {
						MessageRouter.RaiseMessage (new UnitActionMessage () { 
							ActionType = UnitActionMessageType.ATTACK, 
							PlayerNumber = CurrentPlayer,
							Color = HeldFrets [CurrentPlayer] [HeldFrets[CurrentPlayer].Count - 1]
						});
					} else {
						MessageRouter.RaiseMessage (new UnitActionMessage () { 
							ActionType = UnitActionMessageType.ATTACK, 
							PlayerNumber = CurrentPlayer,
							Color = InputButton.NONE
						});
					}
				} else {
					MessageRouter.RaiseMessage(new RejectActionMessage() {
						ActionType = UnitActionMessageType.ATTACK,
						PlayerNumber = CurrentPlayer
					});
				}
				break;
			case InputButton.UP:
			case InputButton.DOWN:
			case InputButton.LEFT:
			case InputButton.RIGHT:
				if (m.PlayerNumber == CurrentPlayer && IsAcceptingActions) {
					MessageRouter.RaiseMessage(new UnitActionMessage() {
						ActionType = UnitActionMessageType.MOVE,
						PlayerNumber = CurrentPlayer,
						Direction = DirectionToVector(m.Button)
					});
				} else {
					MessageRouter.RaiseMessage(new RejectActionMessage() {
						ActionType = UnitActionMessageType.MOVE,
						PlayerNumber = CurrentPlayer
					});
				}
				break;
			default:
				break;
		}
	}

	protected override void OnButtonUp(ButtonUpMessage m) {
		base.OnButtonUp (m);
		if (!enabled)
			return;
		switch (m.Button) {
			case InputButton.GREEN:
			case InputButton.RED:
			case InputButton.YELLOW:
			case InputButton.BLUE:
			case InputButton.ORANGE:
				if (m.PlayerNumber == CurrentPlayer && IsAcceptingActions) {
					MessageRouter.RaiseMessage(new UnitActionMessage() {
						ActionType = UnitActionMessageType.SELECT,
						PlayerNumber = CurrentPlayer,
						Color = m.Button
					});
				}
				break;
			default:
				break;
		}
	}

	private void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		if (!enabled)
			return;
		IsAcceptingActions = true;
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		if (!enabled)
			return;
		if (HeldFrets.ContainsKey(CurrentPlayer) && HeldFrets[CurrentPlayer].Count > 0 && IsAcceptingActions) {
			MessageRouter.RaiseMessage(new UnitActionMessage() {
				ActionType = UnitActionMessageType.SELECT,
				PlayerNumber = CurrentPlayer,
				Color = HeldFrets[CurrentPlayer][HeldFrets[CurrentPlayer].Count - 1]
			});
		}

		IsAcceptingActions = false;
	}

	private void OnUnitAction(UnitActionMessage m) {
		if (!enabled)
			return;
		IsAcceptingActions = false;
	}

	private Vector2 DirectionToVector(InputButton b) {
		switch (b) {
			case InputButton.LEFT:
				return new Vector2 (-1, 0); // Left
			case InputButton.UP:
				return new Vector2 (0, 1); // Up
			case InputButton.DOWN:
				return new Vector2 (0, -1); // Down
			case InputButton.RIGHT:
				return new Vector2 (1, 0); // Right
			default:
				return new Vector2 ();
		}
	}
}
