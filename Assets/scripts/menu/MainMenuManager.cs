using UnityEngine;
using System.Collections;
using Frictionless;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {

	MessageRouter MessageRouter;
	int CurrentSelectionIndex = 0;
	[SerializeField]
	private List<Text> MenuOptions;
	[SerializeField]
	private List<Canvas> HowToPlaySteps;
	[SerializeField]
	private Canvas MainCanvas;

	private bool inHowToPlay;
	private int howToPlayIndex = 0;

	void Awake () {
		ServiceFactory.Instance.RegisterSingleton<MessageRouter>();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<NavigateMenuMessage>(OnNavigateMenu);
		MessageRouter.AddHandler<SceneChangeMessage>(OnSceneChange);
	}

	void Start() {
		MenuOptions[CurrentSelectionIndex].GetComponentInChildren<Image>().enabled = true;
		foreach (Canvas c in HowToPlaySteps)
			c.enabled = false;
		MainCanvas.enabled = true;
	}

	void OnNavigateMenu(NavigateMenuMessage m) {
		switch (m.NavType) {
			case NavigationType.SCROLL_UP:
				Scroll(-1);
				break;
			case NavigationType.SCROLL_DOWN:
				Scroll(1);
				break;
			case NavigationType.SELECT:
				MakeCurrentSelection();
				break;
			case NavigationType.BACK:
				LoadPrevHowToPlay();
				break;
			default:
				break;
		}
	}

	void Scroll(int dir) {
		if (inHowToPlay)
			return;

		MenuOptions[CurrentSelectionIndex].GetComponentInChildren<Image>().enabled = false;
		CurrentSelectionIndex = (CurrentSelectionIndex + dir + MenuOptions.Count) % MenuOptions.Count;
		MenuOptions[CurrentSelectionIndex].GetComponentInChildren<Image>().enabled = true;
	}

	void MakeCurrentSelection() {
		if (inHowToPlay) {
			LoadNextHowToPlay();
		} else {
			switch (MenuOptions[CurrentSelectionIndex].GetComponent<MainMenuItem>().ItemType) {
				case MenuItemType.LoadScene:
					MessageRouter.RaiseMessage(new SceneChangeMessage() {
						nextScene = MenuOptions[CurrentSelectionIndex].GetComponent<MainMenuItem>().SceneName
					});
					break;
				case MenuItemType.ExitGame:
					Application.Quit();
					break;
				case MenuItemType.LoadCanvas:
					LoadHowToPlay();
					break;
				default:
					break;
			}
		}
		
	}

	void LoadNextHowToPlay() {
		if (!inHowToPlay)
			return;
		HowToPlaySteps[howToPlayIndex].enabled = false;
		if (howToPlayIndex+1 < HowToPlaySteps.Count) {
			howToPlayIndex++;
			HowToPlaySteps[howToPlayIndex].enabled = true;
		} else {
			inHowToPlay = false;
			MainCanvas.enabled = true;
		}
		
	}

	void LoadPrevHowToPlay() {
		if (!inHowToPlay)
			return;
		if (howToPlayIndex - 1 >= 0) {
			HowToPlaySteps[howToPlayIndex].enabled = false;
			howToPlayIndex--;
			HowToPlaySteps[howToPlayIndex].enabled = true;
		}
	}

	void LoadHowToPlay() {
		howToPlayIndex = 0;
		inHowToPlay = true;
		MainCanvas.enabled = false;
		HowToPlaySteps[howToPlayIndex].enabled = true;
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine(LoadNextScene(m.nextScene));
	}

	IEnumerator LoadNextScene(string nextScene) {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<NavigateMenuMessage>(OnNavigateMenu);
		MessageRouter.RemoveHandler<SceneChangeMessage>(OnSceneChange);
		SceneManager.LoadScene(nextScene);
	}

}
