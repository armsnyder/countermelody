using UnityEngine;
using System.Collections;
using Frictionless;

public class StartSpecialMoveMessage {
	public MelodyUnit unit;
}
public class EndSpecialMoveMessage {
	public MelodyUnit unit;
}

public class SpecialMoveBase : MonoBehaviour {

	private MessageRouter MessageRouter;
	private Song Song;

	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
