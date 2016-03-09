using UnityEngine;
using System.Collections;

public class PauseMenuInterpreter : MenuInterpreter {

	protected override void Start() {
		base.Start ();
		MessageRouter.AddHandler<NavigateMenuMessage> (OnNavigateMenu);
	}

	void OnNavigateMenu(NavigateMenuMessage m) {
		if (!enabled)
			return;
		if (m.NavType == NavigationType.EXIT) {
			MessageRouter.RaiseMessage (new ResumeGameMessage () { playerNumber = m.PlayerNumber });
		}
	}

	protected override void OnSceneChange(SceneChangeMessage m) {
		base.OnSceneChange (m);
		StartCoroutine (RemoveHandlers ());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame ();
		MessageRouter.RemoveHandler<NavigateMenuMessage> (OnNavigateMenu);
	}
}
