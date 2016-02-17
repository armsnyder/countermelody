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
        if (Input.GetKeyDown(KeyCode.A)) {
			SendButtonPress(InputButton.GREEN);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
			SendButtonPress(InputButton.RED);
        }

        if (Input.GetKeyDown(KeyCode.D)) {
			SendButtonPress(InputButton.YELLOW);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
			SendButtonPress(InputButton.BLUE);
        }

        if (Input.GetKeyDown(KeyCode.G)) {
			SendButtonPress(InputButton.ORANGE);
        }

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			SendButtonPress(InputButton.UP);
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			SendButtonPress(InputButton.RIGHT);
		}

		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			SendButtonPress(InputButton.DOWN);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			SendButtonPress(InputButton.LEFT);
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { 
				Button = InputButton.STRUM, PlayerNumber = this.PlayerNumber
			});
		}
	
	}

	void SendButtonPress(InputButton button) {
		MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = button, PlayerNumber = this.PlayerNumber });
		MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = button, PlayerNumber = this.PlayerNumber });
	}
}
