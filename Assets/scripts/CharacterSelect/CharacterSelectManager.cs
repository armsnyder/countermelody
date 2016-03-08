using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frictionless;
using UnityEngine.SceneManagement;

public class ChangeSelectedCharacterMessage {
	public int playerNumber { get; set; }
	public int index { get; set; }
}

public class AssignCharacterToFretMessage {
	public int playerNumber { get; set; }
	public InputButton fret { get; set; }
	public int index { get; set; }
	public GameObject prefab { get; set;}
}

public class SceneChangeMessage {
	public string currentScene { get; set; }
	public string nextScene { get; set; }
}

/// <summary>
/// Character select manager. This is the controller for all character select logic. If you're looking for view code,
/// check CharacterSelectUI.cs
/// </summary>
public class CharacterSelectManager : MonoBehaviour, IMultiSceneSingleton {

	public GameObject[] unitPrefabs;

	private MessageRouter messageRouter;
	private int[] selected;

	public GameObject[,] chosen;

	private bool ignore;

	void Awake() {
		DontDestroyOnLoad(transform.gameObject); // because we need to access public chosen array in next scene
		ServiceFactory.Instance.RegisterSingleton<MessageRouter> ();
		ServiceFactory.Instance.RegisterSingleton<CharacterSelectManager> (this);
		selected = new int[2];
		chosen = new GameObject[selected.Length, 5];
	}

	void Start () {
		ignore = false;
		messageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		messageRouter.AddHandler<ButtonDownMessage> (OnButtonDown);
		messageRouter.AddHandler<SceneChangeMessage> (OnSceneChange);
		/*if (GameObject.Find ("CharacterSelectManager") != null && GameObject.Find ("CharacterSelectManager") != this.gameObject) {
			Debug.Log("got here");
			Destroy (this.gameObject);
			Destroy(this);
		}*/
	}

	void OnSceneChange(SceneChangeMessage m) {
		//ignore = true;
		if(ignore) {
			return;
		}
		StartCoroutine(RemoveHandlers());
	}

	IEnumerator RemoveHandlers() {
		yield return new WaitForEndOfFrame();
		messageRouter.RemoveHandler<ButtonDownMessage> (OnButtonDown);
		messageRouter.RemoveHandler<SceneChangeMessage> (OnSceneChange);
	}

	void OnButtonDown(ButtonDownMessage m) {
		if(ignore) {
			return;
		}
		switch (m.Button) {
		case InputButton.STRUM_DOWN:
		case InputButton.DOWN:
			Scroll (m.PlayerNumber, 1);
			break;
		case InputButton.STRUM_UP:
		case InputButton.UP:
			Scroll (m.PlayerNumber, -1);
			break;
		case InputButton.GREEN:
		case InputButton.RED:
		case InputButton.YELLOW:
		case InputButton.BLUE:
		case InputButton.ORANGE:
			Choose (m.PlayerNumber, m.Button);
			break;
		case InputButton.PLUS:
		case InputButton.TILT: // AKA Enter key on keyboard
			StartGame ();
			break;
		}
	}

	void Scroll(int playerNumber, int offset) {
		int prev = selected [playerNumber];
		selected [playerNumber] += offset;
		if (selected [playerNumber] < 0) {
			selected [playerNumber] = 0;
		}
		if (selected [playerNumber] >= unitPrefabs.Length) {
			selected [playerNumber] = unitPrefabs.Length - 1;
		}
		if (prev != selected [playerNumber]) {
			messageRouter.RaiseMessage (new ChangeSelectedCharacterMessage () {
				playerNumber = playerNumber,
				index = selected [playerNumber]
			});
		}
	}

	void Choose(int playerNumber, InputButton fret) {
		chosen [playerNumber, (int)fret] = unitPrefabs [selected [playerNumber]];
		messageRouter.RaiseMessage (new AssignCharacterToFretMessage () {
			playerNumber = playerNumber,
			fret = fret,
			index = selected [playerNumber],
			prefab = unitPrefabs [selected [playerNumber]]
		});
	}

	bool StartGame() {
		if(ignore) {
			return true;
		}
		// Make sure all units have been assigned:
		for (int i = 0; i < chosen.GetLength(0); i++) {
			for (int j = 0; j < chosen.GetLength(1); j++) {
				if (chosen [i, j] == null) {
					return false;
				}
			}
		}
		// Enter next scene:
		string nextScene = "Movement";
		messageRouter.RaiseMessage (new SceneChangeMessage () {
			currentScene = SceneManager.GetActiveScene ().name,
			nextScene = nextScene
		});
		StartCoroutine (RemoveHandlers ());
		StartCoroutine (LoadSceneAfterFrame (nextScene));
		return true;
	}

	IEnumerator LoadSceneAfterFrame(string nextScene) {
		yield return null;
		yield return null;
		SceneManager.LoadScene (nextScene);
		yield return null;
		yield return null;
		Destroy(this.gameObject);
		Destroy(this);
		ignore = true;
	}

	public IEnumerator HandleNewSceneLoaded() {
		return null;
	}
}
