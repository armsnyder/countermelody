using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class MelodyUnit : Unit {
	public InputButton ColorButton;
	public Color unitColor;
    public Color LeadingColor;
	public GameObject trim;
    public override void Initialize()
    {
        base.Initialize();
		MovementPoints = int.MaxValue; // TODO: make less hacky?
        transform.position += new Vector3(0, 0, -1);
        if(PlayerNumber == 0) {
            LeadingColor = Color.black;
        }
        else {
            LeadingColor = Color.white;
        }
        GetComponent<Renderer>().material.color = LeadingColor;
        AttackFactor = 100;
		AddTrim();
        this.UnMark();
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

    /*protected override void Defend(Unit other, int damage)
    {
        MarkAsDefending(other);
        HitPoints -= Mathf.Clamp(damage - DefenceFactor, 1, damage);  //Damage is calculated by subtracting attack factor of attacker and defence factor of defender. If result is below 1, it is set to 1.
                                                                      //This behaviour can be overridden in derived classes.
        if (UnitAttacked != null)
            UnitAttacked.Invoke(this, new AttackEventArgs(other, this, damage));

        if (HitPoints <= 0)
        {
            if (UnitDestroyed != null)
                UnitDestroyed.Invoke(this, new AttackEventArgs(other, this, damage));
            OnDestroyed();
        }
    }

    protected override void OnDestroyed()
    {
        Cell.IsTaken = false;
        MarkAsDestroyed();
        //Destroy(gameObject);
    }*/

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
    }

    public override void MarkAsFinished()
    {
    }

    public override void MarkAsFriendly()
    {
        GetComponent<Renderer>().material.color = LeadingColor + new Color(0.8f, 1, 0.8f);
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
