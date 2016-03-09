using UnityEngine;
using System.Collections;
using System;
using Frictionless;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
	private Dictionary<int, MelodyUnit> SelectedUnit;
    private CMCellGrid GameBoard;
	private GameManager GameManager;
	private MessageRouter MessageRouter;
	private StateManager StateManager;
	public GameObject replenishDisplayPrefab;
	public GameObject canvas;

	void Awake() {
		SelectedUnit = new Dictionary<int, MelodyUnit> ();
	}
	
    void Start()
    {
		ServiceFactory.Instance.RegisterSingleton<UnitManager>(this);
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.AddHandler<SwitchPlayerMessage>(OnSwitchPlayer);
		MessageRouter.AddHandler<UnitDeathMessage>(OnUnitDeath);
		MessageRouter.AddHandler<StateChangeMessage>(OnStateChange);
		MessageRouter.AddHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.AddHandler<EndSpecialMoveMessage>(OnEndSpecial);
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

			//focus a spotlight on the selected unit
			RefocusSpotlight(SelectedUnit[i], i);

			if (i == GameManager.CurrentPlayer) {
				TurnOnSpotlight(i);
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
			case UnitActionMessageType.SPECIAL:
				UseSpecial(m.PlayerNumber);
				break;
		}
	}

	void UseSpecial(int playerNumber) {
		MessageRouter.RaiseMessage(new TriggerSpecialMoveMessage {
			unit = SelectedUnit[playerNumber]
		});
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
			RefocusSpotlight(SelectedUnit[playerNumber], playerNumber);
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
		/// Just a note that the code here is a bit asymmetrical: The logic for healing is in a separate Healer
		/// script, whereas the logic for attacking is within the if statement. A better design would be to put the
		/// attack logic elsewhere, like in the MelodyUnit class, but for now, this works fine.
		if (SelectedUnit[playerNumber].GetComponent<Healer>() == null || 
			!SelectedUnit[playerNumber].GetComponent<Healer>().OnHealAction(color)) {
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
						DefendingUnit = recipient,
						battleType = BattleType.ATTACK
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
					DefendingUnit = recipient,
					battleType = BattleType.ATTACK
				});
			} else {
				MessageRouter.RaiseMessage(new RejectActionMessage { PlayerNumber = GameBoard.CurrentPlayerNumber, ActionType = UnitActionMessageType.ATTACK });
			}
		}
	}

	void MarkAttackRange() {
		//find all cells in attack range
		int currentPlayer = GameManager.CurrentPlayer;
		Healer healerComponent = SelectedUnit [currentPlayer].GetComponent<Healer> ();
		// TODO: Differentiate between attack and heal range
		int range = Mathf.Max (SelectedUnit [currentPlayer].AttackRange, healerComponent == null ? 0 : healerComponent.range);
		List<Cell> AttackableCells = GameBoard.Cells.FindAll(c => 
		{
			Vector2 offset = c.OffsetCoord - SelectedUnit[currentPlayer].Cell.OffsetCoord;
			return Math.Abs(offset[0]) + Math.Abs(offset[1]) <= range;
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

	public void UnHighlightAll() {
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

		foreach (int i in SelectedUnit.Keys) {
			TurnOffSpotlight(i);
		}

		if (SelectedUnit.ContainsKey (m.PlayerNumber)) {

			if (!GameBoard.Units.Contains(SelectedUnit[m.PlayerNumber])){
				SelectedUnit[m.PlayerNumber] = GameBoard.Units.Find(c => c.PlayerNumber == m.PlayerNumber) as MelodyUnit;
				RefocusSpotlight(SelectedUnit[m.PlayerNumber], m.PlayerNumber);
			}

			MarkAttackRange();
			TurnOnSpotlight(m.PlayerNumber);
		}
	}

	IEnumerator LoadSceneAfterFrame(string nextScene, string winMessage) {
		Text WinText = GameObject.Find ("WinnerText").GetComponent<Text> ();
		WinText.text = winMessage;
		yield return new WaitForSeconds (5);
		WinText.text = "";
		SceneManager.LoadScene (nextScene);
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<UnitActionMessage>(OnUnitAction);
		MessageRouter.RemoveHandler<SwitchPlayerMessage>(OnSwitchPlayer);
		MessageRouter.RemoveHandler<UnitDeathMessage>(OnUnitDeath);
		MessageRouter.RemoveHandler<StateChangeMessage>(OnStateChange);
		MessageRouter.RemoveHandler<ExitBattleMessage> (OnExitBattle);
		MessageRouter.RemoveHandler<EndSpecialMoveMessage>(OnEndSpecial);
	}

	bool CheckWin() {
		bool zeroHasNoUnits = true;
		bool oneHasNoUnits = true;
		foreach(Unit u in GameBoard.Units) {
			if (u.PlayerNumber == 0) {
				zeroHasNoUnits = false;
			}
			if (u.PlayerNumber == 1) {
				oneHasNoUnits = false;
			}
		}
		string nextScene = "MainMenu";		
		if (zeroHasNoUnits && oneHasNoUnits) {
			MessageRouter.RaiseMessage (new SceneChangeMessage () {
				currentScene = SceneManager.GetActiveScene ().name,
				nextScene = nextScene
			});
			StartCoroutine (RemoveHandlers ());
			StartCoroutine(LoadSceneAfterFrame (nextScene, "Tie Game!"));
			return false;		
		}
		else if (zeroHasNoUnits) {
			MessageRouter.RaiseMessage (new SceneChangeMessage () {
				currentScene = SceneManager.GetActiveScene ().name,
				nextScene = nextScene
			});
			StartCoroutine (RemoveHandlers ());
			StartCoroutine(LoadSceneAfterFrame (nextScene, "Player One Wins!"));
			return false;			
		}
		else if (oneHasNoUnits) {
			MessageRouter.RaiseMessage (new SceneChangeMessage () {
				currentScene = SceneManager.GetActiveScene ().name,
				nextScene = nextScene
			});
			StartCoroutine (RemoveHandlers ());
			StartCoroutine(LoadSceneAfterFrame (nextScene, "Player Zero Wins!"));
			return false;
		}
		return true;
	}

	void OnUnitDeath(UnitDeathMessage m) {
		GameBoard.Units.Remove (m.unit);		
		if (CheckWin()) {
			UnHighlightAll();
			SelectedUnit[m.unit.PlayerNumber] = GameBoard.Units.Find(c => c.PlayerNumber == m.unit.PlayerNumber) as MelodyUnit;
			RefocusSpotlight(SelectedUnit[m.unit.PlayerNumber], m.unit.PlayerNumber);
		}
	}

	void OnStateChange(StateChangeMessage m) {
		if (GameBoard == null)
			return;
		switch (m.State) {
			case State.MoveState:
				UnHighlightAll();
				MarkAttackRange();
				break;
		}
	}

	void OnExitBattle(ExitBattleMessage m) {
		// Battle is over. Deal damage according to results.
		// TODO: Consider whether we want the defending unit to deal damage
		switch (m.battleType) {
			case BattleType.ATTACK:
				m.AttackingUnit.DealDamage(m.DefendingUnit, m.AttackerHitPercent, m.DefenderHitPercent);
				if (m.DefendingUnit.HitPoints > 0 && m.DefendingUnit.AttackRange >= 
					(Math.Abs(m.DefendingUnit.Cell.OffsetCoord[0] - m.AttackingUnit.Cell.OffsetCoord[0])) + 
					(Math.Abs(m.DefendingUnit.Cell.OffsetCoord[1] - m.AttackingUnit.Cell.OffsetCoord[1]))) {

					m.DefendingUnit.DealDamage(m.AttackingUnit, m.DefenderHitPercent, m.AttackerHitPercent);
				}
					
				break;
			case BattleType.HEAL:
				m.AttackingUnit.GetComponent<Healer> ().Heal (m.DefendingUnit, m.AttackerHitPercent);
				break;
		}
	}

	void RefocusSpotlight(MelodyUnit u, int playerNumber) {
		MessageRouter.RaiseMessage(new SpotlightChangeMessage() {
			focusedOnUnit = u,
			type = ChangeType.SWITCH,
			PlayerNumber = playerNumber
		});
	}

	void TurnOnSpotlight(int playerNumber) {
		MessageRouter.RaiseMessage(new SpotlightChangeMessage() {
			type = ChangeType.ON,
			PlayerNumber = playerNumber
		});
	}

	void TurnOffSpotlight(int playerNumber) {
		MessageRouter.RaiseMessage(new SpotlightChangeMessage() {
			type = ChangeType.OFF,
			PlayerNumber = playerNumber
		});
	}

	void OnEndSpecial(EndSpecialMoveMessage m) {
		MarkAttackRange();
	}
}
