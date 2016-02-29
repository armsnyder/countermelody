using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using System;

public class Moonwalk : SpecialMoveBase {
	protected bool isMoving = false;
	private bool hasMoved = false;

	protected override void Start() {
		base.Start();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (!isActive)
			return;

		if (m.PlayerNumber != GetComponent<MelodyUnit>().PlayerNumber)
			return;

		if (hasMoved)
			return;

		switch (m.Button) {
			case InputButton.UP:
			case InputButton.DOWN:
			case InputButton.LEFT:
			case InputButton.RIGHT:
				hasMoved = true;
				StartCoroutine(MovementAnimation(BoardInterpreter.DirectionToVector(m.Button)));
				break;
		}
	}

	protected override IEnumerator DoSpecialMove() {

		ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
		HighlightSpecial();

		yield return new WaitForSeconds(2);

		while (isMoving) {
			yield return null;
		}
		
		isActive = false;
		MessageRouter.RaiseMessage(new EndSpecialMoveMessage());
	}

	private void HighlightSpecial() {
		Vector2[] directions = new Vector2[] { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };

		
		foreach (Vector2 dir in directions) {
			Cell curr = GetComponent<MelodyUnit>().Cell;
			Cell next = ServiceFactory.Instance.Resolve<CellGrid>().Cells.Find(c => c.OffsetCoord == curr.OffsetCoord + dir);
			while (next != null) {
				next.MarkAsReachable();
				next = ServiceFactory.Instance.Resolve<CellGrid>().Cells.Find(c => c.OffsetCoord == next.OffsetCoord + dir);
			}
		}
		
	}

	protected IEnumerator MovementAnimation(Vector2 direction) {
		isMoving = true;
		ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
		// If moving horizontally, set sprite facing opposite direction
		if (direction.x != 0) {
			GetComponentInChildren<SpriteRenderer>().flipX = direction.x > 0;
		}

		List<Cell> path = new List<Cell>();
		Cell neighbor = ServiceFactory.Instance.Resolve<CellGrid>().Cells.Find(
			c => c.OffsetCoord == GetComponent<MelodyUnit>().Cell.OffsetCoord + direction && !c.IsTaken);
		while (neighbor != null) {
			Vector3 startPosition = transform.position;
			float totalDistance = Vector3.Distance(transform.position, neighbor.transform.position);
			float moveProgress = 0;
			while (moveProgress < totalDistance) {
				moveProgress += Time.deltaTime * GetComponent<MelodyUnit>().MovementSpeed;
				if (moveProgress > totalDistance)
					moveProgress = totalDistance;
				transform.position = Vector3.Lerp(startPosition, neighbor.transform.position, moveProgress / totalDistance);
				yield return 0;
			}
			GetComponent<MelodyUnit>().Cell = neighbor;
			neighbor = ServiceFactory.Instance.Resolve<CellGrid>().Cells.Find(
				c => c.OffsetCoord == neighbor.OffsetCoord + direction && !c.IsTaken);
		}

		isMoving = false;
	}

}
