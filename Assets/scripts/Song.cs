using UnityEngine;
using System.Collections;
using Frictionless;

public class EnterBeatWindowMessage {}

public class ExitBeatWindowMessage {}

public class BeatCenterMessage {}

public class Song : MonoBehaviour {

	public int bpm = 80;
	public float window = 0.5f;
	private MessageRouter MessageRouter;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		StartCoroutine ("BeatCoroutine");
	}
	
	void OnDisable() {
		StopCoroutine ("BeatCoroutine");
	}

	private IEnumerator BeatCoroutine() {
		while (true) {
			yield return new WaitForSeconds (60f / bpm * (1 - window));
			MessageRouter.RaiseMessage (new EnterBeatWindowMessage ());
			yield return new WaitForSeconds (60f / bpm * window / 2);
			MessageRouter.RaiseMessage (new BeatCenterMessage ());
			yield return new WaitForSeconds (60f / bpm * window / 2);
			MessageRouter.RaiseMessage (new ExitBeatWindowMessage ());
		}
	}
}
