using UnityEngine;
using System.Collections;
using System;
using Frictionless;

public class UnitManager : MonoBehaviour
{
    private Unit SelectedUnit;
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
				SwitchSelection(m.Color);
				break;
			case UnitActionMessageType.MOVE:
				MoveUnit(m.Direction);
				break;
			case UnitActionMessageType.ATTACK:
				Attack(m.Color);
				break;
		}
	}

	void SwitchSelection(InputButton color) {

	}

	void MoveUnit(Vector2 direction) {
		SelectedUnit = GameBoard.Units[0];
		Cell destination = GameBoard.Cells.Find(c => c.OffsetCoord == SelectedUnit.Cell.OffsetCoord + direction);
		if (!destination.IsTaken) {
			SelectedUnit.Move(destination, SelectedUnit.FindPath(GameBoard.Cells, destination));
		} else {
			MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.MOVE });
		}
		
	}

	void Attack(InputButton color) {

	}

	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            SelectedUnit = GameBoard.Units[0];
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            SelectedUnit = GameBoard.Units[1];
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            SelectedUnit = GameBoard.Units[2];
        }
        
	}
}
