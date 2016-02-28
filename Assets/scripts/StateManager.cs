using UnityEngine;
using System.Collections;
using Frictionless;

public enum State {
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
	BoardInterpreter BoardInterpreter;
	Interpreter DefaultInterpreter;
	int CurrentPlayer;
	private State _CurrentState = State.None;

	void Start () {

		//Add Event Handlers
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<StartSpecialMoveMessage>(OnStartSpecial);
		MessageRouter.AddHandler<EndSpecialMoveMessage>(OnEndSpecial);

		BoardInterpreter = gameObject.AddComponent<BoardInterpreter> ();
		DefaultInterpreter = gameObject.AddComponent<Interpreter> ();

		//Start in Move State
		LoadBoardInterpreter();
		ChangeState (State.MoveState);
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		LoadBoardInterpreter ();
		CurrentPlayer = m.PlayerNumber;
	}

	void OnEnterBattle(EnterBattleMessage m) {
		LoadDefaultInterpreter ();
		ChangeState (State.BattleState);
	}

	void OnExitBattle(ExitBattleMessage m) {
		LoadBoardInterpreter ();
		ChangeState (State.MoveState);
	}

	void LoadBoardInterpreter() {
		Debug.Assert (DefaultInterpreter != null && BoardInterpreter != null);
		DefaultInterpreter.enabled = false;
		BoardInterpreter.enabled = true;
	}

	void LoadDefaultInterpreter() {
		BoardInterpreter.enabled = false;
		DefaultInterpreter.enabled = true;
	}

	void ChangeState(State newState) {
		MessageRouter.RaiseMessage(new StateChangeMessage() { State = newState, PrevState = _CurrentState });
		_CurrentState = newState;
	}

	void OnStartSpecial(StartSpecialMoveMessage m) {
		LoadDefaultInterpreter();
	}

	void OnEndSpecial(EndSpecialMoveMessage m) {
		LoadBoardInterpreter();
	}
}
