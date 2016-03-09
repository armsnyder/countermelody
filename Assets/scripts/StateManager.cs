using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;

public enum State {
	MoveState,
	BattleState,
	PauseState,
	None
}

public class StateChangeMessage {
	public State PrevState;
	public State State;
}

public class StateManager : MonoBehaviour {

	MessageRouter MessageRouter;
	Dictionary<State, Interpreter> interpreters;
	int CurrentPlayer;
	private State _CurrentState = State.None;

	void Awake() {
		interpreters = new Dictionary<State, Interpreter> ();
	}

	void Start () {

		//Add Event Handlers
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<StartSpecialMoveMessage>(OnStartSpecial);
		MessageRouter.AddHandler<EndSpecialMoveMessage>(OnEndSpecial);
		MessageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
		MessageRouter.AddHandler<PauseGameMessage> (OnPauseGame);
		MessageRouter.AddHandler<ResumeGameMessage> (OnResumeGame);

		interpreters [State.MoveState] = gameObject.AddComponent<BoardInterpreter> ();
		interpreters [State.None] = interpreters [State.BattleState] = gameObject.AddComponent<Interpreter> ();
		interpreters [State.PauseState] = gameObject.AddComponent<PauseMenuInterpreter> ();

		//Start in Move State
		StartCoroutine(ChangeState (State.MoveState));
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<SwitchPlayerMessage> (OnSwitchPlayer);
		MessageRouter.RemoveHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.RemoveHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.RemoveHandler<StartSpecialMoveMessage>(OnStartSpecial);
		MessageRouter.RemoveHandler<EndSpecialMoveMessage>(OnEndSpecial);
		MessageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
		MessageRouter.RemoveHandler<PauseGameMessage> (OnPauseGame);
		MessageRouter.RemoveHandler<ResumeGameMessage> (OnResumeGame);
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		StartCoroutine (ChangeState (State.MoveState));
		CurrentPlayer = m.PlayerNumber;
	}

	void OnEnterBattle(EnterBattleMessage m) {
		StartCoroutine (ChangeState (State.BattleState));
	}

	void OnExitBattle(ExitBattleMessage m) {
		StartCoroutine (ChangeState (State.MoveState));
	}

	IEnumerator ChangeState(State newState) {
		yield return new WaitForEndOfFrame ();
		foreach (Interpreter i in interpreters.Values) {
			i.enabled = false;
		}
		interpreters [newState].enabled = true;
		if (_CurrentState != newState) {
			MessageRouter.RaiseMessage (new StateChangeMessage () { State = newState, PrevState = _CurrentState });
			_CurrentState = newState;
		}
	}

	void OnStartSpecial(StartSpecialMoveMessage m) {
		StartCoroutine (ChangeState (State.None));
	}

	void OnEndSpecial(EndSpecialMoveMessage m) {
		StartCoroutine (ChangeState (State.MoveState));
	}

	void OnPauseGame(PauseGameMessage m) {
		StartCoroutine (ChangeState (State.PauseState));
	}

	void OnResumeGame(ResumeGameMessage m) {
		StartCoroutine (ChangeState (State.MoveState));
	}
}
