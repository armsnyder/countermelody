using UnityEngine;
using System.Collections;

public class CMCell : Square {

	void Start() {
		OffsetCoord = new Vector2 (transform.position.x, transform.position.z);
	}

	public override Vector3 GetCellDimensions()
	{
		return GetComponent<Renderer>().bounds.size;
	}

	public override void MarkAsHighlighted()
	{
		GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f); ;
	}

	public override void MarkAsPath()
	{
		GetComponent<Renderer>().material.color = Color.green;
	}

	public override void MarkAsReachable()
	{
		GetComponent<Renderer>().material.color = Color.yellow;
	}

	public override void UnMark()
	{
		GetComponent<Renderer>().material.color = Color.black;
	}

	public void SetColor(Color color) {
		GetComponent<Renderer> ().material.color = color;
	}
}
