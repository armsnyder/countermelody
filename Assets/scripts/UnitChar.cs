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
}
