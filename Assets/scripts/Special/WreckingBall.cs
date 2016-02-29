using UnityEngine;
using System.Collections;
using Frictionless;
using System;

public class WreckingBall : SpecialMoveBase {

	private bool isAnimating;
	private float MOVEMENT_SPEED = 2;
	[SerializeField]
	private GameObject WreckingBallSprite;
	[SerializeField]
	private int damage = 50;

	protected override void Start() {
		base.Start();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
	}

	void OnButtonDown(ButtonDownMessage m) {
		if (!isActive)
			return;

		if (m.PlayerNumber != GetComponent<MelodyUnit>().PlayerNumber)
			return;

		if (isAnimating)
			return;

		switch (m.Button) {
			case InputButton.UP:
			case InputButton.DOWN:
			case InputButton.LEFT:
			case InputButton.RIGHT:
				StartCoroutine(SwingWreckingBall(BoardInterpreter.DirectionToVector(m.Button)));
				break;
		}
	}

	protected override IEnumerator DoSpecialMove() {

		ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
		foreach (Cell c in ServiceFactory.Instance.Resolve<CellGrid>().Cells.FindAll(c => c.OffsetCoord.x == GetComponent<MelodyUnit>().Cell.OffsetCoord.x || c.OffsetCoord.y == GetComponent<MelodyUnit>().Cell.OffsetCoord.y)) {
			c.MarkAsReachable();
		}

		yield return new WaitForSeconds(2);

		while (isAnimating) {
			yield return null;
		}


		isActive = false;
		MessageRouter.RaiseMessage(new EndSpecialMoveMessage());
	}

	protected IEnumerator SwingWreckingBall(Vector2 direction) {
		isAnimating = true;
		//ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();
		//GameObject wreckingBallSprite = GameObjectUtil.Instantiate(WreckingBallSprite);

		//CMCellGrid grid = ServiceFactory.Instance.Resolve<CellGrid>() as CMCellGrid;

		//Vector3 startPos = transform.position;
		//while (startPos.x >= grid.transform.position.x && startPos.y >= grid.transform.position.y) {
		//		startPos -= new Vector3(Math.Abs(direction.x), Math.Abs(direction.y));
		//}

		//wreckingBallSprite.transform.position = startPos;

		//Vector3 endPos = new Vector3();
		//float totalDistance = 0;
		//if (direction.x != 0) {
		//	endPos.x = grid.GetComponent<CMCellGridGenerator>().gridSize.x;
		//	totalDistance = endPos.x;
		//} else if (direction.y != 0) {
		//	endPos.y = grid.GetComponent<CMCellGridGenerator>().gridSize.y;
		//	totalDistance = endPos.y;
		//} else {
		//	Debug.LogError("Wrecking ball direction must be 1 in x or y");
		//}

		//float moveProgress = 0;
		//Vector3 MovementEachFrame = Vector3.Lerp(wreckingBallSprite.transform.position, endPos, MOVEMENT_SPEED * Time.deltaTime / totalDistance);
		//while (moveProgress < totalDistance) { 
		//	moveProgress += MovementEachFrame.magnitude;
		//	if (moveProgress > totalDistance)
		//		moveProgress = totalDistance;
		//	wreckingBallSprite.transform.position += MovementEachFrame;
		//	yield return 0;
		//}
		isAnimating = false;
		DealDamageInDirection(direction);
		ServiceFactory.Instance.Resolve<UnitManager>().UnHighlightAll();

		yield return null;
	}

	private void DealDamageInDirection(Vector2 direction) {
		if(direction.x != 0) {
			foreach (Unit u in ServiceFactory.Instance.Resolve<CellGrid>().Units.FindAll(
				c => c.PlayerNumber != GetComponent<MelodyUnit>().PlayerNumber &&
				c.Cell.OffsetCoord.y == GetComponent<MelodyUnit>().Cell.OffsetCoord.y)) {

				(u as MelodyUnit).Defend(GetComponent<MelodyUnit>(), damage, 0);
			}
		} else if (direction.y != 0) {
			foreach (Unit u in ServiceFactory.Instance.Resolve<CellGrid>().Units.FindAll(
				c => c.PlayerNumber != GetComponent<MelodyUnit>().PlayerNumber && 
				c.Cell.OffsetCoord.x == GetComponent<MelodyUnit>().Cell.OffsetCoord.x)) {

				(u as MelodyUnit).Defend(GetComponent<MelodyUnit>(), damage, 0);
			}
		}
	}
}
