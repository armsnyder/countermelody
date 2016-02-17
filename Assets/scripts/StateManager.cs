using UnityEngine;
using System.Collections;
using Frictionless;

public enum State {
	AttackState,
	MoveState,
	BattleState,
	None
}

public class StateChangeMessage {
	public State PrevState;
	public State State;
}

public class StateManager : MonoBehaviour {

	MessageRouter MessageRouter;
	AttackInterpreter AttackInterpreter;
	MoveInterpreter MoveInterpreter;
	Interpreter DefaultInterpreter;
	int CurrentPlayer;
	private State _CurrentState = State.None;

	void Start () {

		//Add Event Handlers
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);

		//Get Attack and Move Interpreters from the gameobject or create new ones
		AttackInterpreter = gameObject.AddComponent<AttackInterpreter> ();
		MoveInterpreter = gameObject.AddComponent<MoveInterpreter> ();
		DefaultInterpreter = gameObject.AddComponent<Interpreter> ();
//		if (AttackInterpreter == null)
//			AttackInterpreter = gameObject.AddComponent<AttackInterpreter> ();
//		if (MoveInterpreter == null)
//			MoveInterpreter = gameObject.AddComponent<MoveInterpreter> ();
//		if (DefaultInterpreter == null)
//			DefaultInterpreter = gameObject.AddComponent<Interpreter> ();
		//Start in Move State
		LoadMoveInterpreter();
		ChangeState (State.MoveState);
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (_CurrentState == State.MoveState && m.Button == InputButton.WHAMMY && m.PlayerNumber == CurrentPlayer) {
			LoadAttackInterpreter ();
			ChangeState (State.AttackState);
			Debug.Assert(AttackInterpreter.enabled && !MoveInterpreter.enabled,"Attack Interpreter Active");
		}
	}

	void OnButtonUp(ButtonUpMessage m) {
		if (_CurrentState == State.AttackState && m.Button == InputButton.WHAMMY && m.PlayerNumber == CurrentPlayer) {
			LoadMoveInterpreter ();
			ChangeState (State.MoveState);
			Debug.Assert(!AttackInterpreter.enabled && MoveInterpreter.enabled, "Move Interpreter Active");
		}
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		LoadMoveInterpreter ();
		CurrentPlayer = m.PlayerNumber;
	}

	void OnEnterBattle(EnterBattleMessage m) {
		LoadDefaultInterpreter ();
		ChangeState (State.BattleState);
	}

	void OnExitBattle(ExitBattleMessage m) {
		LoadMoveInterpreter ();
		ChangeState (State.MoveState);
	}

	void LoadAttackInterpreter() {
		Debug.Assert (AttackInterpreter != null && MoveInterpreter != null);
		MoveInterpreter.enabled = false;
		DefaultInterpreter.enabled = false;
		AttackInterpreter.enabled = true;
	}

	void LoadMoveInterpreter() {
		Debug.Assert (AttackInterpreter != null && MoveInterpreter != null);
		AttackInterpreter.enabled = false;
		DefaultInterpreter.enabled = false;
		MoveInterpreter.enabled = true;
	}

	void LoadDefaultInterpreter() {
		AttackInterpreter.enabled = false;
		MoveInterpreter.enabled = false;
		DefaultInterpreter.enabled = true;
	}

	void ChangeState(State newState) {
		MessageRouter.RaiseMessage(new StateChangeMessage() { State = newState, PrevState = _CurrentState });
		_CurrentState = newState;
	}
}
