using UnityEngine;
using System.Collections.Generic;
using Frictionless;

public class UnitActionMessage {
	public InputButton Color { get; set; }
	public int PlayerNumber { get; set; }
	public Vector2 Direction { get; set; }
	public UnitActionMessageType ActionType { get; set; }
}

public enum UnitActionMessageType {
	MOVE,
	ATTACK,
	SELECT
}

public class RejectActionMessage {
	public int PlayerNumber { get; set; }
	public UnitActionMessageType ActionType { get; set; }
}

public abstract class Interpreter : MonoBehaviour {
	protected static Dictionary<int, List<InputButton>> HeldFrets = new Dictionary<int, List<InputButton>> ();
	protected static int CurrentPlayer;
	protected MessageRouter MessageRouter;

	protected virtual void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
	}

	protected virtual void OnButtonDown(ButtonDownMessage m) {
		if (!enabled)
			return;
		switch (m.Button) {
		case InputButton.GREEN:
		case InputButton.RED:
		case InputButton.YELLOW:
		case InputButton.BLUE:
		case InputButton.ORANGE:
			if (!HeldFrets.ContainsKey (m.PlayerNumber)) {
				HeldFrets.Add (m.PlayerNumber, new List<InputButton> ());
			}
			HeldFrets [m.PlayerNumber].Add (m.Button);
			break;
		default:
			break;
		}
	}

	protected virtual void OnButtonUp(ButtonUpMessage m) {
		if (!enabled)
			return;
		if (HeldFrets.ContainsKey (m.PlayerNumber)) {
			while (HeldFrets [m.PlayerNumber].Contains (m.Button)) {
				HeldFrets [m.PlayerNumber].Remove (m.Button);
			}
		}
	}

	protected virtual void OnSwitchPlayer(SwitchPlayerMessage m) {
		CurrentPlayer = m.PlayerNumber;
	}
}
