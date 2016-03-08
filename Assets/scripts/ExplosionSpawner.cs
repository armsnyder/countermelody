using UnityEngine;
using System.Collections;
using Frictionless;

public class ExplosionSpawner : MonoBehaviour {

	public GameObject prefab;
	public AudioClip sound;
	public float volume = 0.5f;

	MessageRouter messageRouter;
	AudioSource audioSource;

	void Awake() {
		audioSource = gameObject.AddComponent<AudioSource> ();
		audioSource.clip = sound;
		audioSource.Stop ();
	}

	void Start () {
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
		messageRouter.AddHandler<UnitDeathMessage> (OnUnitDeath);
	}

	void OnUnitDeath(UnitDeathMessage m) {
		audioSource.Stop ();
		audioSource.Play ();
		StartCoroutine (QueueExplosion (m.unit.transform.position));
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine (UnregisterHandlers ());
	}

	IEnumerator QueueExplosion(Vector3 position) {
		yield return new WaitForSeconds (0.1f);
		GameObject explosionObject = GameObjectUtil.Instantiate (prefab, position);
		StartCoroutine (CleanUpExplosionObject (explosionObject));
	}

	IEnumerator UnregisterHandlers() {
		yield return new WaitForEndOfFrame ();
		messageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
		messageRouter.RemoveHandler<UnitDeathMessage> (OnUnitDeath);
	}

	IEnumerator CleanUpExplosionObject(GameObject obj) {
		yield return new WaitForSeconds (1f);
		GameObjectUtil.Destroy (obj);
	}
}
