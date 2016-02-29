using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Frictionless;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour {

	public Material player0Material;
	public Material player1Material;

	CharacterSelectManager manager;
	MessageRouter messageRouter;
	Color unselectedColor;
	Color selectedColor;
	List<InputButton>[,] assigments;

	void Awake() {
		unselectedColor = new Color (1, 1, 1, 0);
		selectedColor = new Color (1, 1, 1, 0.5f);
	}

	void Start () {
		StartCoroutine (FindManager ());
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<ChangeSelectedCharacterMessage> (OnChangeSelectedCharacter);
		messageRouter.AddHandler<AssignCharacterToFretMessage> (OnAssignCharacterToFret);
		messageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
		SetUnitHighlight (0, 0, true);
		SetUnitHighlight (1, 0, true);
		StartCoroutine (ShowSelectedUnits ());
	}

	IEnumerator FindManager() {
		while (manager == null) {
			manager = ServiceFactory.Instance.Resolve<CharacterSelectManager> ();
			if (manager) {
				LoadUI ();
				assigments = new List<InputButton>[2,manager.unitPrefabs.Length];
				for (int i = 0; i < assigments.GetLength (0); i++) {
					for (int j = 0; j < assigments.GetLength (1); j++) {
						assigments [i, j] = new List<InputButton> ();
					}
				}
				break;
			}
			yield return null;
		}
	}

	void LoadUI() {
		for (int i = 0; i < manager.unitPrefabs.Length; i++) {
			MelodyUnit unit = manager.unitPrefabs[i].GetComponent<MelodyUnit> ();
			Image image = transform.FindChild ("UnitList0").FindChild ("Unit" + i).FindChild ("Panel").GetComponent<Image> ();
			SpriteRenderer spriteRenderer = unit.GetComponentInChildren<SpriteRenderer> ();
			image.sprite = spriteRenderer.sprite;
			image.material = player0Material;
			image = transform.FindChild ("UnitList1").FindChild ("Unit" + i).FindChild ("Panel").GetComponent<Image> ();
			image.sprite = spriteRenderer.sprite;
			image.material = player1Material;
		}
	}

	void OnChangeSelectedCharacter(ChangeSelectedCharacterMessage m) {
		// Clear all selections:
		for (int i = 0; i < manager.unitPrefabs.Length; i++) {
			SetUnitHighlight (m.playerNumber, i, false);
		}
		// Make new selection:
		SetUnitHighlight(m.playerNumber, m.index, true);
	}

	void SetUnitHighlight(int playerNumber, int index, bool selected) {
		Color color = selected ? selectedColor : unselectedColor;
		transform.FindChild ("UnitList" + playerNumber).FindChild ("Unit" + index).GetComponent<Image> ().color = color;
		if (selected) {
			SetText (playerNumber, "Name", manager.unitPrefabs [index].GetComponent<MelodyUnit> ().characterName);
			SetText (playerNumber, "Description", manager.unitPrefabs [index].GetComponent<MelodyUnit> ().description);
			SetText (playerNumber, "HP", "HP: " + manager.unitPrefabs [index].GetComponent<MelodyUnit> ().HitPoints);
			SetText (playerNumber, "ATK", "ATK: " + manager.unitPrefabs [index].GetComponent<MelodyUnit> ().AttackFactor);
			SetText (playerNumber, "DEF", "DEF: " + manager.unitPrefabs [index].GetComponent<MelodyUnit> ().DefenceFactor);
			SetText (playerNumber, "ATK RANGE", "ATK RANGE: " + manager.unitPrefabs [index].GetComponent<MelodyUnit> ().AttackRange);
			SpriteRenderer fromR = manager.unitPrefabs [index].GetComponentInChildren<SpriteRenderer> ();
			SpriteRenderer toR = GameObject.Find ("Unit" + playerNumber + "Sprite").GetComponent<SpriteRenderer> ();
			toR.sprite = fromR.sprite;
			toR.material = playerNumber == 0 ? player0Material : player1Material;
		}
	}

	void SetText(int playerNumber, string child, string text) {
		transform.FindChild ("Stats" + playerNumber).FindChild (child).GetComponent<Text> ().text = text;
	}

	void OnAssignCharacterToFret(AssignCharacterToFretMessage m) {
		for (int i = 0; i < assigments.GetLength (1); i++) {
			assigments [m.playerNumber, i].Remove (m.fret);
		}
		assigments [m.playerNumber, m.index].Add (m.fret);
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine (RemoveHandlers ());
	}

	IEnumerator RemoveHandlers() {
		yield return null;
		messageRouter.RemoveHandler<ChangeSelectedCharacterMessage> (OnChangeSelectedCharacter);
		messageRouter.RemoveHandler<AssignCharacterToFretMessage> (OnAssignCharacterToFret);
		messageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	IEnumerator ShowSelectedUnits() {
		int counter = 0;
		for (;;) {
			yield return new WaitForSeconds (0.5f);
			counter++;
			for (int i = 0; i < assigments.GetLength (0); i++) {
				for (int j = 0; j < assigments.GetLength (1); j++) {
					Color color;
					if (assigments [i, j].Count > 0) {
						color = colorFromFret(assigments [i, j] [counter % assigments [i, j].Count], i);
					} else {
						color = Color.white;
					}
					transform.FindChild ("UnitList"+i).FindChild ("Unit" + j).FindChild ("Panel")
						.GetComponent<Image> ().color = color;
				}
			}
		}
	}

	Color colorFromFret(InputButton fret, int playerNumber) {
		int b = (int)fret;
		Color color = b == 0 ? Color.green : b == 1 ? Color.red : b == 2 ? Color.yellow : b == 3 ? 
			Color.blue : new Color (1, 0.5f, 0, 1);
		if (playerNumber > 0) {
			color = new Color (1 - color.r, 1 - color.g, 1 - color.b, 1);
		}
		return color;
	}
}
