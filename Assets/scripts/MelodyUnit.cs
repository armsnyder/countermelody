using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Frictionless;

public class UnitDeathMessage {
	public MelodyUnit unit;
}

public class MelodyUnit : Unit {
	public InputButton ColorButton;
	public Color unitColor;
	public MessageRouter MessageRouter;
	public float hopHeight = 10;

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

        this.UnMark();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<EnterBeatWindowMessage> (OnEnterBeatWindow);
	}

	void Defend(Unit other, int damage, float defenseModifier) {
		MarkAsDefending(other);
		HitPoints -= Mathf.Clamp(damage - (int)(DefenceFactor * defenseModifier), 1, damage);

		UpdateHealthBar();

		if (HitPoints < 0)
			base.OnDestroyed();
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

	void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		// Animate unit's beat animation on every beat
		// TODO: Account for different lead-in times for different tempos, probably with a coroutine
		GetComponentInChildren<Animator> ().SetTrigger ("beat");
	}
}
