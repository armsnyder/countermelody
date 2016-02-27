using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamageDisplay : MonoBehaviour {

	private IEnumerator display(int damage) {

		GetComponent<Text>().text = "-"+ damage.ToString();
		yield return new WaitForSeconds(1);
		GameObjectUtil.Destroy(gameObject);
	}

	public void displayDamage(int damage) {
		StartCoroutine(display(damage));
	}
}
