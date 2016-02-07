using UnityEngine;
using System.Collections;
using System;
using Frictionless;

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
				SelectedUnit.Move(destination, SelectedUnit.FindPath(GameBoard.Cells, destination));
			} else {
				MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.MOVE });
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
