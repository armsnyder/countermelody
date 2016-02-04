using UnityEngine;
using System.Collections;
using System;

public class KeyboardInput : MonoBehaviour, IGameInput {



    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A)) 
        {
            if (green_strum != null)
            {
				green_strum(this, new ButtonEventArgs(InputButton.GREEN_STRUM, 0));
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (red_strum != null)
            {
				red_strum(this, new ButtonEventArgs(InputButton.RED_STRUM, 0));
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (yellow_strum != null)
            {
				yellow_strum(this, new ButtonEventArgs(InputButton.YELLOW_STRUM, 0));
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (blue_strum != null)
            {
				blue_strum(this, new ButtonEventArgs(InputButton.BLUE_STRUM, 0));
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (orange_strum != null)
            {
				orange_strum(this, new ButtonEventArgs(InputButton.ORANGE_STRUM, 0));
            }
        }
	
	}

    public event EventHandler green;

    public event EventHandler red;

    public event EventHandler yellow;

    public event EventHandler blue;

    public event EventHandler orange;

    public event EventHandler green_strum;

    public event EventHandler red_strum;

    public event EventHandler yellow_strum;

    public event EventHandler blue_strum;

    public event EventHandler orange_strum;

    public event EventHandler whammy_down;

    public event EventHandler whammy_up;
}
