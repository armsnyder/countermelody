using UnityEngine;
using System.Collections.Generic;

public class CMCellGridGenerator : MonoBehaviour, ICellGridGenerator
{
    public Transform CellsParent;
	public GameObject cellPrefab;
	public Vector2 gridSize;
	public Vector2[] gaps;

    /// <summary>
    /// Returns cells that are already children of CellsParent object.
    /// </summary>
    public List<Cell> GenerateGrid()
    {
		List<Cell> ret = new List<Cell>();
		for (int i = 0; i < gridSize.x; i++) {
			for (int j = 0; j < gridSize.y; j++) {
				bool skip = false;
				foreach (Vector2 gap in gaps) {
					if (gap.x == i && gap.y == j) {
						skip = true;
						break;
					}
				}
				if (skip)
					continue;
				GameObject o = GameObjectUtil.Instantiate (cellPrefab, CellsParent.position + new Vector3(i, 0, j));
				o.transform.parent = CellsParent.transform;
				var cell = o.GetComponent<Cell>();
				if (cell != null)
					ret.Add(cell);
				else
					Debug.LogError("Invalid object in cells paretn game object");
			}
		}
        return ret;
    }

	void OnDrawGizmos() {
		// Draw lines
		Gizmos.color = Color.cyan;
		Vector3 offset = new Vector3 (-0.5f, 0, -0.5f);
		for (int i = 0; i <= gridSize.x; i++) {
			Gizmos.DrawLine (CellsParent.position + new Vector3 (i, 0, 0) + offset, 
				CellsParent.position + new Vector3 (i, 0, gridSize.y) + offset);
		}
		for (int j = 0; j <= gridSize.y; j++) {
			Gizmos.DrawLine (CellsParent.position + new Vector3 (0, 0, j) + offset, 
				CellsParent.position + new Vector3 (gridSize.x, 0, j) + offset);
		}
		// Draw gaps
		Gizmos.color = Color.black;
		foreach (Vector2 gap in gaps) {
			if (gap.x < gridSize.x && gap.y < gridSize.y) {
				Gizmos.DrawCube (CellsParent.position + new Vector3 (gap.x, 0, gap.y), new Vector3 (1, 0.01f, 1));
			}
		}
	}
}
