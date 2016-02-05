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
	}

	[SerializeField]
	public int WhammyThreshold = 20;

	private GuitarInputManager GuitarInputManager;
	private MessageRouter MessageRouter;
	private GuitarDataModel LastFrameData; // Data from the last frame, used to detect button up/down
	private GuitarDataModel ThisFrameData; // Basically a buffer for incoming guitar data

	public GuitarInput(int PlayerNumber) : base(PlayerNumber) {}

	void Start () {
		GuitarInputManager = ServiceFactory.Instance.Resolve<GuitarInputManager> ();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.RaiseMessage (new RegisterGuitarInputMessage (){ PlayerNumber = PlayerNumber });
	}

	void OnEnable() {
		if (MessageRouter != null) {
			MessageRouter.RaiseMessage (new RegisterGuitarInputMessage (){ PlayerNumber = PlayerNumber });
		}
	}

	void OnDisable() {
		MessageRouter.RaiseMessage (new UnregisterGuitarInputMessage (){ PlayerNumber = PlayerNumber });
	}

	void Update () {

		// Make sure the wiimote and guitar are connected
		if (!GuitarInputManager.wiimotes.ContainsKey(PlayerNumber))
			return;
		Wiimote wiimote = GuitarInputManager.wiimotes [PlayerNumber];
		if (wiimote == null || wiimote.current_ext != ExtensionController.GUITAR)
			return;

		// Update data buffer
		GuitarData guitar = wiimote.Guitar;
		ThisFrameData.green = guitar.green;
		ThisFrameData.red = guitar.red;
		ThisFrameData.yellow = guitar.yellow;
		ThisFrameData.blue = guitar.blue;
		ThisFrameData.orange = guitar.orange;
		ThisFrameData.whammy = guitar.whammy > WhammyThreshold;
		ThisFrameData.minus = guitar.minus;
		ThisFrameData.plus = guitar.plus;
		ThisFrameData.strum = guitar.strum;

		// plus
		if (ThisFrameData.plus && !LastFrameData.plus) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.PLUS, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.plus && LastFrameData.plus) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.PLUS, PlayerNumber = PlayerNumber });
		}

		// minus
		if (ThisFrameData.minus && !LastFrameData.minus) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.MINUS, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.minus && LastFrameData.minus) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.MINUS, PlayerNumber = PlayerNumber });
		}

		// whammy
		if (ThisFrameData.whammy && !LastFrameData.whammy) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.WHAMMY, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.whammy && LastFrameData.whammy) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.WHAMMY, PlayerNumber = PlayerNumber });
		}

		// green
		if (ThisFrameData.green && !LastFrameData.green) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.GREEN, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.green && LastFrameData.green) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.GREEN, PlayerNumber = PlayerNumber });
		}

		// red
		if (ThisFrameData.red && !LastFrameData.red) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.RED, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.red && LastFrameData.red) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.RED, PlayerNumber = PlayerNumber });
		}

		// yellow
		if (ThisFrameData.yellow && !LastFrameData.yellow) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.YELLOW, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.yellow && LastFrameData.yellow) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.YELLOW, PlayerNumber = PlayerNumber });
		}

		// blue
		if (ThisFrameData.blue && !LastFrameData.blue) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.BLUE, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.blue && LastFrameData.blue) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.BLUE, PlayerNumber = PlayerNumber });
		}

		// orange
		if (ThisFrameData.orange && !LastFrameData.orange) {
			MessageRouter.RaiseMessage (new ButtonDownMessage () { Button = InputButton.ORANGE, PlayerNumber = PlayerNumber });
		} else if (!ThisFrameData.orange && LastFrameData.orange) {
			MessageRouter.RaiseMessage (new ButtonUpMessage () { Button = InputButton.ORANGE, PlayerNumber = PlayerNumber });
		}

		// strum
		// NOTE: As is currently implemented, multiple-fret strum is unsupported
		if (ThisFrameData.strum && !LastFrameData.strum) {
			if (ThisFrameData.green) {
				MessageRouter.RaiseMessage (new ButtonDownMessage () {
					Button = InputButton.GREEN_STRUM,
					PlayerNumber = PlayerNumber
				});
			} else if (ThisFrameData.red) {
				MessageRouter.RaiseMessage (new ButtonDownMessage () {
					Button = InputButton.RED_STRUM,
					PlayerNumber = PlayerNumber
				});
			} else if (ThisFrameData.yellow) {
				MessageRouter.RaiseMessage (new ButtonDownMessage () {
					Button = InputButton.YELLOW_STRUM,
					PlayerNumber = PlayerNumber
				});
			} else if (ThisFrameData.blue) {
				MessageRouter.RaiseMessage (new ButtonDownMessage () {
					Button = InputButton.BLUE_STRUM,
					PlayerNumber = PlayerNumber
				});
			} else if (ThisFrameData.orange) {
				MessageRouter.RaiseMessage (new ButtonDownMessage () {
					Button = InputButton.ORANGE_STRUM,
					PlayerNumber = PlayerNumber
				});
			} else {
				MessageRouter.RaiseMessage (new ButtonDownMessage () {
					Button = InputButton.BLANK_STRUM,
					PlayerNumber = PlayerNumber
				});
			}
		} // Strum ButtonUpMessage would be useless, so not included for now

		// Update LastFrameData
		LastFrameData = ThisFrameData;
	}
}
