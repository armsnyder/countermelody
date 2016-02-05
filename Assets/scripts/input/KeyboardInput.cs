using UnityEngine;
using System.Collections;
using System;
using Frictionless;

public class KeyboardInput : ControllerInput {

	public KeyboardInput(int playerNumber) : base(playerNumber) {}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A)) {
			ServiceFactory.Instance.Resolve<MessageRouter> ().RaiseMessage (new ButtonDownMessage ()
				{ Button = InputButton.GREEN_STRUM, PlayerNumber = PlayerNumber });
        }

        if (Input.GetKeyDown(KeyCode.S)) {
			ServiceFactory.Instance.Resolve<MessageRouter> ().RaiseMessage (new ButtonDownMessage ()
				{ Button = InputButton.RED_STRUM, PlayerNumber = PlayerNumber });
        }

        if (Input.GetKeyDown(KeyCode.D)) {
			ServiceFactory.Instance.Resolve<MessageRouter> ().RaiseMessage (new ButtonDownMessage ()
				{ Button = InputButton.YELLOW_STRUM, PlayerNumber = PlayerNumber });
        }

        if (Input.GetKeyDown(KeyCode.F)) {
			ServiceFactory.Instance.Resolve<MessageRouter> ().RaiseMessage (new ButtonDownMessage ()
				{ Button = InputButton.BLUE_STRUM, PlayerNumber = PlayerNumber });
        }

        if (Input.GetKeyDown(KeyCode.G)) {
			ServiceFactory.Instance.Resolve<MessageRouter> ().RaiseMessage (new ButtonDownMessage ()
				{ Button = InputButton.ORANGE_STRUM, PlayerNumber = PlayerNumber });
        }
	
	}
}
