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
    public Color LeadingColor;
	public GameObject trim;
	public MessageRouter MessageRouter;

    public override void Initialize()
    {
        base.Initialize();
        transform.position += new Vector3(0, 0, -1);
        if(PlayerNumber == 0) {
            LeadingColor = Color.black;
        }
        else {
            LeadingColor = Color.white;
        }
        GetComponent<Renderer>().material.color = LeadingColor;
        AttackFactor = 20;
		AddTrim();
        this.UnMark();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
	}

	protected override void Defend(Unit other, int damage) {
		base.Defend(other, damage);
		UpdateHealthBar();
	}

	public void UpdateHealthBar() {
		if (GetComponentInChildren<Image>() != null) {
			GetComponentInChildren<Image>().transform.localScale = new Vector3((float)((float)HitPoints / (float)TotalHitPoints), 1, 1);
			GetComponentInChildren<Image>().color = Color.Lerp(Color.red, Color.green,
				(float)((float)HitPoints / (float)TotalHitPoints));
		}
	}

	private void AddTrim() {
		GameObject Trim = Instantiate(trim);
		Trim.transform.parent = transform;
		Trim.transform.localPosition = new Vector3(0, 0, 0);
		foreach (Renderer i in Trim.GetComponentsInChildren<Renderer>()) {
			i.material.color = unitColor;
		}
		MovementPoints = int.MaxValue;
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
        GetComponent<Renderer>().material.color = LeadingColor + Color.red ;
    }

    public override void MarkAsSelected()
    {
        GetComponent<Renderer>().material.color = LeadingColor + Color.green;
    }

    public override void UnMark()
    {
        GetComponent<Renderer>().material.color = LeadingColor;
    }
}
