using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WiimoteApi;
using Frictionless;

public abstract class WiimoteMessage {
	public int PlayerNumber { get; set; }
}

public class WiimoteConnectMessage : WiimoteMessage {}

public class WiimoteDisconnectMessage : WiimoteMessage {}

public class GuitarExtensionConnectMessage : WiimoteMessage {}

public class GuitarExtensionDisconnectMessage : WiimoteMessage {}


/// <summary>
/// Guitar/wiimote input manager. This component continuously runs and manages connection with wiimotes. It handles 
/// connecting and reconnecting with wiimotes, and provides access to the Wiimote and Guitar Hero extension input.
/// NOTE that this script is not a replacement for the computer's bluetooth driver. As such, all wiimotes must be 
/// first connected to the computer via the computer's bluetooth settings by the player before this script will have
/// access to them.
/// </summary>
public class GuitarConnectionManager : MonoBehaviour, IMultiSceneSingleton {

	[SerializeField]
	private float updateInterval = 1f;  // Number of seconds between checking on the status of connected wiimotes
	[SerializeField]
	private bool findWiimotesOnEnable = true;  // If true, automatially start looking for wiimotes at game start

	private bool isSearching = false;
	private Dictionary<int, int> readFailures;
	private Dictionary<int, bool> isGuitarConnected;
	private int maxTolerateReadFailures = 3;
	private List<int> playerRequestQueue; // Queue of player numbers registered as inputs awaiting a connected wiimote
	private MessageRouter MessageRouter;

	/// <summary>
	/// Gets the connected guitars as a dictionary of Wiimote objects, indexed by player number, which contain the 
	/// input state of the connected guitar inside the Guitar property. Wiimotes that do not have guitars connected 
	/// will still appear in this dictionary.
	/// </summary>
	/// <value>The connected wiimotes.</value>
	public Dictionary<int, Wiimote> wiimotes;

	private void Awake() {
		wiimotes = new Dictionary<int, Wiimote> ();
		isGuitarConnected = new Dictionary<int, bool> ();
		readFailures = new Dictionary<int, int> ();
		playerRequestQueue = new List<int> ();

		// Register this GameObject component as a singleton so that it can be referenced elsewhere
		// Perhaps later we will change where this singleton is registed to an external GameManager
		// class if we don't want to have to attach GuitarInputManager as a component.
		ServiceFactory.Instance.RegisterSingleton<GuitarConnectionManager> (this);
	}

	private void Start() {
		GuitarConnectionManager managerSingleton = ServiceFactory.Instance.Resolve<GuitarConnectionManager> (true);
		Debug.Log("got one");
		Debug.Log(managerSingleton);
		Debug.Log(managerSingleton.isGuitarConnected);
		//if (managerSingleton != null && managerSingleton != this) {
		if (GameObject.Find ("GuitarConnectionManager") != null && GameObject.Find ("GuitarConnectionManager") != this.gameObject) {
			Debug.Log("got here");
			Destroy (this.gameObject);
		}
		DontDestroyOnLoad(transform.gameObject);
		ServiceFactory.Instance.RegisterSingleton<GuitarConnectionManager> (this);
		MessageRouter = ServiceFactory.Instance.Resolve<MessageRouter> ();
		MessageRouter.AddHandler<RegisterGuitarInputMessage> (OnRegisterGuitarInputMessage);
		MessageRouter.AddHandler<UnregisterGuitarInputMessage> (OnUnregisterGuitarInputMessage);
	}

	private void Update() {
		foreach (KeyValuePair<int, Wiimote> wiimote in wiimotes) {
			// Read wiimote data
			int ret;
			do {
				ret = wiimote.Value.ReadWiimoteData();
			} while (ret > 0);
			if (ret == -1) {
				// Read failure.
				Debug.Log ("Read failure: " + wiimote.Key);
				readFailures [wiimote.Key]++;
				if (readFailures [wiimote.Key] > maxTolerateReadFailures) {
					// Assume wiimote has disconnected.
					DisconnectWiimote(wiimote.Key);
					continue;
				}
			} else {
				readFailures [wiimote.Key] = 0;
			}
			// Check state of extension:
			if (isGuitarConnected [wiimote.Key] && wiimote.Value.current_ext == ExtensionController.NONE) {
				isGuitarConnected [wiimote.Key] = false;
				Debug.Log ("guitar disconnected: " + wiimote.Key);
				MessageRouter.RaiseMessage (new GuitarExtensionDisconnectMessage () { PlayerNumber = wiimote.Key });
			} else if (!isGuitarConnected [wiimote.Key] && wiimote.Value.current_ext == ExtensionController.GUITAR) {
				isGuitarConnected [wiimote.Key] = true;
				Debug.Log ("guitar connected: " + wiimote.Key);
				MessageRouter.RaiseMessage (new GuitarExtensionConnectMessage () { PlayerNumber = wiimote.Key });
			}
		}
	}

	private void OnEnable() {
		if (findWiimotesOnEnable) {
			StartManagingWiimotes ();
		}
	}

	private void OnDisable() {
		
		StopManagingWiimotes ();

		// Disconnect all wiimotes
		while (WiimoteManager.Wiimotes.Count > 0) {
			int localIndex = IndexOfWiimote (WiimoteManager.Wiimotes [0]);
			if (localIndex > -1) {
				DisconnectWiimote (localIndex);
			} else {
				Debug.LogError ("Disconnecting wiimote unassociated with player number");
				WiimoteManager.Cleanup (WiimoteManager.Wiimotes [0]);
			}
		}
	}

	private void DisconnectWiimote(int playerNumber) {
		Debug.Assert (wiimotes.ContainsKey (playerNumber));
		Debug.Assert (isGuitarConnected.ContainsKey (playerNumber));
		Debug.Assert (readFailures.ContainsKey (playerNumber));
		wiimotes [playerNumber].SendPlayerLED (false, false, false, false);
		// TODO: Figure out a way to not have to do the thread-sleep thing. Sometimes causes crash.
		System.Threading.Thread.Sleep (50);
		WiimoteManager.Cleanup (wiimotes [playerNumber]);
		wiimotes.Remove (playerNumber);
		isGuitarConnected.Remove (playerNumber);
		readFailures.Remove (playerNumber);
		EnqueuePlayerNumber (playerNumber);
		Debug.Log ("wiimote disconnected: " + playerNumber);
		MessageRouter.RaiseMessage (new WiimoteDisconnectMessage () { PlayerNumber = playerNumber });
	}

	private void EnqueuePlayerNumber(int playerNumber) {
		Debug.Assert (!playerRequestQueue.Contains (playerNumber));
		Debug.Assert (!wiimotes.ContainsKey (playerNumber));
		Debug.Assert (!isGuitarConnected.ContainsKey (playerNumber));
		Debug.Assert (!readFailures.ContainsKey (playerNumber));
		playerRequestQueue.Add (playerNumber);
		playerRequestQueue.Sort ();
	}

	private void OnRegisterGuitarInputMessage(RegisterGuitarInputMessage e) {
		if (wiimotes.ContainsKey (e.PlayerNumber) || playerRequestQueue.Contains (e.PlayerNumber))
			return;
		Debug.Assert (!isGuitarConnected.ContainsKey (e.PlayerNumber));
		Debug.Assert (!readFailures.ContainsKey (e.PlayerNumber));
		EnqueuePlayerNumber (e.PlayerNumber);
	}

	private void OnUnregisterGuitarInputMessage(UnregisterGuitarInputMessage e) {
		if (wiimotes.ContainsKey (e.PlayerNumber)) {
			Debug.Assert (isGuitarConnected.ContainsKey (e.PlayerNumber));
			Debug.Assert (readFailures.ContainsKey (e.PlayerNumber));
			if (playerRequestQueue.Contains (e.PlayerNumber)) {
				playerRequestQueue.Remove (e.PlayerNumber);
			} else {
				DisconnectWiimote (e.PlayerNumber);
			}
		}
	}

	/// <summary>
	/// Starts a coroutine that periodically searches for new wiimotes and maintains connection status.
	/// </summary>
	public void StartManagingWiimotes() {
		if (!isSearching) {
			StartCoroutine ("SearchForWiimotes");
		}
	}

	/// <summary>
	/// Stops tracking wiimote connection status.
	/// </summary>
	public void StopManagingWiimotes() {
		isSearching = false;
	}

	/// <summary>
	/// Given a Wiimote object, get the Wiimote's player number. -1 if not being tracked.
	/// </summary>
	/// <returns>The player number of wiimote.</returns>
	/// <param name="wiimote">Wiimote.</param>
	public int IndexOfWiimote(Wiimote wiimote) {
		foreach (KeyValuePair<int, Wiimote> i in wiimotes) {
			if (i.Value.hidapi_handle == wiimote.hidapi_handle) {
				return i.Key;
			}
		}
		return -1;
	}

	/// <summary>
	/// Coroutine that periodically checks on the state of wiimotes
	/// </summary>
	private IEnumerator SearchForWiimotes() {
		isSearching = true;
		while (isSearching) {
			// Manage connection state of wiimotes
			if (playerRequestQueue.Count > 0) {
				WiimoteManager.FindWiimotes ();
				yield return new WaitForSeconds (0.1f); // Give wiimotes ample time to conenct
				for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++) {
					if (IndexOfWiimote (WiimoteManager.Wiimotes [i]) == -1) {
						// Wiimote is not being tracked. It must be new! Let's try to connect it!
						if (playerRequestQueue.Count > 0) {
							int playerNumber = playerRequestQueue [0];
							playerRequestQueue.RemoveAt (0);
							wiimotes.Add (playerNumber, WiimoteManager.Wiimotes [i]);
							isGuitarConnected.Add (playerNumber, false);
							readFailures.Add (playerNumber, 0);
							WiimoteManager.Wiimotes [i].SendPlayerLED (playerNumber == 0, playerNumber == 1, 
								playerNumber == 2, playerNumber == 3);
							WiimoteManager.Wiimotes [i].SendDataReportMode (InputDataType.REPORT_BUTTONS_ACCEL_EXT16);
							Debug.Log ("wiimote connected: " + playerNumber);
							MessageRouter.RaiseMessage (new WiimoteConnectMessage ());
						} else {
							// No room for a new wiimote. Reject.
							WiimoteManager.Cleanup (WiimoteManager.Wiimotes [i]);
						}
					}
				}
			}
			yield return new WaitForSeconds(updateInterval);
		}
		yield return null;
	}

	public IEnumerator HandleNewSceneLoaded() {
		return null;
	}
}
