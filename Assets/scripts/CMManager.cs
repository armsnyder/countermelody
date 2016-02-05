using UnityEngine;
using System.Collections;
using System;

public class CMManager : MonoBehaviour
{
    public Unit _unit;
    public CMCellGrid _cellGrid;
	
    void Start()
    {
        Debug.Log("Press 'n' to end turn");
        Cell cur = _unit.Cell;
        //Debug.Log(_cellGrid.Cells.Count);
        /*foreach (var cell in _cellGrid.Cells)
        {
            Debug.Log("Here");
            if(cell.OffsetCoord[0] == _unit.Cell.OffsetCoord[0] && cell.OffsetCoord[0] + 1 == _unit.Cell.OffsetCoord[0]) {
                Debug.Log("Cur");
                cur = cell;
            }
        }*/
        //Debug.Log(cur.OffsetCoord);
        //var path = _unit.FindPath(_cellGrid.Cells, cur);
        //_unit.Move(cur, path);
    }

	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            _cellGrid.EndTurn();//User ends his turn by pressing "n" on keyboard.
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            Cell cur = _unit.Cell;
            //Debug.Log(_cellGrid.Cells.Count);
            foreach (var cell in _cellGrid.Cells) 
            {
                //Debug.Log(cell.OffsetCoord);
                //Debug.Log(_unit.Cell);
                Debug.Log(cell.OffsetCoord);
                if(cell.OffsetCoord[0] == _unit.Cell.OffsetCoord[0] && cell.OffsetCoord[1] + 1 == _unit.Cell.OffsetCoord[1]) {
                    Debug.Log("Cur");
                    cur = cell;
                    break;
                }
            }
            Debug.Log(cur.OffsetCoord);
            Debug.Log(_cellGrid.Cells.Count);
            Debug.Log(_unit.Cell.OffsetCoord);
            var path = _unit.FindPath(_cellGrid.Cells, cur);
            _unit.Move(cur, path);
        }
	}
}
