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
		player = gameObject.AddComponent<AudioSource> ();
		player.clip = songFile;
		player.volume = 0.4f;
	}

	void OnRejectAction(RejectActionMessage m) {
		player.Play ();
	}
}
