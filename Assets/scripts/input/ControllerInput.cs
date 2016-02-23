using UnityEngine;
using System.Collections;
using System;

public enum InputButton {
    GREEN = 0,
    RED = 1,
    YELLOW = 2,
    BLUE = 3,
    ORANGE = 4,
	STRUM,
    WHAMMY,
    PLUS,
    MINUS,
    UP,
    DOWN,
    LEFT,
    RIGHT,
	NONE
}

public class ButtonInputMessage {
	public InputButton Button { get; set; }
	public int PlayerNumber { get; set; }
}

public class ButtonDownMessage : ButtonInputMessage {}

public class ButtonUpMessage : ButtonInputMessage {}

public abstract class ControllerInput : MonoBehaviour {

	[SerializeField]
	protected int PlayerNumber;

	public ControllerInput(int playerNumber) {
		this.PlayerNumber = playerNumber;
	}
}
