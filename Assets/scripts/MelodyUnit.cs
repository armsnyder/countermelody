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
        GetComponent<Renderer>().material.color = LeadingColor;
		AddTrim();
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
