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

	void Awake () {
		ServiceFactory.Instance.RegisterSingleton<MessageRouter>();
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter>();
		MessageRouter.AddHandler<NavigateMenuMessage>(OnNavigateMenu);
		MessageRouter.AddHandler<SceneChangeMessage>(OnSceneChange);
	}

	void Start() {
		MenuOptions[CurrentSelectionIndex].GetComponentInChildren<Image>().enabled = true;
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
			default:
				break;
		}
	}

	void Scroll(int dir) {
		MenuOptions[CurrentSelectionIndex].GetComponentInChildren<Image>().enabled = false;
		CurrentSelectionIndex = (CurrentSelectionIndex + dir + MenuOptions.Count) % MenuOptions.Count;
		MenuOptions[CurrentSelectionIndex].GetComponentInChildren<Image>().enabled = true;
	}

	void MakeCurrentSelection() {
		switch (MenuOptions[CurrentSelectionIndex].GetComponent<MainMenuItem>().ItemType) {
			case MenuItemType.LoadScene:
				MessageRouter.RaiseMessage(new SceneChangeMessage() {
					nextScene = MenuOptions[CurrentSelectionIndex].GetComponent<MainMenuItem>().SceneName
				});
				break;
			case MenuItemType.ExitGame:
				Application.Quit();
				break;
			default:
				break;
		}
	}

	void OnSceneChange(SceneChangeMessage m) {
		StartCoroutine(RemoveHandlers(m.nextScene));
	}

	IEnumerator RemoveHandlers(string nextScene) {
		yield return new WaitForEndOfFrame();
		MessageRouter.RemoveHandler<NavigateMenuMessage>(OnNavigateMenu);
		MessageRouter.RemoveHandler<SceneChangeMessage>(OnSceneChange);
		SceneManager.LoadScene(nextScene);
	}

}
