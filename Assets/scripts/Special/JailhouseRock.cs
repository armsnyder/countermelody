using UnityEngine;
using System.Collections;
using Frictionless;
using System;

public class JailhouseRock : SpecialMoveBase {

	CMCellGrid GameBoard;
	private int DisabledCounter;
	private Unit DisabledUnit;
	private Material prevMaterial;
	private GameObject JailCell;

	[SerializeField]
	private int DisableTurns = 5;
	[SerializeField]
	private GameObject JailPrefab;

	protected override void Start() {
		base.Start();
		MessageRouter.AddHandler<SwitchPlayerMessage>(OnSwitchPlayer);
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
	}
	protected override IEnumerator DoSpecialMove() {
		StartSpecialMove();

		while (inputStartTime + inputWaitTime > Time.time) {
			if (hasButtonPressed) {
				break;
			}
			yield return null;
		}

		while (audioSource.isPlaying) {
				yield return null;
		}

		EndSpecialMove();
	}

	protected override void HighlightSpecial() {
		foreach (Unit u in ServiceFactory.Instance.Resolve<CellGrid>().Units.FindAll(c => c.PlayerNumber != GetComponent<MelodyUnit>().PlayerNumber)) {
			u.Cell.MarkAsReachable();
		}
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {

		if (DisabledUnit != null) {
			DisabledCounter++;
			if (DisabledCounter == DisableTurns) {
				GameBoard.Units.Add(DisabledUnit);
				DisabledUnit = null;
				GameObjectUtil.Destroy(JailCell);
			}
		}
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (!isActive)
			return;

		if (m.PlayerNumber != GetComponent<MelodyUnit>().PlayerNumber)
			return;

		if (DisabledUnit != null)
			return;

		GameBoard = ServiceFactory.Instance.Resolve<CellGrid>() as CMCellGrid;

		Unit recipient;

		switch (m.Button) {
			case InputButton.GREEN:
			case InputButton.RED:
			case InputButton.YELLOW:
			case InputButton.BLUE:
			case InputButton.ORANGE:
				recipient = GameBoard.Units.Find(c => (c.PlayerNumber!=m.PlayerNumber) && ((c as MelodyUnit).ColorButton == m.Button));
				if (recipient) {
					hasButtonPressed = true;
					DisableUnit(recipient);
					ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
				}
				audioSource.Play ();
				break;
		}
	}

	void DisableUnit(Unit u) {
		GameBoard = ServiceFactory.Instance.Resolve<CellGrid>() as CMCellGrid;
		DisabledUnit = u;
		DisabledCounter = 0;
		GameBoard.Units.Remove(u);

		JailCell = GameObjectUtil.Instantiate(JailPrefab);
		JailCell.transform.parent = u.Cell.transform;
		JailCell.transform.localPosition = new Vector3();
		JailCell.transform.localPosition += new Vector3(0, .01f);
		JailCell.GetComponent<SpriteRenderer>().color = GetComponent<MelodyUnit>().unitColor;
	}
}
