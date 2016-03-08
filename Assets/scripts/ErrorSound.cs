using UnityEngine;
using System.Collections;
using Frictionless;

public class ErrorSound : MonoBehaviour {

	public AudioClip songFile;

	private MessageRouter MessageRouter;
	private AudioSource player;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<RejectActionMessage> (OnRejectAction);
		MessageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
		player = gameObject.AddComponent<AudioSource> ();
		player.clip = songFile;
		player.volume = 0.4f;
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<RejectActionMessage> (OnRejectAction);
		MessageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnRejectAction(RejectActionMessage m) {
		player.Play ();
	}
}
