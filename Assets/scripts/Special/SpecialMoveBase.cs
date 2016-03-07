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
	public string description;

	protected float inputWaitTime = 10f;
	protected float inputStartTime;
	protected int NumPerformed = 0;
	protected MessageRouter MessageRouter;
	protected Song Song;
	protected bool isActive;
	protected AudioSource audioSource;
	protected bool hasButtonPressed;

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
		StartSpecialMove();
		yield return new WaitForSeconds(inputWaitTime);
		EndSpecialMove();
	}

	protected abstract void HighlightSpecial();

	protected virtual void StartSpecialMove() {
		inputStartTime = Time.time;
		ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
		HighlightSpecial();
	}

	protected virtual void EndSpecialMove() {
		isActive = false;
		ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
		MessageRouter.RaiseMessage(new EndSpecialMoveMessage());
	}

	void OnDestroy() {
		if (MessageRouter != null) {
			MessageRouter.RemoveHandler<TriggerSpecialMoveMessage> (OnTriggerSpecial);
		}
	}
}
