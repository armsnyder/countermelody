using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Frictionless;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour {

	public Material spriteMat;

	CharacterSelectManager manager;
	MessageRouter messageRouter;
	static readonly Color unselectedColor = new Color (1, 1, 1, 0);
	static readonly Color selectedColor = new Color (1, 1, 1, 0.5f);
	List<InputButton>[,] assigments;

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
			SpriteRenderer spriteRenderer = unit.GetComponentInChildren<SpriteRenderer> ();
			for (int j = 0; j < 2; j++) {
				Image image = transform.FindChild ("UnitList"+j).FindChild ("Unit" + i).FindChild ("Panel").GetComponent<Image> ();
				image.sprite = spriteRenderer.sprite;
				image.material = Instantiate(spriteMat) as Material;
				image.material.EnableKeyword (j == 0 ? "INVERT_OFF" : "INVERT_ON");
				image.material.color = (j == 0 ? Color.white : Color.black);
			}
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
			MelodyUnit selectedUnit = manager.unitPrefabs [index].GetComponent<MelodyUnit> ();
			Healer selectedHealer = manager.unitPrefabs [index].GetComponent<Healer> ();
			SpecialMoveBase selectedSpecial = manager.unitPrefabs [index].GetComponent<SpecialMoveBase> ();
			SetText (playerNumber, "Name", selectedUnit.characterName);
			SetText (playerNumber, "Description", selectedUnit.description);
			SetText (playerNumber, "HP", "HP: " + selectedUnit.HitPoints);
			SetText (playerNumber, "ATK", "ATK: " + selectedUnit.AttackFactor);
			SetText (playerNumber, "DEF", "DEF: " + selectedUnit.DefenceFactor);
			SetText (playerNumber, "ATK RANGE", "ATK RANGE: " + selectedUnit.AttackRange);
			SetText (playerNumber, "HEAL", "HEAL: " + (selectedHealer == null ? "N/A" : selectedHealer.amount.ToString ()));
			SetText (playerNumber, "HEAL RANGE", "HEAL RANGE: " + (selectedHealer == null ? "N/A" : selectedHealer.range.ToString()));
			SetText (playerNumber, "Special Move", "SPECIAL MOVE: " + (selectedSpecial == null ? "N/A" : selectedSpecial.description));
			SpriteRenderer fromR = manager.unitPrefabs [index].GetComponentInChildren<SpriteRenderer> ();
			SpriteRenderer toR = GameObject.Find ("Unit" + playerNumber + "Sprite").GetComponent<SpriteRenderer> ();
			toR.sprite = fromR.sprite;
			toR.material = fromR.sharedMaterial;
			toR.material.EnableKeyword (playerNumber == 0 ? "INVERT_OFF" : "INVERT_ON");
			toR.material.color = (playerNumber == 0 ? Color.white : Color.black);
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
