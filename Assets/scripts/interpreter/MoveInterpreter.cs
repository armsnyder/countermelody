using UnityEngine;
using System.Collections.Generic;
using Frictionless;

public class MoveInterpreter : Interpreter {

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
			if (HeldFrets.ContainsKey(m.PlayerNumber)) {
				if (m.PlayerNumber == CurrentPlayer && HeldFrets [CurrentPlayer].Count > 0 && IsAcceptingActions) {
					MessageRouter.RaiseMessage (new UnitActionMessage () { 
						ActionType = UnitActionMessageType.SELECT, 
						PlayerNumber = CurrentPlayer,
						Color = HeldFrets [CurrentPlayer] [HeldFrets[CurrentPlayer].Count - 1]
					});
				} else {
					MessageRouter.RaiseMessage (new RejectActionMessage () { 
						ActionType = UnitActionMessageType.SELECT, 
						PlayerNumber = CurrentPlayer
					});
				}
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
		if (m.PlayerNumber == CurrentPlayer && HeldFrets.ContainsKey (m.PlayerNumber) 
			&& HeldFrets [CurrentPlayer].Count == 0 && IsAcceptingActions) {
			switch (m.Button) {
			case InputButton.GREEN:
			case InputButton.RED:
			case InputButton.YELLOW:
			case InputButton.BLUE:
				// Send move message
				MessageRouter.RaiseMessage (new UnitActionMessage () { 
					ActionType = UnitActionMessageType.MOVE, 
					PlayerNumber = CurrentPlayer, 
					Direction = ColorToVector (m.Button)
				});
				break;
			default:
				break;
			}
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
		if (IsAcceptingActions && HeldFrets.ContainsKey(CurrentPlayer) && HeldFrets [CurrentPlayer].Count > 0) {
			// Send end-of-beat move message
			switch (HeldFrets [CurrentPlayer] [HeldFrets [CurrentPlayer].Count - 1]) {
				case InputButton.GREEN:
				case InputButton.BLUE:
				case InputButton.YELLOW:
				case InputButton.UP:
				case InputButton.DOWN:
				case InputButton.LEFT:
				case InputButton.RIGHT:
				case InputButton.RED:
					MessageRouter.RaiseMessage (new UnitActionMessage () { 
						ActionType = UnitActionMessageType.MOVE, 
						PlayerNumber = CurrentPlayer, 
						Direction = ColorToVector (HeldFrets [CurrentPlayer] [HeldFrets[CurrentPlayer].Count - 1])
					});
					break;
				default:
					break;
			}
		}
		
		IsAcceptingActions = false;
	}

	private void OnUnitAction(UnitActionMessage m) {
		if (!enabled)
			return;
		IsAcceptingActions = false;
	}

	private Vector2 ColorToVector(InputButton color) {
		switch (color) {
		case InputButton.GREEN:
			return new Vector2 (-1, 0); // Left
		case InputButton.RED:
			return new Vector2 (0, 1); // Up
		case InputButton.YELLOW:
			return new Vector2 (0, -1); // Down
		case InputButton.BLUE:
			return new Vector2 (1, 0); // Right
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
