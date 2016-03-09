using UnityEngine;
using System.Collections;
using Frictionless;
using System;

public class WreckingBall : SpecialMoveBase {

	private bool isAnimating;
	private readonly int MUSIC_INTRO_SAMPLES = 56006;
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

		if (hasButtonPressed)
			return;

		switch (m.Button) {
			case InputButton.UP:
			case InputButton.DOWN:
			case InputButton.LEFT:
			case InputButton.RIGHT:
				hasButtonPressed = true;
				StartCoroutine(SwingWreckingBall(BoardInterpreter.DirectionToVector(m.Button)));
				audioSource.Play ();
				break;
		}
	}

	protected override IEnumerator DoSpecialMove() {
		StartSpecialMove();

		while (inputStartTime + inputWaitTime > Time.time) {
			if(hasButtonPressed) {
				break;
			}
			yield return null;
		}

		while (isAnimating) {
			yield return null;
		}

		EndSpecialMove();
	}

	protected override void HighlightSpecial() {
		foreach (Cell c in ServiceFactory.Instance.Resolve<CellGrid>().Cells.FindAll(c => c.OffsetCoord.x == GetComponent<MelodyUnit>().Cell.OffsetCoord.x || c.OffsetCoord.y == GetComponent<MelodyUnit>().Cell.OffsetCoord.y)) {
			c.MarkAsReachable();
		}
	}

	protected IEnumerator SwingWreckingBall(Vector2 direction) {
		isAnimating = true;

		while (audioSource.timeSamples < MUSIC_INTRO_SAMPLES) {
			yield return null; // Wait for "I came in like a--" to pass before swinging ball
		}

		float swingTime = audioSource.clip.length - audioSource.time;

		GameObject wreckingBallSprite = GameObjectUtil.Instantiate(WreckingBallSprite);

		CMCellGrid grid = ServiceFactory.Instance.Resolve<CellGrid>() as CMCellGrid;

		Vector3 startPos = transform.position;
		float grid_width = grid.GetComponent<CMCellGridGenerator>().gridSize.x;
		float grid_height = grid.GetComponent<CMCellGridGenerator>().gridSize.y;
		while (startPos.x >= grid.transform.position.x && startPos.x <= grid.transform.position.x + grid_width  && startPos.z >= grid.transform.position.z && startPos.z <= grid.transform.position.z + grid_height) {
				startPos -= new Vector3(direction.x, 0, direction.y);
		}
		// Grid offset
		startPos -= new Vector3(0, 0, 0.5f);

		wreckingBallSprite.transform.position += startPos;

		Vector3 endPos = startPos;
		endPos = startPos + new Vector3(direction.x * grid_width, 0, direction.y * grid_height);

		float i = 0;
		float rate = 1.0f / swingTime;

		wreckingBallSprite.transform.GetChild(0).eulerAngles += new Vector3((60 * direction.y)-40, 0, -60 * direction.x);

		Vector3 startRotation = new Vector3();
		Vector3 endRotation = new Vector3(-120 * direction.y, 0, 120 * direction.x);
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
