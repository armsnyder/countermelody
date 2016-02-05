using UnityEngine;
using System.Collections;
using System;

public enum InputButton {
    GREEN,
    RED,
    YELLOW,
    BLUE,
    ORANGE,
    GREEN_STRUM,
    RED_STRUM,
    YELLOW_STRUM,
    BLUE_STRUM,
    ORANGE_STRUM,
    WHAMMY_DOWN,
    WHAMMY_UP,
    PLUS,
    MINUS
}

public class ButtonInputMessage {
	public InputButton Button { get; set; }
	public int PlayerNumber { get; set; }
}

public abstract class ControllerInput : MonoBehaviour {

	[SerializeField]
	protected int PlayerNumber;

	public ControllerInput(int playerNumber) {
		this.PlayerNumber = playerNumber;
	}
}
