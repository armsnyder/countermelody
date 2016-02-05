using UnityEngine;
using System.Collections;
using Frictionless;

public class AttackInterpreter : MonoBehaviour {

	private MessageRouter MessageRouter;

	void Start() {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
	}

	private void OnButtonDown(ButtonDownMessage m) {
		MessageRouter.RaiseMessage(new UnitActionMessage() 
		{ 
			Color = m.Button, 
			PlayerNumber = m.PlayerNumber, 
			ActionType = UnitActionMessageType.ATTACK 
		});
	}
}
