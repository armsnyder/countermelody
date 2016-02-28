using UnityEngine;
using System.Collections;
using Frictionless;

public class StartSpecialMoveMessage {
	public MelodyUnit unit;
}
public class EndSpecialMoveMessage { }

public class SpecialMoveBase : MonoBehaviour {

	protected MessageRouter MessageRouter;
	protected Song Song;

	protected virtual void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<StartSpecialMoveMessage>(OnStartSpecial);
	}

	protected virtual void OnStartSpecial(StartSpecialMoveMessage m) {
		if(m.unit.Equals(gameObject.GetComponent<MelodyUnit>())) {
			StartCoroutine(DoSpecialMove());
		}
	}

	protected virtual IEnumerator DoSpecialMove() {
		yield return new WaitForSeconds(2);
		MessageRouter.RaiseMessage(new EndSpecialMoveMessage());
	}
}
