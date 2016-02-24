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
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
        this.UnMark();
	}

	void Defend(Unit other, int damage, float defenseModifier) {
		MarkAsDefending(other);
		HitPoints -= Mathf.Clamp(damage - (int)(DefenceFactor * defenseModifier), 1, damage);
		Vector2 viewportPoint = Camera.main.WorldToViewportPoint(new Vector3((float)other.transform.position.x, (float)(other.transform.position.y + 3), (float)other.transform.position.z));
		Text damage_display = GameObject.Find ("Damage").GetComponent<Text> ();
		damage_display.rectTransform.anchorMin = viewportPoint;  
 		damage_display.rectTransform.anchorMax = viewportPoint; 
		damage_display.alignment = TextAnchor.MiddleCenter;
		damage_display.text = Mathf.Clamp(damage - (int)(DefenceFactor * defenseModifier), 1, damage).ToString();
		StartCoroutine("DisplayDamage");	

		UpdateHealthBar();

		if (HitPoints < 0)
			OnDestroyed();
	}

	private IEnumerator DisplayDamage() {
		yield return new WaitForSeconds(0.5F);
		Text damage_display = GameObject.Find ("Damage").GetComponent<Text> ();
		damage_display.text = "";
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
}
