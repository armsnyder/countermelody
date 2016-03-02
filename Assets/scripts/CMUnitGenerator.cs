using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Frictionless;
using System.Reflection;


public class CMUnitGenerator : MonoBehaviour, IUnitGenerator
{
    public Transform UnitsParent;

	void Start() {
		ServiceFactory.Instance.RegisterSingleton<CMUnitGenerator> (this);
	}

    /// <summary>
    /// Returns units that are already children of UnitsParent object.
    /// </summary>
    public List<Unit> SpawnUnits(List<Cell> cells)
    {
		ReplaceDefaultUnitsWithChosenUnits ();

        List<Unit> ret = new List<Unit>();
        for (int i = 0; i < UnitsParent.childCount; i++)
        {
            var unit = UnitsParent.GetChild(i).GetComponent<Unit>();
            if(unit !=null)
            {
                var cell = cells.OrderBy(h => Math.Abs((h.transform.position - unit.transform.position).magnitude)).First();
                if (!cell.IsTaken)
                {
                    cell.IsTaken = true;
                    unit.Cell = cell;
					unit.transform.position = cell.transform.position;
                    unit.Initialize();
                    ret.Add(unit);
                }//Unit gets snapped to the nearest cell
                else
                {
                    GameObjectUtil.Destroy(unit.gameObject);
                }//If the nearest cell is taken, the unit gets destroyed.
            }
            else
            {
                Debug.LogError("Invalid object in Units Parent game object");
            }
            
        }
        return ret;
    }

	/// <summary>
	/// Replaces the pre-placed units in the scene with units chosen by the player in the Character Select screen.
	/// </summary>
	void ReplaceDefaultUnitsWithChosenUnits () {
		CharacterSelectManager characterSelectManager = ServiceFactory.Instance.Resolve<CharacterSelectManager> ();
		if (characterSelectManager == null)
			return; // If the Movement scene is started first and no characters have been properly chosen, abort!
		Transform ChosenParent = new GameObject("Units Parent").transform;
		for (int i = 0; i < UnitsParent.childCount; i++) {
			MelodyUnit unit = UnitsParent.GetChild (i).GetComponent<MelodyUnit> ();
			if (unit == null)
				continue;
			// Instantiate correct prefab:
			GameObject replacement = Instantiate (characterSelectManager.chosen [unit.PlayerNumber, (int)unit.ColorButton]);
			replacement.transform.parent = ChosenParent;
			replacement.transform.position = unit.transform.position;
			MelodyUnit replacementUnit = replacement.GetComponent<MelodyUnit> ();
			// Set new unit's color and player number:
			replacementUnit.ColorButton = unit.ColorButton;
			replacementUnit.unitColor = unit.unitColor;
			replacementUnit.PlayerNumber = unit.PlayerNumber;
			// Replace material:
			replacement.GetComponentInChildren<SpriteRenderer> ().material = 
				Instantiate (unit.GetComponentInChildren<SpriteRenderer> ().sharedMaterial);
		}
		// Swap out default units with newly constructed units:
		Destroy (UnitsParent.gameObject);
		UnitsParent = ChosenParent;
	}
}
