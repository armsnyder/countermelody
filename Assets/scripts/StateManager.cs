using UnityEngine;
using System.Collections;
using Frictionless;

public enum State {
	AttackState,
	MoveState
}

public class StateChangeMessage {
	public State PrevState;
	public State State;
}

public class StateManager : MonoBehaviour {

	MessageRouter MessageRouter;
	AttackInterpreter AttackInterpreter;
	MoveInterpreter MoveInterpreter;
	int CurrentPlayer;
	private State _CurrentState;

	void Start () {

		//Add Event Handlers
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);

		//Get Attack and Move Interpreters from the gameobject or create new ones
		AttackInterpreter = gameObject.GetComponent<AttackInterpreter> ();
		MoveInterpreter = gameObject.GetComponent<MoveInterpreter> ();
		if (AttackInterpreter == null)
			AttackInterpreter = gameObject.AddComponent<AttackInterpreter> ();
		if (MoveInterpreter == null)
			MoveInterpreter = gameObject.AddComponent<MoveInterpreter> ();
		//Start in Move State
		AttackInterpreter.enabled = false;
		MoveInterpreter.enabled = true;
		_CurrentState = State.MoveState;
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (m.Button == InputButton.WHAMMY && m.PlayerNumber == CurrentPlayer) {
			LoadAttackInterpreter ();
			Debug.Assert(AttackInterpreter.enabled && !MoveInterpreter.enabled,"Attack Interpreter Active");
		}
	}

	void OnButtonUp(ButtonUpMessage m) {
		if (m.Button == InputButton.WHAMMY && m.PlayerNumber == CurrentPlayer) {
			LoadMoveInterpreter ();
			Debug.Assert(!AttackInterpreter.enabled && MoveInterpreter.enabled, "Move Interpreter Active");
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
		MessageRouter.RaiseMessage(new StateChangeMessage () { State = State.AttackState, PrevState = _CurrentState });
		_CurrentState = State.AttackState;
	}

	void LoadMoveInterpreter() {
		Debug.Assert (AttackInterpreter != null && MoveInterpreter != null);
		AttackInterpreter.enabled = false;
		MoveInterpreter.enabled = true;
		MessageRouter.RaiseMessage(new StateChangeMessage() { State = State.MoveState, PrevState = _CurrentState });
		_CurrentState = State.MoveState;
	}
}
