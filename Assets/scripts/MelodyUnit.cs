using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Frictionless;
using UnityEngine.UI;

public class UnitDeathMessage {
	public MelodyUnit unit;
}

public class MelodyUnit : Unit {
	public InputButton ColorButton;
	public Color unitColor;
	public MessageRouter MessageRouter;
	public float hopHeight = 10;
	public UnitChar character;

    public override void Initialize()
    {
        base.Initialize();
		// Set Unit Color
		unitColor.a = 1f; // Override alpha channel
		GetComponentInChildren<SpriteRenderer> ().material.SetColor("_Color", unitColor);
		// Invert colors
		if (PlayerNumber > 0) {
			GetComponentInChildren<SpriteRenderer> ().material.EnableKeyword ("INVERT_ON");
		} else {
			GetComponentInChildren<SpriteRenderer> ().material.EnableKeyword ("INVERT_OFF");
		}
		// Set the character
		GetComponentInChildren<Animator>().runtimeAnimatorController = UnitCharManager.ToAnimator(character);
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<BeatCenterMessage> (OnBeatCenter);
        this.UnMark();
	}

	void Defend(Unit other, int damage, float defenseModifier) {
		MarkAsDefending(other);
		int damageTaken = Mathf.Max(damage - (int)(DefenceFactor * defenseModifier), 0);
		HitPoints -= damageTaken;

		UpdateHealthBar();

		if (HitPoints < 0)
			OnDestroyed();
	}
	
	public void UpdateHealthBar() {
		if (GetComponentInChildren<Image>() != null) {
			GetComponentInChildren<Image>().transform.localScale = new Vector3((float)((float)HitPoints / (float)TotalHitPoints), 1, 1);
			GetComponentInChildren<Image>().color = Color.Lerp(Color.red, Color.green,
				(float)((float)HitPoints / (float)TotalHitPoints));
		}
	}

    public int GetActionPoints() {
        return TotalActionPoints;
    }

    public int GetAttackRange() {
        return AttackRange;
    }

    public override void MarkAsAttacking(Unit other)
    {      
    }

    public override void MarkAsDefending(Unit other)
    {       
    }

    public override void MarkAsDestroyed()
    {
		MessageRouter.RaiseMessage(new UnitDeathMessage {
			unit = this
		});
    }

    public override void MarkAsFinished()
    {
    }

    public override void MarkAsFriendly()
    {
    }

    public override void MarkAsReachableEnemy()
    {
		//TODO: Do something other than color to mark as reachable
    }

    public override void MarkAsSelected()
    {
    }

    public override void UnMark()
    {
    }

	protected override void OnDestroyed()
	{
		Cell.IsTaken = false;
		MarkAsDestroyed();
		StartCoroutine (QueuedDestroy ());
	}

	IEnumerator QueuedDestroy() {
		// Delay destroy until next frame to ensure we are not within a MessageRouter raised message loop
		yield return null;
		MessageRouter.RemoveHandler<BeatCenterMessage> (OnBeatCenter);
		GameObjectUtil.Destroy(gameObject);
	}

	public void DealDamage(MelodyUnit other, float attackModifier, float DefenseModifier)
	{
		if (isMoving)
			return;
		if (ActionPoints == 0)
			return;

		MarkAsAttacking(other);
		ActionPoints--;
		other.Defend(this, (int) (AttackFactor * attackModifier), DefenseModifier);

		if (ActionPoints == 0)
		{
			SetState(new UnitStateMarkedAsFinished(this));
			MovementPoints = 0;
		}  
	}

	protected override IEnumerator MovementAnimation(List<Cell> path)
	{
		isMoving = true;

		path.Reverse();
		foreach (var cell in path)
		{
			// Set sprite facing direction
			float MIN_DELTA_X = 0.1f;
			if (Math.Abs(transform.position.x - cell.transform.position.x) > MIN_DELTA_X) {
				GetComponentInChildren<SpriteRenderer> ().flipX = (transform.position - cell.transform.position).x > 0;
			}

			Vector3 startPosition = transform.position;
			float totalDistance = Vector3.Distance (transform.position, cell.transform.position);
			float moveProgress = 0;
			while (moveProgress < totalDistance) {
				moveProgress += Time.deltaTime * MovementSpeed;
				if (moveProgress > totalDistance)
					moveProgress = totalDistance;
				transform.position = Vector3.Lerp (startPosition, cell.transform.position, moveProgress / totalDistance);
				transform.position += new Vector3 
					(0f, (float)Math.Sin (Math.PI * (moveProgress / totalDistance)) * hopHeight, 0f);
				yield return 0;
			}
		}

		isMoving = false;
	}

	void OnBeatCenter(BeatCenterMessage m) {
		// Animate unit's beat animation on every beat
		float whenStartAnimate = 60f / m.BeatsPerMinute - UnitCharManager.GetDanceEaseIn(character);
		if (whenStartAnimate < 0)
			whenStartAnimate = 0f;
		StartCoroutine (QueuedDance (whenStartAnimate));
	}

	IEnumerator QueuedDance(float whenStartAnimate) {
		// Delay destroy until next frame to ensure we are not within a MessageRouter raised message loop
		yield return new WaitForSeconds(whenStartAnimate);
		GetComponentInChildren<Animator> ().SetTrigger ("beat");
	}
}
