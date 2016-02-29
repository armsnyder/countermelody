using UnityEngine;
using System.Collections;
using Frictionless;
using WiimoteApi;

/// <summary>
/// A message sent by the GuitarInput object when it is enabled, indicating that we are interested in receiving input
/// </summary>
public class RegisterGuitarInputMessage {
	public int PlayerNumber { get; set; }
}

/// <summary>
/// A message sent by the GuitarInput object when it is disabled, indicating that we are no longer interested in receiving input
/// </summary>
public class UnregisterGuitarInputMessage {
	public int PlayerNumber { get; set; }
}

public class GuitarInput : ControllerInput {

	private struct GuitarDataModel {
		public bool green;
		public bool red;
		public bool yellow;
		public bool blue;
		public bool orange;
		public bool whammy;
		public bool plus;
		public bool minus;
		public bool strum;
		public bool strum_down;
		public bool strum_up;
		public bool up;
		public bool down;
		public bool left;
		public bool right;
	}

	[SerializeField]
	public int WhammyThreshold = 20;

	private GuitarConnectionManager GuitarInputManager;
	private MessageRouter MessageRouter;
	private GuitarDataModel LastFrameData; // Data from the last frame, used to detect button up/down
	private GuitarDataModel ThisFrameData; // Basically a buffer for incoming guitar data

	public GuitarInput(int PlayerNumber) : base(PlayerNumber) { }

	void Start() {
		GuitarInputManager = ServiceFactory.Instance.Resolve<GuitarConnectionManager>();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		StartCoroutine ("RaiseRegisterInputCoroutine");
	}

	IEnumerator RaiseRegisterInputCoroutine() {
		yield return null;
		MessageRouter.RaiseMessage(new RegisterGuitarInputMessage() { PlayerNumber = PlayerNumber });
	}

	void OnEnable() {
		if (MessageRouter != null) {
			MessageRouter.RaiseMessage(new RegisterGuitarInputMessage() { PlayerNumber = PlayerNumber });
		}
	}

	void OnDisable() {
		MessageRouter.RaiseMessage(new UnregisterGuitarInputMessage() { PlayerNumber = PlayerNumber });
	}

	void Update() {

		// Make sure the wiimote and guitar are connected
		if (!GuitarInputManager.wiimotes.ContainsKey(PlayerNumber))
			return;
		Wiimote wiimote = GuitarInputManager.wiimotes[PlayerNumber];
		if (wiimote == null || wiimote.current_ext != ExtensionController.GUITAR)
			return;

		// Update data buffer
		GuitarData guitar = wiimote.Guitar;
		float[] stick = guitar.GetStick01 ();
		float stickThreshold = 0.1f;
		ThisFrameData.green = guitar.green;
		ThisFrameData.red = guitar.red;
		ThisFrameData.yellow = guitar.yellow;
		ThisFrameData.blue = guitar.blue;
		ThisFrameData.orange = guitar.orange;
		ThisFrameData.whammy = guitar.whammy > WhammyThreshold;
		ThisFrameData.minus = guitar.minus;
		ThisFrameData.plus = guitar.plus;
		ThisFrameData.strum = guitar.strum;
		ThisFrameData.strum_down = guitar.strum_down;
		ThisFrameData.strum_up = guitar.strum_up;
		ThisFrameData.up = wiimote.Button.d_right || stick[1] < 0.5 - stickThreshold;
		ThisFrameData.down = wiimote.Button.d_left || stick[1] > 0.5 + stickThreshold;
		ThisFrameData.left = wiimote.Button.d_up || stick[0] < 0.5 - stickThreshold;
		ThisFrameData.right = wiimote.Button.d_down || stick[0] > 0.5 + stickThreshold;


		// plus
		if (ThisFrameData.plus && !LastFrameData.plus) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.PLUS, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.plus && LastFrameData.plus) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.PLUS, PlayerNumber = PlayerNumber });
		}

		// minus
		if (ThisFrameData.minus && !LastFrameData.minus) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.MINUS, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.minus && LastFrameData.minus) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.MINUS, PlayerNumber = PlayerNumber });
		}

		// whammy
		if (ThisFrameData.whammy && !LastFrameData.whammy) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.WHAMMY, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.whammy && LastFrameData.whammy) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.WHAMMY, PlayerNumber = PlayerNumber });
		}

		// green
		if (ThisFrameData.green && !LastFrameData.green) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.GREEN, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.green && LastFrameData.green) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.GREEN, PlayerNumber = PlayerNumber });
		}

		// red
		if (ThisFrameData.red && !LastFrameData.red) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.RED, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.red && LastFrameData.red) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.RED, PlayerNumber = PlayerNumber });
		}

		// yellow
		if (ThisFrameData.yellow && !LastFrameData.yellow) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.YELLOW, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.yellow && LastFrameData.yellow) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.YELLOW, PlayerNumber = PlayerNumber });
		}

		// blue
		if (ThisFrameData.blue && !LastFrameData.blue) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.BLUE, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.blue && LastFrameData.blue) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.BLUE, PlayerNumber = PlayerNumber });
		}

		// orange
		if (ThisFrameData.orange && !LastFrameData.orange) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.ORANGE, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.orange && LastFrameData.orange) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.ORANGE, PlayerNumber = PlayerNumber });
		}

		// strum
		if (ThisFrameData.strum && !LastFrameData.strum) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.STRUM, PlayerNumber = PlayerNumber });
		}
		if (ThisFrameData.strum_down && !LastFrameData.strum_down) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.STRUM_DOWN, PlayerNumber = PlayerNumber });
		}
		if (ThisFrameData.strum_up && !LastFrameData.strum_up) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.STRUM_UP, PlayerNumber = PlayerNumber });
		}

		// up
		if (ThisFrameData.up && !LastFrameData.up) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.UP, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.up && LastFrameData.up) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.UP, PlayerNumber = PlayerNumber });
		}

		// down
		if (ThisFrameData.down && !LastFrameData.down) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.DOWN, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.down && LastFrameData.down) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.DOWN, PlayerNumber = PlayerNumber });
		}

		// left
		if (ThisFrameData.left && !LastFrameData.left) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.LEFT, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.left && LastFrameData.left) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.LEFT, PlayerNumber = PlayerNumber });
		}

		// right
		if (ThisFrameData.right && !LastFrameData.right) {
			MessageRouter.RaiseMessage(new ButtonDownMessage() { Button = InputButton.RIGHT, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.right && LastFrameData.right) {
			MessageRouter.RaiseMessage(new ButtonUpMessage() { Button = InputButton.RIGHT, PlayerNumber = PlayerNumber });
		}

		// Update LastFrameData
		LastFrameData = ThisFrameData;
	}
}
