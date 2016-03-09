using UnityEngine;
using System.Collections;

public class NavigateMenuMessage {
	public NavigationType NavType;
	public int PlayerNumber;
}

public enum NavigationType {
	SCROLL_UP,
	SCROLL_DOWN,
	SELECT,
	BACK,
	EXIT
}

public class MenuInterpreter : Interpreter {

	protected override void OnButtonDown(ButtonDownMessage m) {
		base.OnButtonDown (m);
		if (!enabled)
			return;
		switch (m.Button) {
			case InputButton.STRUM_UP:
			case InputButton.UP:
				MessageRouter.RaiseMessage(new NavigateMenuMessage() {
					PlayerNumber = m.PlayerNumber,
					NavType = NavigationType.SCROLL_UP
				});
				break;
			case InputButton.STRUM_DOWN:
			case InputButton.DOWN:
				MessageRouter.RaiseMessage(new NavigateMenuMessage() {
					PlayerNumber = m.PlayerNumber,
					NavType = NavigationType.SCROLL_DOWN
				});
				break;
			case InputButton.GREEN:
				MessageRouter.RaiseMessage(new NavigateMenuMessage() {
					PlayerNumber = m.PlayerNumber,
					NavType = NavigationType.SELECT
				});
				break;
			case InputButton.RED:
				MessageRouter.RaiseMessage(new NavigateMenuMessage() {
					PlayerNumber = m.PlayerNumber,
					NavType = NavigationType.BACK
				});
				break;
			case InputButton.PLUS:
				MessageRouter.RaiseMessage(new NavigateMenuMessage() {
					PlayerNumber = m.PlayerNumber,
					NavType = NavigationType.EXIT
				});
				break;
			default:
				break;
		}
	}
}
