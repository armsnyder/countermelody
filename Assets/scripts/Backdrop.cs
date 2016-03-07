using UnityEngine;
using System.Collections;
using Frictionless;

public class Backdrop : MonoBehaviour {

	private bool side;
	private bool isInSpecial;
	private bool ignore;

	// Use this for initialization
	void Start () {
		ignore = false;
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<BeatCenterMessage> (OnBeatCenter);
		ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<StartSpecialMoveMessage>(OnStartSpecial);
		ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<EndSpecialMoveMessage>(OnEndSpecial);
		ServiceFactory.Instance.Resolve<MessageRouter>().AddHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnSceneChange(SceneChangeMessage m) {
		ignore = true;
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<BeatCenterMessage> (OnBeatCenter);
		ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<StartSpecialMoveMessage>(OnStartSpecial);
		ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<EndSpecialMoveMessage>(OnEndSpecial);
		ServiceFactory.Instance.Resolve<MessageRouter>().RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}
	
	// Update is called once per frame
	void Update () {
		GetComponentInChildren<Light> ().intensity *= 0.95f;
	}

	void OnBeatCenter(BeatCenterMessage m) {
		if (!ignore) {
			if (isInSpecial)
				return;

			side = !side;
			GetComponentInChildren<Light> ().transform.localPosition = new Vector3 ((side ? 0.02f : -0.1f) + Random.value * 0.08f, -0.4f + Random.value * 0.5f, -20f);
			GetComponentInChildren<Light> ().intensity = 5;
		}
	}

	void OnStartSpecial(StartSpecialMoveMessage m) {
		isInSpecial = true;
	}

	void OnEndSpecial(EndSpecialMoveMessage m) {
		isInSpecial = false;
	}
}
