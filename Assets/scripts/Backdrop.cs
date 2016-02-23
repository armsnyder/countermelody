using UnityEngine;
using System.Collections;
using Frictionless;

public class Backdrop : MonoBehaviour {

	private bool side;

	// Use this for initialization
	void Start () {
		ServiceFactory.Instance.Resolve<MessageRouter> ().AddHandler<BeatCenterMessage> (OnBeatCenter);
	}
	
	// Update is called once per frame
	void Update () {
		GetComponentInChildren<Light> ().intensity *= 0.95f;
	}

	void OnBeatCenter(BeatCenterMessage m) {
		side = !side;
		GetComponentInChildren<Light> ().transform.localPosition = new Vector3 ((side ? 0.02f : -0.1f) + Random.value * 0.08f, -0.4f + Random.value * 0.5f, -20f);
		GetComponentInChildren<Light> ().intensity = 5;
	}
}
