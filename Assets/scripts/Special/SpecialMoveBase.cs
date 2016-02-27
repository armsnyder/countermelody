using UnityEngine;
using System.Collections;
using Frictionless;

public class StartSpecialMoveMessage {
	public MelodyUnit unit;
}
public class EndSpecialMoveMessage { }

public class SpecialMoveBase : MonoBehaviour {

	private MessageRouter MessageRouter;
	private Song Song;

	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<StartSpecialMoveMessage>(OnStartSpecial);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnStartSpecial(StartSpecialMoveMessage m) {
		if(m.unit.Equals(gameObject.GetComponent<MelodyUnit>())) {
			StartCoroutine(DoSpecialMove());
		}
	}

	IEnumerator DoSpecialMove() {
		yield return new WaitForSeconds(2);
		MessageRouter.RaiseMessage(new EndSpecialMoveMessage());
	}
}
