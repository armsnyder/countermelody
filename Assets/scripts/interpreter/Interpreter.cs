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

	protected virtual void Start() {
		// TODO: Cleanup handlers upon switching out Interpreters
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<ButtonDownMessage> (OnButtonDown);
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<ButtonUpMessage> (OnButtonUp);
	}

	protected virtual void OnButtonDown(ButtonDownMessage m) {
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
		if (HeldFrets.ContainsKey (m.PlayerNumber)) {
			HeldFrets [m.PlayerNumber].Remove (m.Button);
		}
	}

	// TODO: Fix message ordering causing a player to attack an extra time
	protected virtual void OnSwitchPlayer(SwitchPlayerMessage m) {
		CurrentPlayer = m.PlayerNumber;
	}
}
