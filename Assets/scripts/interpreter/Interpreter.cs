using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
	SELECT,
	SPECIAL
}

public class RejectActionMessage {
	public int PlayerNumber { get; set; }
	public UnitActionMessageType ActionType { get; set; }
}

public class Interpreter : MonoBehaviour {
	public static Dictionary<int, List<InputButton>> HeldFrets = new Dictionary<int, List<InputButton>> ();
	protected static int CurrentPlayer;
	protected MessageRouter MessageRouter;
	private bool ignore;

	protected virtual void Start() {
		ignore = false;
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		MessageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnSceneChange(SceneChangeMessage m) {
		ignore = true;
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.RemoveHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.RemoveHandler<ButtonUpMessage> (OnButtonUp);
		MessageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	protected virtual void OnButtonDown(ButtonDownMessage m) {
		if (ignore) {
			return;
		}
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
