using UnityEngine;
using System.Collections;
using Frictionless;
using System;
using UnityEngine.UI;

public class DamageDisplayManager : MonoBehaviour {

	public GameObject damageDisplayPrefab;
	public GameObject replenishDisplayPrefab;
	private MessageRouter MessageRouter;
	// Use this for initialization
	void Start () {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<TakeDamageMessage>(OnTakeDamage);
		MessageRouter.AddHandler<ExitHealMessage>(OnExitHeal);
		MessageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<TakeDamageMessage>(OnTakeDamage);
		MessageRouter.RemoveHandler<ExitHealMessage>(OnExitHeal);
		MessageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnTakeDamage(TakeDamageMessage m) {
		// Find position to display the damage number. Should be above the unit.
		//TODO: use math to figure out where the damage numbers should go.
		float height_offset = 0.1f;
		float width_offset = -0.02f;
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(m.Recipient.transform.position);
		viewportPoint += new Vector3(width_offset, height_offset);

		DisplayDamage(m.damage, viewportPoint);
	}

	void OnExitHeal(ExitHealMessage m) {
		int replenish = m.Replenish;

		// Find position to display the damage number. Should be above the unit.
		//TODO: use math to figure out where the damage numbers should go.
		float height_offset = 0.1f;
		float width_offset = -0.02f;
		Vector3 viewportPoint = Camera.main.WorldToViewportPoint(m.Recipient.transform.position);
		viewportPoint += new Vector3(width_offset, height_offset);

		DisplayHeal(replenish, viewportPoint);
	}

	private void DisplayHeal(int replenish, Vector3 position) {

		GameObject replenish_display_object = GameObjectUtil.Instantiate(replenishDisplayPrefab);
		replenish_display_object.transform.parent = transform;
		replenish_display_object.transform.localPosition = new Vector3();

		Text replenish_display = replenish_display_object.GetComponent<Text>();

		replenish_display.rectTransform.anchorMin = position;
		replenish_display.rectTransform.anchorMax = position;

		replenish_display_object.GetComponent<ReplenishDisplay>().displayHealth(replenish);
	}

	private void DisplayDamage(int damage, Vector3 position) {

		GameObject damage_display_object = GameObjectUtil.Instantiate(damageDisplayPrefab);
		damage_display_object.transform.parent = transform;
		damage_display_object.transform.localPosition = new Vector3();

		Text damage_display = damage_display_object.GetComponent<Text>();

		damage_display.rectTransform.anchorMin = position;
		damage_display.rectTransform.anchorMax = position;

		damage_display_object.GetComponent<DamageDisplay>().displayDamage(damage);
	}
}
