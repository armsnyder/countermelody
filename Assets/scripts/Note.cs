using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour {
	public int position { get; set; } // Number of position units into the song the note occurs (120 = 16th note)
	public int instrumentID { get; set; } // Which musical track the note belongs to
	public int difficulty { get; set; } // Easy = 0, medium = 1, hard = 2
	public int duration { get; set; } // Number of position units the note lasts (120 = 16th note)
	public int velocity { get; set; } // Extraneous MIDI information we might use late on, but for now is always 127
	public int fretNumber { get; set; } // Number of fret (0 = green, 4 = orange)
	public InputButton fretColor
	{
		get
		{
			return fretNumber == 0 ? InputButton.GREEN : fretNumber == 1 ? 
				InputButton.RED : fretNumber == 2 ? InputButton.YELLOW : fretNumber == 3 ? 
				InputButton.BLUE : InputButton.ORANGE;
		}
	} // Color of fret as InputButton
	public bool isHOPO { get { return velocity < 127; } } // If true, strum not required to hit note


	private void Start() {
		Color color = fretNumber == 0 ? Color.green : 
			fretNumber == 1 ? Color.red :
			fretNumber == 2 ? Color.yellow : 
			fretNumber == 3 ? Color.green : 
			new Color(255, 123, 0, 1);

		GetComponent<Renderer>().material.color = color;
	}
}
