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

public class ButtonEventArgs : EventArgs {
    public InputButton button;
    public int playerNum;

    public ButtonEventArgs(InputButton button, int playerNum) {
        this.button = button;
        this.playerNum = playerNum;
    }
}

public interface IGameInput {

    event EventHandler green;
    event EventHandler red;
    event EventHandler yellow;
    event EventHandler blue;
    event EventHandler orange;

    event EventHandler green_strum;
    event EventHandler red_strum;
    event EventHandler yellow_strum;
    event EventHandler blue_strum;
    event EventHandler orange_strum;

    event EventHandler whammy_down;
    event EventHandler whammy_up;

}
