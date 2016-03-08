using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;

/// <summary>
/// Object pool. This class is only ever used internally within GameObjectUtil, so don't worry about it.
/// </summary>
public class ObjectPool : MonoBehaviour {

	public RecycleGameObject prefab;

	private List<RecycleGameObject> poolInstances = new List<RecycleGameObject>();

	private MessageRouter messageRouter;
	private bool ignore;

	void Awake() {
		DontDestroyOnLoad(transform.gameObject);
	}

	void Start() {
		ignore = false;
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnSceneChange(SceneChangeMessage m) {
		ignore = true;
		poolInstances = new List<RecycleGameObject>();	
		//StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();		

		messageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	private RecycleGameObject CreateInstance(Vector3 pos){

		var clone = GameObject.Instantiate (prefab);
		clone.transform.position = pos;
		clone.transform.parent = transform;

		poolInstances.Add (clone);

		return clone;
	}

	public RecycleGameObject NextObject(Vector3 pos){		
		RecycleGameObject instance = null;

		foreach (var go in poolInstances) {
			if(go.gameObject.activeSelf != true){
				instance = go;
				instance.transform.position = pos;
			}
		}

		if(instance == null)
			instance = CreateInstance (pos);

		instance.Restart ();

		return instance;
	}

}
