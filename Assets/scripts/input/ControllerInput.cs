using UnityEngine;
using System.Collections;
using System;

public enum InputButton {
    GREEN,
    RED,
    YELLOW,
    BLUE,
    ORANGE,
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
