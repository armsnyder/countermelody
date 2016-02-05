using UnityEngine;
using System.Collections;

public class UnitActionMessage {
	public InputButton Color { get; set; }
	public int PlayerNumber { get; set; }
	public Vector2 Direction { get; set; }
	public UnitActionMessageType ActionType { get; set; }
}

public enum UnitActionMessageType {
	MOVE,
	ATTACK,
	SELECT
}

public abstract class Interpreter : MonoBehaviour {

}
