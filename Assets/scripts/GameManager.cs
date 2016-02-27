﻿using UnityEngine;
using System.Collections;
using Frictionless;

public class SwitchPlayerMessage {
	public int PlayerNumber { get; set; }
}

public class GameManager : MonoBehaviour {

	[SerializeField]
	public int NumberOfPlayers = 2;

	[SerializeField]
	public int MeasuresPerTurn = 1;

	private int _CurrentPlayer;
	public int CurrentPlayer { get { return _CurrentPlayer; } }
	private bool isInBattle;
	private bool isInSpecial;

	private MessageRouter MessageRouter;

	void Awake() {
		// Register MessageRouter (the event BUS) as a singleton so that it can be referenced anywhere
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
		// Register GameManager as a singleton so we can get access to things like Number of Players elsewhere
		ServiceFactory.Instance.RegisterSingleton<GameManager>(this);
		_CurrentPlayer = 0;
	}

	void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
		MessageRouter.AddHandler<EnterBattleMessage> (OnEnterBattle);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<StartSpecialMoveMessage>(OnStartSpecial);
		MessageRouter.AddHandler<EndSpecialMoveMessage>(OnEndSpecial);
		StartCoroutine (FirstFrameCoroutine ());
	}

	IEnumerator FirstFrameCoroutine() {
		yield return null;
		for (int i = 0; i < NumberOfPlayers; i++) {
			// Start every player on medium
			// TODO: Remove this once proper difficulty select is implemented
			MessageRouter.RaiseMessage (new BattleDifficultyChangeMessage () { PlayerNumber = i, Difficulty = 2 });
		}
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
		// Gets info about turn length / whose turn it is from Song.cs (helps game remain in sync)
		if (isInBattle || isInSpecial) return; // Do not change player if in battle
		int BeatsPerTurn = m.BeatsPerMeasure * MeasuresPerTurn;
		if (m.BeatNumber % BeatsPerTurn == BeatsPerTurn - 1) { // If it's the last beat of a player's turn...
			_CurrentPlayer = (_CurrentPlayer + 1) % NumberOfPlayers;
			StartCoroutine ("SwitchPlayerCoroutine");
		}
	}

	private IEnumerator SwitchPlayerCoroutine() {
		yield return null;
		MessageRouter.RaiseMessage (new SwitchPlayerMessage () { PlayerNumber = _CurrentPlayer });
	}

	void OnEnterBattle(EnterBattleMessage m) {
		isInBattle = true;
	}

	void OnExitBattle(ExitBattleMessage m) {
		isInBattle = false;
		_CurrentPlayer = (_CurrentPlayer + 1) % NumberOfPlayers;
		StartCoroutine ("SwitchPlayerCoroutine");
	}

	void OnStartSpecial(StartSpecialMoveMessage m) {
		isInSpecial = true;
	}

	void OnEndSpecial(EndSpecialMoveMessage m) {
		isInSpecial = false;
	}
}
