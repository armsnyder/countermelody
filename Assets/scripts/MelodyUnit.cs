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
			while (new Vector2(transform.position.x,transform.position.z) != 
				new Vector2(cell.transform.position.x,cell.transform.position.z))
			{
				transform.position = Vector3.MoveTowards(transform.position, 
					new Vector3(cell.transform.position.x, transform.position.y, cell.transform.position.z),
					Time.deltaTime * MovementSpeed);
				yield return 0;
			}
		}

		isMoving = false;
	}
}
