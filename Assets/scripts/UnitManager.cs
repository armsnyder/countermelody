using UnityEngine;
using System.Collections;
using System;
using Frictionless;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    private MelodyUnit SelectedUnit;
    public CMCellGrid GameBoard;

	private MessageRouter MessageRouter;
	
    void Start()
    {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
    }

	void OnUnitAction(UnitActionMessage m) {
		switch (m.ActionType) {
			case UnitActionMessageType.SELECT:
				SwitchSelection(m.Color, m.PlayerNumber);
				break;
			case UnitActionMessageType.MOVE:
				MoveUnit(m.Direction);
				break;
			case UnitActionMessageType.ATTACK:
				Attack(m.Color, m.PlayerNumber);
				break;
		}
	}

	void SwitchSelection(InputButton color, int playerNumber) {
		SelectedUnit = (GameBoard.Units.Find(c => (c.PlayerNumber == playerNumber) && ((c as MelodyUnit).ColorButton == color)) as MelodyUnit);
	}

	void MoveUnit(Vector2 direction) {
		if (SelectedUnit) {
			Cell destination = GameBoard.Cells.Find(c => c.OffsetCoord == SelectedUnit.Cell.OffsetCoord + direction);
			if (destination && !destination.IsTaken) {
				UncolorDirections (SelectedUnit.Cell);
				SelectedUnit.Move(destination, SelectedUnit.FindPath(GameBoard.Cells, destination));
				ColorDirections (destination);
			} else {
				MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.MOVE });
			}
		}
	}

	public void UncolorDirections(Cell cell) {
		List<Cell> neighbors = cell.GetNeighbours (GameBoard.Cells);
		foreach (Cell neighbor in neighbors) {
			neighbor.UnMark ();
		}
	}

	public void ColorDirections(Cell cell) {
		List<Cell> neighbors = cell.GetNeighbours (GameBoard.Cells);
		foreach (Cell neighbor in neighbors) {
			if (neighbor.IsTaken)
				continue;
			Vector2 offset = neighbor.OffsetCoord - cell.OffsetCoord;
			if (offset.x < 0) {
				(neighbor as CMCell).SetColor (Color.green);
			} else if (offset.x > 0) {
				(neighbor as CMCell).SetColor (Color.blue);
			} else if (offset.y < 0) {
				(neighbor as CMCell).SetColor (Color.yellow);
			} else {
				(neighbor as CMCell).SetColor (Color.red);
			}
		}	
	}

	void Attack(InputButton color, int playerNumber) {
		SelectedUnit = GameBoard.Units[0] as MelodyUnit;
		MelodyUnit recipient = GameBoard.Units.Find(c => 
			(c.PlayerNumber != playerNumber) && 
			((c as MelodyUnit).ColorButton == color) &&
 			(Math.Abs(SelectedUnit.Cell.OffsetCoord[0] - c.Cell.OffsetCoord[0])) <= 1 && 
			(Math.Abs(SelectedUnit.Cell.OffsetCoord[1] - c.Cell.OffsetCoord[1])) <= 1)
			as MelodyUnit;
		if (recipient) {
			SelectedUnit.DealDamage(recipient);
		} else {
			MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.ATTACK });
		}
	}
}
