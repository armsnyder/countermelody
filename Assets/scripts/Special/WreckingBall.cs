using UnityEngine;
using System.Collections;
using Frictionless;
using System;

public class WreckingBall : SpecialMoveBase {

	private bool isAnimating;
	private float MOVEMENT_TIME = .5f;
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

		GameObject wreckingBallSprite = GameObjectUtil.Instantiate(WreckingBallSprite);

		CMCellGrid grid = ServiceFactory.Instance.Resolve<CellGrid>() as CMCellGrid;

		Vector3 startPos = transform.position;
		while (startPos.x >= grid.transform.position.x && startPos.z >= grid.transform.position.z) {
				startPos -= new Vector3(Math.Abs(direction.x), 0, Math.Abs(direction.y));
		}
		// Grid offset
		startPos -= new Vector3(0, 0, 0.5f);

		wreckingBallSprite.transform.position += startPos;

		Vector3 endPos = startPos;
		if (direction.x != 0) {
			endPos.x = grid.GetComponent<CMCellGridGenerator>().gridSize.x;
		} else if (direction.y != 0) {
			endPos.z = grid.GetComponent<CMCellGridGenerator>().gridSize.y;
		} else {
			Debug.LogError("Wrecking ball direction must be 1 in x or y");
		}

		float i = 0;
		float rate = 1.0f / MOVEMENT_TIME;

		wreckingBallSprite.transform.GetChild(0).eulerAngles += new Vector3(20 * Math.Abs(direction.y), 0, -60 * Math.Abs(direction.x));

		Vector3 startRotation = new Vector3();
		Vector3 endRotation = new Vector3(-120 * Math.Abs(direction.y), 0, 120 * Math.Abs(direction.x));
		Vector3 rotationperFrame = Vector3.Slerp(startRotation, endRotation, Time.deltaTime * rate);

		while (i < 1) { 
			i += Time.deltaTime * rate;
			wreckingBallSprite.transform.position = Vector3.Lerp(startPos, endPos, i);

			wreckingBallSprite.transform.GetChild(0).eulerAngles += rotationperFrame;

			yield return 0;
		}

		isAnimating = false;
		DealDamageInDirection(direction);
		GameObjectUtil.Destroy(wreckingBallSprite);
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
