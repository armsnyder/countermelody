using UnityEngine;
using System.Collections;

public enum MenuItemType {
	LoadScene,
	ExitGame
}

public class MainMenuItem : MonoBehaviour {
	public MenuItemType ItemType;
	public string SceneName;
}
