using UnityEngine;
using System.Collections;
using System;
using Frictionless;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
	private Dictionary<int, MelodyUnit> SelectedUnit;
    public CMCellGrid GameBoard;

	private MessageRouter MessageRouter;

	void Awake() {
		SelectedUnit = new Dictionary<int, MelodyUnit> ();
	}
	
    void Start()
    {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<SwitchPlayerMessage> (OnSwitchPlayer);
    }

	void OnUnitAction(UnitActionMessage m) {
		switch (m.ActionType) {
			case UnitActionMessageType.SELECT:
				SwitchSelection(m.Color, m.PlayerNumber);
				break;
			case UnitActionMessageType.MOVE:
				MoveUnit(m.Direction, m.PlayerNumber);
				break;
			case UnitActionMessageType.ATTACK:
				Attack(m.Color, m.PlayerNumber);
				break;
		}
	}

	void SwitchSelection(InputButton color, int playerNumber) {
		if (SelectedUnit.ContainsKey(playerNumber)) {
			UncolorDirections (SelectedUnit[playerNumber].Cell);
		}
		
		SelectedUnit[playerNumber] = (GameBoard.Units.Find(c => (c.PlayerNumber == playerNumber) && 
			((c as MelodyUnit).ColorButton == color)) as MelodyUnit);

		if (SelectedUnit.ContainsKey(playerNumber)) {
			ColorDirections (SelectedUnit[playerNumber].Cell);
		}
	}

	void MoveUnit(Vector2 direction, int playerNumber) {
		if (SelectedUnit.ContainsKey(playerNumber)) {
			Cell destination = GameBoard.Cells.Find(c => c.OffsetCoord == 
				SelectedUnit[playerNumber].Cell.OffsetCoord + direction);
			if (destination && !destination.IsTaken) {
				UncolorDirections (SelectedUnit[playerNumber].Cell);
				SelectedUnit[playerNumber].Move(destination, 
					SelectedUnit[playerNumber].FindPath(GameBoard.Cells, destination));
				ColorDirections (destination);
				SelectedUnit[playerNumber].MovementPoints = int.MaxValue; // TODO: Wow such hack
			} else {
				MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, 
					ActionType = UnitActionMessageType.MOVE });
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
//		SelectedUnit[playerNumber] = GameBoard.Units[0] as MelodyUnit;
		MelodyUnit recipient = GameBoard.Units.Find(c => 
			(c.PlayerNumber != playerNumber) && 
			((c as MelodyUnit).ColorButton == color) &&
			(Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[0] - c.Cell.OffsetCoord[0])) <= 1 && 
			(Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[1] - c.Cell.OffsetCoord[1])) <= 1)
			as MelodyUnit;
		if (recipient) {
			SelectedUnit[playerNumber].DealDamage(recipient);
		} else {
			MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.ATTACK });
		}
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		foreach (MelodyUnit cur in SelectedUnit.Values) {
			UncolorDirections (cur.Cell);
		}
		if (SelectedUnit.ContainsKey (m.PlayerNumber)) {
			ColorDirections (SelectedUnit [m.PlayerNumber].Cell);
		}
	}
}
