using UnityEngine;
using System.Collections.Generic;
using Frictionless;

/// <summary>
/// Units with this component are capable of healing other units
/// </summary>
public class Healer : MonoBehaviour {

	public int amount; // Amount to heal
	public int range; // Range of heal

	private MelodyUnit unit;

	void Start() {
		unit = GetComponent<MelodyUnit> ();
	}

	/// <summary>
	/// This function is called from UnitManager upon receiving an ATTACK action if the unit is a Healer type. It 
	/// attempts to find a nearby unit on the same team to heal and initiates a "battle" if it finds a unit. If it
	/// does not, it returns false, and control can be passed to attack code elsewhere.
	/// </summary>
	/// <param name="color">Color.</param>
	public bool OnHealAction(InputButton color) {
		if (unit.ActionPoints <= 0)
			return false;
		
		CMCellGrid board = ServiceFactory.Instance.Resolve<CellGrid> () as CMCellGrid;
		MelodyUnit recipient = null;

		if (color == InputButton.NONE) {
			List<Unit> recipients = board.Units.FindAll (c => (c != unit) && (c.PlayerNumber == unit.PlayerNumber) &&
			                        (Mathf.Abs (unit.Cell.OffsetCoord [0] - c.Cell.OffsetCoord [0])) +
			                        (Mathf.Abs (unit.Cell.OffsetCoord [1] - c.Cell.OffsetCoord [1])) <= range);
			if (recipients.Count == 0)
				return false;
			int lowestValue = int.MaxValue;
			foreach (Unit r in recipients) {
				if (r.HitPoints < lowestValue) {
					recipient = r as MelodyUnit;
					lowestValue = r.HitPoints;
				}
			}
			if (recipient.HitPoints == recipient.MaxHitPoints)
				return false;
		} else {
			recipient = board.Units.Find (c => (c.PlayerNumber == unit.PlayerNumber) &&
			((c as MelodyUnit).ColorButton == color) &&
			(Mathf.Abs (unit.Cell.OffsetCoord [0] - c.Cell.OffsetCoord [0])) +
			(Mathf.Abs (unit.Cell.OffsetCoord [1] - c.Cell.OffsetCoord [1])) <= range) as MelodyUnit;
			if (recipient == null)
				return false;
		}
		ServiceFactory.Instance.Resolve<MessageRouter> ().RaiseMessage (new EnterBattleMessage () {
			AttackingUnit = unit,
			DefendingUnit = recipient,
			battleType = BattleType.HEAL
		});
		return true;
	}

	public void Heal(MelodyUnit other, float healModifier) {
		unit.MarkAsAttacking(other);
		unit.ActionPoints--;
		other.Replenish (unit, (int)Mathf.Round (amount * healModifier));

		if (unit.ActionPoints == 0) {
			unit.SetState(new UnitStateMarkedAsFinished(unit));
			unit.MovementPoints = 0;
		}
	}
}
