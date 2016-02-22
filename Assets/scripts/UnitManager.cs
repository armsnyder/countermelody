using UnityEngine;
using System.Collections;
using System;
using Frictionless;
using System.Collections.Generic;
using System.Linq;

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
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<EnterBeatWindowMessage> (OnEnterBeatWindow);
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
				MarkAttackRange();
			}
		}
		CreatePartyViews();
	}

	void CreatePartyViews() {
		Dictionary <int, List<Unit>> partyLists = new Dictionary<int, List<Unit>>();
		foreach(int i in SelectedUnit.Keys) {
			if (!partyLists.ContainsKey(i)) {
				partyLists.Add(i, new List<Unit>());
			}

			partyLists[i] = GameBoard.Units.FindAll(c => c.PlayerNumber == i);
			partyLists[i].Sort((x, y) => (x as MelodyUnit).ColorButton.CompareTo((y as MelodyUnit).ColorButton));
		}

		List<int> partys = partyLists.Keys.ToList();
		partys.Sort();
		float offset = 0;
		float UNITCAMERAWIDTH = 0.035f; //TODO: make this not a constant

		int maxPartySize = 0;
		foreach (int i in partyLists.Keys) {
			maxPartySize = Math.Max(maxPartySize, partyLists[i].Count);
		}
		float margin= (1 - (UNITCAMERAWIDTH*maxPartySize*partys.Count)) / (partys.Count-1);


		foreach (int i in partys) {
			foreach( Unit u in partyLists[i]) {
				Camera UnitCamera = u.GetComponentInChildren<Camera>();

				UnitCamera.rect = new Rect(offset, 0, UnitCamera.rect.width, UnitCamera.rect.height);
				offset += UnitCamera.rect.width;
			}
			offset += margin;
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
			UnHighlightAll();
		}

		MelodyUnit selection = (GameBoard.Units.Find(c => (c.PlayerNumber == playerNumber) && ((c as MelodyUnit).ColorButton == color)) as MelodyUnit);
		if (selection != null)
			SelectedUnit[playerNumber] = selection;
		else
			MessageRouter.RaiseMessage(new RejectActionMessage () {
				ActionType = UnitActionMessageType.SELECT,
				PlayerNumber = playerNumber
			});

		if (SelectedUnit.ContainsKey(playerNumber)) {
			MarkAttackRange();
		}
	}

	void MoveUnit(Vector2 direction, int playerNumber) {
		if (SelectedUnit.ContainsKey(playerNumber) && SelectedUnit[playerNumber]) {
			Cell destination = GameBoard.Cells.Find(c => c.OffsetCoord == 
				SelectedUnit[playerNumber].Cell.OffsetCoord + direction);
			if (destination && !destination.IsTaken) {
				UnHighlightAll();
				SelectedUnit[playerNumber].Move(destination, 
					SelectedUnit[playerNumber].FindPath(GameBoard.Cells, destination));
				MarkAttackRange();
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
		MelodyUnit recipient = null;
		List<Unit> recipients;
		if (color == InputButton.NONE) {
			recipients = GameBoard.Units.FindAll(c => 
			(c.PlayerNumber != playerNumber) && 
			(Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[0] - c.Cell.OffsetCoord[0])) + (Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[1] - c.Cell.OffsetCoord[1])) <= SelectedUnit[playerNumber].AttackRange);
            int lowestValue = int.MaxValue;
            if (recipients.Count > 0) {
            	recipient = recipients[0] as MelodyUnit;
            	lowestValue = recipient.HitPoints;
            }
            foreach(Unit r in recipients) {
            	if (r.HitPoints < lowestValue) {
            		recipient = r as MelodyUnit;
            		lowestValue = r.HitPoints;
            	}
            }
			if (recipient) {
				MessageRouter.RaiseMessage (new EnterBattleMessage () { 
					AttackingUnit = SelectedUnit [playerNumber],
					DefendingUnit = recipient
				});
			}
			else {
				MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.ATTACK });
			}
			return;
		}
		recipient = GameBoard.Units.Find(c => 
			(c.PlayerNumber != playerNumber) && 
			((c as MelodyUnit).ColorButton == color) &&
			(Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[0] - c.Cell.OffsetCoord[0])) + (Math.Abs(SelectedUnit[playerNumber].Cell.OffsetCoord[1] - c.Cell.OffsetCoord[1])) <= SelectedUnit[playerNumber].AttackRange) 
			as MelodyUnit;		
		if (recipient && SelectedUnit[playerNumber]) {
			// Passes control to BattleManager
			MessageRouter.RaiseMessage (new EnterBattleMessage () { 
				AttackingUnit = SelectedUnit [playerNumber],
				DefendingUnit = recipient
			});
		} else {
			MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.ATTACK });
		}
	}

	void MarkAttackRange() {
		//find all cells in attack range
		int currentPlayer = GameManager.CurrentPlayer;
		List<Cell> AttackableCells = GameBoard.Cells.FindAll(c => 
		{
			Vector2 offset = c.OffsetCoord - SelectedUnit[currentPlayer].Cell.OffsetCoord;
			return Math.Abs(offset[0]) + Math.Abs(offset[1]) <= SelectedUnit[currentPlayer].AttackRange;
		});
		//highlight them
		foreach (Cell c in AttackableCells) {
			(c as CMCell).MarkAsHighlighted();
		}
		//highlight units that are in the range
		List<Unit> AttackableUnits = GameBoard.Units.FindAll(c => AttackableCells.Contains(c.Cell) && c.PlayerNumber != currentPlayer);
		foreach (Unit u in AttackableUnits) {
			(u as MelodyUnit).MarkAsReachableEnemy();
		}
	}

	void UnHighlightAll() {
		foreach(Cell c in GameBoard.Cells) {
			c.UnMark();
		}
		foreach(Unit u in GameBoard.Units) {
			u.UnMark();
		}
	}

	void OnSwitchPlayer(SwitchPlayerMessage m) {
		foreach (MelodyUnit cur in SelectedUnit.Values) {
			UnHighlightAll();
		}
		if (SelectedUnit.ContainsKey (m.PlayerNumber)) {
			MarkAttackRange();
		}
	}

	void OnUnitDeath(UnitDeathMessage m) {
		UnHighlightAll();
		SelectedUnit[m.unit.PlayerNumber] = GameBoard.Units.Find(c => c.PlayerNumber == m.unit.PlayerNumber) as MelodyUnit;
	}

	void OnStateChange(StateChangeMessage m) {
		UnHighlightAll();
		switch (m.State) {
			case State.MoveState:
				MarkAttackRange();
				break;
		}
	}

	void OnExitBattle(ExitBattleMessage m) {
		// Battle is over. Deal damage according to results.
		// TODO: Consider whether we want the defending unit to deal damage
		if (m.AttackingUnit == null || m.DefendingUnit == null) {
			return;
			// TODO: Figure out why this is ever null. Ignored for demo purposes only.
		}
		m.AttackingUnit.DealDamage(m.DefendingUnit, m.AttackerHitPercent, m.DefenderHitPercent);
//		if(m.DefendingUnit.HitPoints <= 0) {
//
//		}
	}

	void OnEnterBeatWindow(EnterBeatWindowMessage m) {
		// Animate unit's beat animation on every beat
		// TODO: Account for different lead-in times for different tempos, probably with a coroutine
		foreach (MelodyUnit u in GameBoard.Units) {
			if (u != null) {
				// TODO: Why are there null objects in the GameBoard.Units?
				u.GetComponentInChildren<Animator> ().SetTrigger ("beat");
			}
		}
	}
}
