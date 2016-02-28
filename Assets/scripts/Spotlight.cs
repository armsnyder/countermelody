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
			transform.position = new Vector3(focusedOn.transform.position.x, transform.position.y, focusedOn.transform.position.z);
		}
	}

	void turnOff() {
		GetComponent<Light>().spotAngle = 4;
		GetComponent<Light>().intensity = 1f;
	}

	void turnOn() {
		GetComponent<Light>().spotAngle = 4;
		GetComponent<Light>().intensity = 4f;
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
					break;
				default:
					break;
			}
		}
		
	}
}
