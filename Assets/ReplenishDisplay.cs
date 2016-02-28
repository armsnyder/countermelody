using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReplenishDisplay : MonoBehaviour {

	private IEnumerator displayHeal(int replenish) {
		GetComponent<Text>().text = "+"+ replenish.ToString();
		yield return new WaitForSeconds(1);
		GameObjectUtil.Destroy(gameObject);
	}

	public void displayHealth(int replenish) {
		StartCoroutine(displayHeal(replenish));
	}
}
