using UnityEngine;
using System.Collections;
using System;
using Frictionless;

public class KeyboardInput : ControllerInput {

	public KeyboardInput(int playerNumber) : base(playerNumber) {}

	private MessageRouter MessageRouter;

	void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		// TODO: Should we raise a keyboard connected message here?
	}

	// Update is called once per frame
	void Update () {

		// KeyDowns

        if (Input.GetKeyDown(KeyCode.A)) {
			SendButtonDown(InputButton.GREEN);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
			SendButtonDown(InputButton.RED);
        }

        if (Input.GetKeyDown(KeyCode.D)) {
			SendButtonDown(InputButton.YELLOW);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
			SendButtonDown(InputButton.BLUE);
        }

        if (Input.GetKeyDown(KeyCode.G)) {
			SendButtonDown(InputButton.ORANGE);
        }

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			SendButtonDown(InputButton.UP);
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			SendButtonDown(InputButton.RIGHT);
		}

		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			SendButtonDown(InputButton.DOWN);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			SendButtonDown(InputButton.LEFT);
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			SendButtonDown (InputButton.STRUM);
		}

		if (Input.GetKeyDown(KeyCode.Equals)) {
			SendButtonDown (InputButton.PLUS);
		}

		if (Input.GetKeyDown (KeyCode.Minus)) {
			SendButtonDown (InputButton.MINUS);
		}

		// KeyUps

		if (Input.GetKeyUp(KeyCode.A)) {
			SendButtonUp(InputButton.GREEN);
		}

		if (Input.GetKeyUp(KeyCode.S)) {
			SendButtonUp(InputButton.RED);
		}

		if (Input.GetKeyUp(KeyCode.D)) {
			SendButtonUp(InputButton.YELLOW);
		}

		if (Input.GetKeyUp(KeyCode.F)) {
			SendButtonUp(InputButton.BLUE);
		}

		if (Input.GetKeyUp(KeyCode.G)) {
			SendButtonUp(InputButton.ORANGE);
		}

		if (Input.GetKeyUp(KeyCode.UpArrow)) {
			SendButtonUp(InputButton.UP);
		}

		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			SendButtonUp(InputButton.RIGHT);
		}

		if (Input.GetKeyUp(KeyCode.DownArrow)) {
			SendButtonUp(InputButton.DOWN);
		}

		if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			SendButtonUp(InputButton.LEFT);
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			SendButtonUp (InputButton.STRUM);
		}

		if (Input.GetKeyUp (KeyCode.Equals)) {
			SendButtonUp (InputButton.PLUS);
		}

		if (Input.GetKeyUp (KeyCode.Minus)) {
			SendButtonUp (InputButton.MINUS);
		}
	
	}

	void SendButtonPress(InputButton button) {
		SendButtonDown (button);
		SendButtonUp (button);
	}

	void SendButtonDown(InputButton button) {
		MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = button, PlayerNumber = this.PlayerNumber });
	}

	void SendButtonUp(InputButton button) {
		MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = button, PlayerNumber = this.PlayerNumber });
	}
}
