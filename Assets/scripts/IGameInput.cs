using UnityEngine;
using System.Collections;
using System;

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
