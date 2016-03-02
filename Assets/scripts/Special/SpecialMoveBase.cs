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
	public AudioClip music;
	public float musicVolume;

	protected int NumPerformed = 0;
	protected MessageRouter MessageRouter;
	protected Song Song;
	protected bool isActive;
	protected AudioSource audioSource;

	protected virtual void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<TriggerSpecialMoveMessage>(OnTriggerSpecial);
		audioSource = gameObject.AddComponent<AudioSource> ();
		audioSource.clip = music;
		audioSource.volume = musicVolume;
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
		if (MessageRouter != null) {
			MessageRouter.RemoveHandler<TriggerSpecialMoveMessage> (OnTriggerSpecial);
		}
	}
}
