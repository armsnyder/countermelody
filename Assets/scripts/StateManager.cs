using UnityEngine;
using System.Collections;
using Frictionless;

public class StateManager : MonoBehaviour {

	MessageRouter MessageRouter;
	int CurrentPlayer;
	AttackInterpreter AttackInterpreter;
	MoveInterpreter MoveInterpreter;

	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		AttackInterpreter = gameObject.GetComponent<AttackInterpreter> ();
		MoveInterpreter = gameObject.GetComponent<MoveInterpreter> ();
		if (AttackInterpreter == null)
			AttackInterpreter = gameObject.AddComponent<AttackInterpreter> ();
		if (MoveInterpreter == null)
			MoveInterpreter = gameObject.AddComponent<MoveInterpreter> ();
		AttackInterpreter.enabled = false;
		MoveInterpreter.enabled = true;
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (m.Button == InputButton.WHAMMY && m.PlayerNumber == CurrentPlayer) {
			LoadAttackInterpreter ();
		}
	}

	void OnButtonUp(ButtonUpMessage m) {
		if (m.Button == InputButton.WHAMMY && m.PlayerNumber == CurrentPlayer) {
			LoadMoveInterpreter ();
		}
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		LoadMoveInterpreter ();
		CurrentPlayer = m.PlayerNumber;
	}

	void LoadAttackInterpreter() {
		Debug.Assert (AttackInterpreter != null && MoveInterpreter != null);
		MoveInterpreter.enabled = false;
		AttackInterpreter.enabled = true;
	}

	void LoadMoveInterpreter() {
		Debug.Assert (AttackInterpreter != null && MoveInterpreter != null);
		AttackInterpreter.enabled = false;
		MoveInterpreter.enabled = true;
	}
}
