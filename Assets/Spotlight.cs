using UnityEngine;
using System.Collections;
using Frictionless;

public class SpotlightChangeMessage {
	public MelodyUnit focusedOnUnit;
	public ChangeType type;
	public int PlayerNumber;
}

public enum ChangeType {
	SWITCH,
	ON,
	OFF
}

public class Spotlight : MonoBehaviour {

	private MessageRouter MessageRouter;
	public MelodyUnit focusedOn;
	public int PlayerNumber;

	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<SpotlightChangeMessage>(OnSpotlightChange);
		turnOff();
	}
	
	// Update is called once per frame
	void Update () {
		if (focusedOn != null) {
			transform.LookAt(focusedOn.transform.position);
		}
	}

	void turnOff() {
		GetComponent<Light>().spotAngle = 15;
		GetComponent<Light>().intensity = .5f;
	}

	void turnOn() {
		GetComponent<Light>().spotAngle = 5;
		GetComponent<Light>().intensity = 8f;
	}

	void OnSpotlightChange(SpotlightChangeMessage m) {
		if (m.PlayerNumber == PlayerNumber) {
			switch (m.type) {
				case ChangeType.OFF:
					turnOff();
					break;
				case ChangeType.ON:
					turnOn();
					break;
				case ChangeType.SWITCH:
					focusedOn = m.focusedOnUnit;
					GetComponent<Light>().color = focusedOn.unitColor;
					transform.LookAt(focusedOn.transform.position);
					break;
				default:
					break;
			}
		}
		
	}
}
