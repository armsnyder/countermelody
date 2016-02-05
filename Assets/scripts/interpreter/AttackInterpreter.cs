using UnityEngine;
using System.Collections;
using Frictionless;

public class AttackUnitMessage {
	public InputButton Color { get; set; }
	public int PlayerNumber { get; set; }
}

public class AttackInterpreter : MonoBehaviour {

	private MessageRouter MessageRouter;

	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
	}

	private void OnButtonDown(ButtonDownMessage m) {
			MessageRouter.RaiseMessage(new AttackUnitMessage() { Color = m.Button, PlayerNumber = m.PlayerNumber });
	}
}
