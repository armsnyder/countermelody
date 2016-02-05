﻿using UnityEngine;
using System.Collections;
using Frictionless;

public class MoveUnitMessage {
	public Vector2 Direction { get; set; }
	public int PlayerNumber { get; set; }
}

public class SelectUnitMessage {
	public InputButton Color { get; set; }
	public int PlayerNumber { get; set; }
}

public class MoveInterpreter : Interpreter {

	private MessageRouter MessageRouter;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		MessageRouter.AddHandler<ButtonUpMessage> (OnButtonUp);
		MessageRouter.AddHandler<EnterBeatWindowMessage> (OnEnterBeatWindow);
		MessageRouter.AddHandler<ExitBeatWindowMessage> (OnExitBeatWindow);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void OnButtonDown(ButtonDownMessage m) {
	}

	private void OnButtonUp(ButtonUpMessage m) {
	}

	private void OnEnterBeatWindow(EnterBeatWindowMessage m) {
	}

	private void OnExitBeatWindow(ExitBeatWindowMessage m) {
	}
}
