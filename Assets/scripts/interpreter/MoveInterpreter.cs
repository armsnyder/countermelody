using UnityEngine;
using System.Collections.Generic;
using Frictionless;

public class MoveInterpreter : Interpreter {

	private MessageRouter MessageRouter;
	private bool IsAcceptingActions;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<EnterBeatWindowMessage> (OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
		MessageRouter.AddHandler<UnitActionMessage> (OnUnitAction);
	}

	protected override void OnButtonDown(ButtonDownMessage m) {
		base.OnButtonDown (m);
		switch (m.Button) {
		case InputButton.STRUM:
			if (HeldFrets.ContainsKey(m.PlayerNumber)) {
				if (m.PlayerNumber == CurrentPlayer && HeldFrets [CurrentPlayer].Count > 0 && IsAcceptingActions) {
					MessageRouter.RaiseMessage (new UnitActionMessage () { 
						ActionType = UnitActionMessageType.SELECT, 
						PlayerNumber = CurrentPlayer
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
		if (m.PlayerNumber == CurrentPlayer && HeldFrets.ContainsKey (m.PlayerNumber) 
			&& HeldFrets [CurrentPlayer].Count == 0 && IsAcceptingActions) {
			switch (m.Button) {
			case InputButton.GREEN:
			case InputButton.RED:
			case InputButton.YELLOW:
			case InputButton.BLUE:
			case InputButton.ORANGE:
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
		IsAcceptingActions = true;
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		if (IsAcceptingActions && HeldFrets.ContainsKey(CurrentPlayer) && HeldFrets [CurrentPlayer].Count > 0) {
			// Send end-of-beat move message
			MessageRouter.RaiseMessage (new UnitActionMessage () { 
				ActionType = UnitActionMessageType.MOVE, 
				PlayerNumber = CurrentPlayer, 
				Direction = ColorToVector (HeldFrets [CurrentPlayer] [HeldFrets.Count - 1])
			});
		}
		IsAcceptingActions = false;
	}

	private void OnUnitAction(UnitActionMessage m) {
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
		default:
			return new Vector2 ();
		}
	}
}
