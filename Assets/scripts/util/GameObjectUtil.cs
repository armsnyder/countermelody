using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Static class that handles instantiating and destroying GameObjects by routing through the ObjectPool. ALL cases of
/// instantiating and destroying GameObjects should be routed through this class. Don't use GameObject.Instantiate() or
/// GameObject.Destroy() anymore. Use GameObjectUtil.Instantiate() and GameObjectUtil.Destroy() instead!
/// </summary>
public class GameObjectUtil {

	private static Dictionary<RecycleGameObject, ObjectPool> pools = new Dictionary<RecycleGameObject, ObjectPool> ();

	public static GameObject Instantiate(GameObject prefab, Vector3 pos){
		GameObject instance = null;

		var recycledScript = prefab.GetComponent<RecycleGameObject> ();
		if (recycledScript != null) {
			var pool = GetObjectPool (recycledScript);
			instance = pool.NextObject (pos).gameObject;
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

		if (pools.ContainsKey (reference)) {
			pool = pools [reference];
		} else {
			var poolContainer = new GameObject(reference.gameObject.name + "ObjectPool");
			pool = poolContainer.AddComponent<ObjectPool>();
			pool.prefab = reference;
			pools.Add (reference, pool);
		}

		return pool;
	}

}
