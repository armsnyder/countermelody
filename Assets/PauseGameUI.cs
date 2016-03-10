using UnityEngine;
using System.Collections;
using Frictionless;

public class PauseGameUI : MonoBehaviour {

	MessageRouter messageRouter;

	// Use this for initialization
	void Start () {
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<PauseGameMessage> ();
		messageRouter.AddHandler<ResumeGameMessage> ();
		messageRouter.AddHandler<ButtonDownMessage> ();
		messageRouter.AddHandler<SceneChangeMessage> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame ();
		messageRouter.RemoveHandler<PauseGameMessage> ();
		messageRouter.RemoveHandler<ResumeGameMessage> ();
		messageRouter.RemoveHandler<ButtonDownMessage> ();
		messageRouter.RemoveHandler<SceneChangeMessage> ();
	}

	void OnPauseGame(PauseGameMessage m) {
	}

	void OnResumeGame(ResumeGameMessage m) {
	}

	void OnButtonDown(ButtonDownMessage m) {
	}

	void Scene(PauseGameMessage m) {
	}
}
