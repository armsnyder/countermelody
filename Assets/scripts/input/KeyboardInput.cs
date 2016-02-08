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
			SendStrumSequence(InputButton.GREEN);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
			SendStrumSequence(InputButton.RED);
        }

        if (Input.GetKeyDown(KeyCode.D)) {
			SendStrumSequence(InputButton.YELLOW);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
			SendStrumSequence(InputButton.BLUE);
        }

        if (Input.GetKeyDown(KeyCode.G)) {
			SendStrumSequence(InputButton.ORANGE);
        }

		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			SendMoveSequence(InputButton.RED);
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			SendMoveSequence(InputButton.BLUE);
		}

		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			SendMoveSequence(InputButton.YELLOW);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			SendMoveSequence(InputButton.GREEN);
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { 
				Button = InputButton.WHAMMY, PlayerNumber = this.PlayerNumber
			});
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { 
				Button = InputButton.WHAMMY, PlayerNumber = this.PlayerNumber
			});
		}
	
	}

	void SendStrumSequence(InputButton button) {
		MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = button, PlayerNumber = this.PlayerNumber });
		MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.STRUM, PlayerNumber = this.PlayerNumber });
		MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = button, PlayerNumber = this.PlayerNumber });
	}

	void SendMoveSequence(InputButton button) {
		MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = button, PlayerNumber = this.PlayerNumber });
		MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = button, PlayerNumber = this.PlayerNumber });
	}
}
