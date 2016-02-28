using UnityEngine;
using System.Collections;

public enum UnitChar {
	ELVIS,
	JACKSON,
	MILEY,
	BEATLES,
	KANYE,
	MIKU
}

public class UnitCharManager {
	public static RuntimeAnimatorController ToAnimator(UnitChar unitChar) {
		switch (unitChar) {
		case UnitChar.ELVIS:
			return Resources.Load<RuntimeAnimatorController> ("controllers/elvis");
		case UnitChar.JACKSON:
			return Resources.Load<RuntimeAnimatorController> ("controllers/jackson");
		default:
			return null;
		}
	}

	/// <summary>
	/// Returns the number of seconds into a unit's dance animation where the unit is "on" the beat
	/// </summary>
	/// <returns>The dance ease in time.</returns>
	/// <param name="unitChar">Unit char.</param>
	public static float GetDanceEaseIn(UnitChar unitChar) {
		switch (unitChar) {
		default:
			return 1f / 12; // assumes 1 frame at 12 fps
		}
	}
}
