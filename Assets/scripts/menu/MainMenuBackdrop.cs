using UnityEngine;
using System.Collections;

public class MainMenuBackdrop : MonoBehaviour {
	[SerializeField]
	float songBPM = 160;
	bool side;
	// Use this for initialization
	void Start () {
		StartCoroutine(FlashLight());
	}

	void Update() {
		GetComponentInChildren<Light>().intensity *= 0.95f;
	}

	IEnumerator FlashLight() {
		while (true) {
			yield return new WaitForSeconds(60/songBPM);
			side = !side;
			GetComponentInChildren<Light>().transform.localPosition = new Vector3((side ? 0.02f : -0.1f) + Random.value * 0.08f, -0.4f + Random.value * 0.5f, -20f);
			GetComponentInChildren<Light>().color = Random.ColorHSV(0, 1, .8f, 1, 1, 1);
			GetComponentInChildren<Light>().intensity = 3;
		}
	}	
}
