using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using System;

public class Moonwalk : SpecialMoveBase {
	protected bool isMoving = false;
	public float moonwalkSpeed = 3;
	private readonly float MUSIC_FADE_OUT_TIME = 0.1f;

	protected override void Start() {
		base.Start();
		MessageRouter.AddHandler<ButtonDownMessage>(OnButtonDown);
		audioSource.loop = true;
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
				StartCoroutine(MovementAnimation(BoardInterpreter.DirectionToVector(m.Button)));
				StartCoroutine (PlayMusic ());
				break;
		}
	}

	IEnumerator PlayMusic() {
		audioSource.Play ();
		while (isMoving) {
			yield return null;
		}
		float elapsedFade = 0;
		while (elapsedFade < MUSIC_FADE_OUT_TIME) {
			audioSource.volume = (1 - elapsedFade / MUSIC_FADE_OUT_TIME) * musicVolume;
			elapsedFade += Time.deltaTime;
			yield return null;
		}
		audioSource.Stop ();
		audioSource.volume = musicVolume;
	}

	protected override IEnumerator DoSpecialMove() {
		StartSpecialMove();

		while (inputStartTime + inputWaitTime > Time.time) {
			if (hasButtonPressed) {
				break;
			}
			yield return null;
		}

		while (isMoving || audioSource.isPlaying) {
			yield return null;
		}
		base.EndSpecialMove();
	}

	protected override void HighlightSpecial() {
		Vector2[] directions = new Vector2[] { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };

		
		foreach (Vector2 dir in directions) {
			Cell curr = GetComponent<MelodyUnit>().Cell;
			Cell next = ServiceFactory.Instance.Resolve<CellGrid>().Cells.Find(c => c.OffsetCoord == curr.OffsetCoord + dir);
			while (next != null && !next.IsTaken) {
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
				moveProgress += Time.deltaTime * moonwalkSpeed;
				if (moveProgress > totalDistance)
					moveProgress = totalDistance;
				transform.position = Vector3.Lerp(startPosition, neighbor.transform.position, moveProgress / totalDistance);
				yield return 0;
			}
			GetComponent<MelodyUnit>().Cell.IsTaken = false;
			GetComponent<MelodyUnit>().Cell = neighbor;
			neighbor = ServiceFactory.Instance.Resolve<CellGrid>().Cells.Find(
				c => c.OffsetCoord == neighbor.OffsetCoord + direction && !c.IsTaken);
		}

		isMoving = false;
	}

}
