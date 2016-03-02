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
		for (int i = 0; i < UnitsParent.childCount; i++) {
			MelodyUnit unit = UnitsParent.GetChild (i).GetComponent<MelodyUnit> ();
			if (unit == null)
				continue;
			GameObject replacement = characterSelectManager.chosen [unit.PlayerNumber, (int)unit.ColorButton];
			MelodyUnit replacementUnit = replacement.GetComponent<MelodyUnit> ();
			FieldInfo[] properties = unit.GetType ().GetFields ();
			// Copy unit values from prefab:
			List<string> skip = new List<string> { "playernumber", "unitcolor", "colorbutton" };
			foreach (FieldInfo property in properties) {
				if (skip.Contains (property.Name.ToLower ()))
					continue;
				if (!property.IsPublic) // If not public, skip
					continue;
				property.SetValue (unit, property.GetValue (replacementUnit));
			}
			// Replace animator:
			UnitsParent.GetChild (i).GetComponentInChildren<Animator> ().runtimeAnimatorController = 
				replacement.GetComponentInChildren<Animator> ().runtimeAnimatorController;
			// Replace material:
			UnitsParent.GetChild (i).GetComponentInChildren<SpriteRenderer> ().material = 
				Instantiate (replacement.GetComponentInChildren<SpriteRenderer> ().sharedMaterial);
			// Replace special move:
			Destroy(UnitsParent.GetChild (i).gameObject.GetComponent<SpecialMoveBase>());
			SpecialMoveBase replacementSpecial = replacement.GetComponent<SpecialMoveBase> ();
			if (replacementSpecial == null)
				continue;
			UnitsParent.GetChild (i).gameObject.AddComponent (replacementSpecial.GetType ());
			SpecialMoveBase addedSpecial = UnitsParent.GetChild (i).GetComponent<SpecialMoveBase> ();
			properties = replacementSpecial.GetType ().GetFields ();
			foreach (FieldInfo property in properties) {
				if (!property.IsPublic) // If not public, skip
					continue;
				property.SetValue (addedSpecial, property.GetValue (replacementSpecial));
			}
		}
	}
}
