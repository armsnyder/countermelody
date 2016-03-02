using UnityEngine;
using System.Collections;
using Frictionless;

public class TriggerSpecialMoveMessage {
	public MelodyUnit unit;
}

public class StartSpecialMoveMessage { };

public class EndSpecialMoveMessage { }

public abstract class SpecialMoveBase : MonoBehaviour {

	public int NumPerGame = 1;

	protected int NumPerformed = 0;
	protected MessageRouter MessageRouter;
	protected Song Song;
	protected bool isActive;

	protected virtual void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<TriggerSpecialMoveMessage>(OnTriggerSpecial);
	}

	protected virtual void OnTriggerSpecial(TriggerSpecialMoveMessage m) {
		if(m.unit.Equals(gameObject.GetComponent<MelodyUnit>())) {
			if (NumPerformed < NumPerGame) {
				isActive = true;
				NumPerformed++;
				MessageRouter.RaiseMessage(new StartSpecialMoveMessage());
				StartCoroutine(DoSpecialMove());
			} else {
				MessageRouter.RaiseMessage(new RejectActionMessage() {
					PlayerNumber = m.unit.PlayerNumber,
					ActionType = UnitActionMessageType.SPECIAL
				});
			}

		} 
	}

	protected virtual IEnumerator DoSpecialMove() {
		yield return new WaitForSeconds(2);
		isActive = false;
		MessageRouter.RaiseMessage(new EndSpecialMoveMessage());
	}

	void OnDestroy() {
		MessageRouter.RemoveHandler<TriggerSpecialMoveMessage> (OnTriggerSpecial);
	}
}
