using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;

/// <summary>
/// Static class that handles instantiating and destroying GameObjects by routing through the ObjectPool. ALL cases of
/// instantiating and destroying GameObjects should be routed through this class. Don't use GameObject.Instantiate() or
/// GameObject.Destroy() anymore. Use GameObjectUtil.Instantiate() and GameObjectUtil.Destroy() instead!
/// </summary>
public class GameObjectUtil : MonoBehaviour {

	private static Dictionary<RecycleGameObject, ObjectPool> pools = new Dictionary<RecycleGameObject, ObjectPool> ();

	private MessageRouter messageRouter;
	//private static bool ignore;

	void Start() {
		//ignore = false;
		pools = new Dictionary<RecycleGameObject, ObjectPool> ();
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnSceneChange(SceneChangeMessage m) {
		//ignore = true;
		//StartCoroutine(RemoveHandlers());
		pools = new Dictionary<RecycleGameObject, ObjectPool> ();
		/*var recycledScript = prefab.GetComponent<RecycleGameObject> ();

		var pool = GetObjectPool (recycledScript);
		instance = pool.NextObject (pos).gameObject;
		while (instance != null) {
			Destroy(instance);
			instance = pool.NextObject (pos).gameObject;
		}*/
	}

	/*IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();		
		messageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}*/

	public static GameObject Instantiate(GameObject prefab, Vector3 pos){
		/*if (ignore) {
			return null;
		}*/
		GameObject instance = null;

		var recycledScript = prefab.GetComponent<RecycleGameObject> ();
		Debug.Log(recycledScript);
		if (recycledScript != null) {
			var pool = GetObjectPool (recycledScript);
			Debug.Log(pool);
			/*if (pool == null) {
				GameObjectUtil.Destroy(recycledScript.gameObject);
				instance = GameObject.Instantiate (prefab);
				instance.transform.position = pos;
			}*/
			//else {
			instance = pool.NextObject (pos).gameObject; 
			//}
		} else {

			instance = GameObject.Instantiate (prefab);
			instance.transform.position = pos;
		}
		return instance;
	}

	public static GameObject Instantiate(GameObject prefab){
		return Instantiate (prefab, new Vector3 (0, 0, 0));
	}

	public static void Destroy(GameObject gameObject){

		var recyleGameObject = gameObject.GetComponent<RecycleGameObject> ();

		if (recyleGameObject != null) {
			recyleGameObject.Shutdown ();
		} else {
			GameObject.Destroy (gameObject);
		}
	}

	private static ObjectPool GetObjectPool(RecycleGameObject reference){
		ObjectPool pool = null;
		Debug.Log(pools);

		if (pools.ContainsKey (reference)) {
			Debug.Log("here1");
			pool = pools [reference];
		} else {
			Debug.Log("here2");
			var poolContainer = new GameObject(reference.gameObject.name + "ObjectPool");
			pool = poolContainer.AddComponent<ObjectPool>();
			pool.prefab = reference;
			pools.Add (reference, pool);
		}

		return pool;
	}

}
