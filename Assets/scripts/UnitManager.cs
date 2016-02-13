﻿using UnityEngine;
using System.Collections;
using System;
using Frictionless;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
	private Dictionary<int, MelodyUnit> SelectedUnit;
    private CMCellGrid GameBoard;
	private GameManager GameManager;
	private MessageRouter MessageRouter;
	private StateManager StateManager;

	void Awake() {
		SelectedUnit = new Dictionary<int, MelodyUnit> ();
	}
	
    void Start()
    {
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<SwitchPlayerMessage>(OnSwitchPlayer);
		MessageRouter.AddHandler<UnitDeathMessage>(OnUnitDeath);
		MessageRouter.AddHandler<StateChangeMessage>(OnStateChange);
		GameManager = ServiceFactory.Instance.Resolve<GameManager>();
		StartCoroutine("GetGameBoard");
    }

	private IEnumerator GetGameBoard() {
		while (!ServiceFactory.Instance.Resolve<CellGrid>()) {
			yield return null;
		}
		GameBoard = ServiceFactory.Instance.Resolve<CellGrid>() as CMCellGrid;
		for (int i = 0; i < GameManager.NumberOfPlayers; i++) {
			SelectedUnit[i] = GameBoard.Units.Find(c => 
			c.PlayerNumber == i && ((c as MelodyUnit).ColorButton == InputButton.GREEN)) as MelodyUnit;

			if (i == GameManager.CurrentPlayer) {
				ColorDirections(SelectedUnit[i].Cell);
			}
		}
	}

    public CMCellGrid getGrid() {
    	return GameBoard;
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
		if (SelectedUnit.ContainsKey(playerNumber) && SelectedUnit[playerNumber]) {
			UncolorDirections (SelectedUnit[playerNumber].Cell);
		}
		
		SelectedUnit[playerNumber] = (GameBoard.Units.Find(c => (c.PlayerNumber == playerNumber) && 
			((c as MelodyUnit).ColorButton == color)) as MelodyUnit);

		if (SelectedUnit.ContainsKey(playerNumber)) {
			ColorDirections (SelectedUnit[playerNumber].Cell);
		}
	}

	void MoveUnit(Vector2 direction, int playerNumber) {
		if (SelectedUnit.ContainsKey(playerNumber) && SelectedUnit[playerNumber]) {
			Cell destination = GameBoard.Cells.Find(c => c.OffsetCoord == 
				SelectedUnit[playerNumber].Cell.OffsetCoord + direction);
			if (destination && !destination.IsTaken) {
				UncolorDirections (SelectedUnit[playerNumber].Cell);
				SelectedUnit[playerNumber].Move(destination, 
					SelectedUnit[playerNumber].FindPath(GameBoard.Cells, destination));
				ColorDirections (destination);
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
		MelodyUnit recipient = GameBoard.Units.Find(c => 
			(c.PlayerNumber != playerNumber) && 
			((c as MelodyUnit).ColorButton == color) &&
			(Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[0] - c.Cell.OffsetCoord[0])) <= c.AttackRange && 
			(Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[1] - c.Cell.OffsetCoord[1])) <= c.AttackRange)
			as MelodyUnit;
		if (recipient && SelectedUnit[playerNumber]) {
			SelectedUnit[playerNumber].DealDamage(recipient);
			if(recipient.HitPoints <= 0) {

			}
		} else {
			MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.ATTACK });
		}
	}

	void MarkAttackRange() {
		//find all cells in attack range
		int currentPlayer = GameManager.CurrentPlayer;
		List<Cell> AttackableCells = GameBoard.Cells.FindAll(c => 
		{
			Vector2 offset = c.OffsetCoord - SelectedUnit[GameManager.CurrentPlayer].Cell.OffsetCoord;
			return Math.Abs(offset[0]) + Math.Abs(offset[1]) <= SelectedUnit[GameManager.CurrentPlayer].AttackRange;
		});
		// highlight them
		foreach (Cell c in AttackableCells) {
			(c as CMCell).MarkAsHighlighted();
		}
	}

	void UnHighlightCells() {
		foreach(Cell c in GameBoard.Cells) {
			c.UnMark();
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

	void OnUnitDeath(UnitDeathMessage m) {
		UncolorDirections(m.unit.Cell);
		SelectedUnit[m.unit.PlayerNumber] = GameBoard.Units.Find(c => c.PlayerNumber == m.unit.PlayerNumber) as MelodyUnit;
	}

	void OnStateChange(StateChangeMessage m) {
		UnHighlightCells();
		switch (m.State) {
			case State.AttackState:
				MarkAttackRange();
				break;
			case State.MoveState:
				ColorDirections(SelectedUnit[GameManager.CurrentPlayer].Cell);
				break;
		}

	}
}
