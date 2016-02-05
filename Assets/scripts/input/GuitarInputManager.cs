using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WiimoteApi;

/// <summary>
/// Guitar/wiimote input manager. This component continuously runs and manages connection with wiimotes. It handles 
/// connecting and reconnecting with wiimotes, and provides access to the Wiimote and Guitar Hero extension input.
/// NOTE that this script is not a replacement for the computer's bluetooth driver. As such, all wiimotes must be 
/// first connected to the computer via the computer's bluetooth settings by the player before this script will have
/// access to them.
/// </summary>
public class GuitarInputManager : MonoBehaviour {

	[SerializeField]
	private int _numRequiredPlayers = 0;  // After connected wiimotes reaches this number, no new connections are made
	[SerializeField]
	private float updateInterval = 1f;  // Number of seconds between checking on the status of connected wiimotes
	[SerializeField]
	private bool findWiimotesOnEnable = true;  // If true, automatially start looking for wiimotes at game start

	public delegate void WiimoteConnect(int player);
	/// <summary>
	/// Occurs when a wiimote connects.
	/// </summary>
	public static event WiimoteConnect OnWiimoteConnect;

	public delegate void WiimoteDisconnect(int player);
	/// <summary>
	/// Occurs when a wiimote disconnects.
	/// </summary>
	public static event WiimoteDisconnect OnWiimoteDisconnect;

	public delegate void GuitarExtensionConnect(int player);
	/// <summary>
	/// Occurs when a guitar extension is connected to a wiimote.
	/// </summary>
	public static event GuitarExtensionConnect OnGuitarExtensionConnect;

	public delegate void GuitarExtensionDisconnect(int player);
	/// <summary>
	/// Occurs when a guitar extension is disconnected from a wiimote.
	/// </summary>
	public static event GuitarExtensionDisconnect OnGuitarExtensionDisconnect;

	private bool isSearching = false;
	private List<Wiimote> _wiimotes;
	private List<bool> isGuitarConnected;
	private List<int> readFailures;
	private int maxTolerateReadFailures = 3;

	/// <summary>
	/// Gets the connected guitars as a list of Wiimote objects, which contain the input state of the connected 
	/// guitar inside the Guitar property. The index of the wiimote is that wiimote's player number. Wiimotes that do 
	/// not have guitars connected will still appear in this list.
	/// </summary>
	/// <value>The connected wiimotes.</value>
	public ReadOnlyCollection<Wiimote> wiimotes {
		get {
			return _wiimotes.AsReadOnly ();
		}
	}

	/// <summary>
	/// Gets or sets the number required players. As long as the number of connected players is less than the number 
	/// of required players set here, the script will continuously seek out new wiimotes to connect with. If the number 
	/// of required players is decremented, the last connected wiimote will be disconnected.
	/// </summary>
	/// <value>The number required players.</value>
	public int numRequiredPlayers {
		get {
			return _numRequiredPlayers;
		} set {
			if (value >= 0 && value != _numRequiredPlayers) {  // validates new value
				if (value > _numRequiredPlayers) {
					// If the value increases numRequiredPlayers, increase the size of guitars and begin looking for 
					// new wiimotes.
					for (int i = 0; i < value - _numRequiredPlayers; i++) {
						_wiimotes.Add (null);
						isGuitarConnected.Add (false);
						readFailures.Add (0);
					}
					StartManagingWiimotes ();
				} else {
					// If the value decreases numRequiredPlayers, decrease the size of guitars, and disconnect any 
					// connected wiimotes that exceed the requirement.
					for (int i = _numRequiredPlayers - 1; i >= value; i--) {
						if (_wiimotes [i] != null) {
							_wiimotes [i].SendPlayerLED (false, false, false, false);
							WiimoteManager.Cleanup (_wiimotes [i]);
							Debug.Log ("wiimote disconnected");
							if (OnWiimoteDisconnect != null) {
								OnWiimoteDisconnect (i);
							}
						}
						Debug.Assert (_wiimotes.Count == isGuitarConnected.Count);
						Debug.Assert (readFailures.Count == isGuitarConnected.Count);
						_wiimotes.RemoveAt (_wiimotes.Count - 1);
						isGuitarConnected.RemoveAt (isGuitarConnected.Count - 1);
						readFailures.RemoveAt (readFailures.Count - 1);
					}
				}
				_numRequiredPlayers = value;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether all required Guitar controllers are connected.
	/// </summary>
	/// <value><c>true</c> if ready; otherwise, <c>false</c>.</value>
	public bool ready {
		// TODO: Precalculate ready state
		get {
			for (int i = 0; i < _numRequiredPlayers; i++) {
				if (_wiimotes [i] == null || !isGuitarConnected [i]) {
					return false;
				}
			}
				return true;
		}
	}

	private void Awake() {
		_wiimotes = new List<Wiimote> (_numRequiredPlayers);
		isGuitarConnected = new List<bool> (_numRequiredPlayers);
		readFailures = new List<int> (_numRequiredPlayers);
		for (int i = 0; i < _numRequiredPlayers; i++) {
			_wiimotes.Add (null);
			isGuitarConnected.Add (false);
			readFailures.Add (0);
		}
	}

	private void Update() {
		for (int i = 0; i < _wiimotes.Count; i++) {
			if (_wiimotes [i] != null) {
				// Read wiimote data
				int ret;
				do {
					ret = _wiimotes [i].ReadWiimoteData();
				} while (ret > 0);
				if (ret == -1) {
					// Read failure.
					Debug.Log ("Read failure: " + i);
					readFailures [i]++;
					if (readFailures [i] > maxTolerateReadFailures) {
						// Assume wiimote has disconnected.
						_wiimotes [i] = null;
						isGuitarConnected [i] = false;
						WiimoteManager.Cleanup (_wiimotes [i]);
						Debug.Log ("wiimote disconnected: " + i);
						if (OnWiimoteDisconnect != null) {
							OnWiimoteDisconnect (i);
						}
						i--;
						continue;
					}
				} else {
					readFailures [i] = 0;
				}
				// Check state of extension:
				if (isGuitarConnected [i] && _wiimotes [i].current_ext == ExtensionController.NONE) {
					isGuitarConnected [i] = false;
					Debug.Log ("guitar disconnected: " + i);
					if (OnGuitarExtensionDisconnect != null) {
						OnGuitarExtensionDisconnect (i);
					}
				} else if (!isGuitarConnected [i] && _wiimotes [i].current_ext == ExtensionController.GUITAR) {
					isGuitarConnected [i] = true;
					Debug.Log ("guitar connected: " + i);
					if (OnGuitarExtensionConnect != null) {
						OnGuitarExtensionConnect (i);
					}
				}
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
				_wiimotes [localIndex] = null;
				isGuitarConnected [localIndex] = false;
				readFailures [localIndex] = 0;
			}
			WiimoteManager.Wiimotes [0].SendPlayerLED (false, false, false, false);
			// TODO: Figure out a way to not have to do the thread-sleep thing. Sometimes causes crash.
			System.Threading.Thread.Sleep (50);
			WiimoteManager.Cleanup (WiimoteManager.Wiimotes [0]);
			Debug.Log ("wiimote disconnected: " + localIndex);
			if (OnWiimoteDisconnect != null) {
				OnWiimoteDisconnect (localIndex);
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
		for (int i = 0; i < _wiimotes.Count; i++) {
			if (_wiimotes [i] != null && _wiimotes [i].hidapi_handle == wiimote.hidapi_handle) {
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Coroutine that periodically checks on the state of wiimotes
	/// </summary>
	private IEnumerator SearchForWiimotes() {
		// TODO: Check for concurrency problems, probably lock a few things
		isSearching = true;
		while (isSearching) {

			// Manage connection state of wiimotes
			for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++) {
				if (IndexOfWiimote (WiimoteManager.Wiimotes [i]) == -1) {
					// Wiimote is not being tracked. It must be new! Let's try to connect it!
					bool connected = false;
					for (int j = 0; j < _wiimotes.Count; j++) {
						if (_wiimotes [j] == null) {
							// Finalize connection
							connected = true;
							_wiimotes [j] = WiimoteManager.Wiimotes [i];
							WiimoteManager.Wiimotes [i].SendPlayerLED (j == 0, j == 1, j == 2, j == 3);
							WiimoteManager.Wiimotes [i].SendDataReportMode (InputDataType.REPORT_BUTTONS_EXT8);
							Debug.Log ("wiimote connected: " + j);
							if (OnWiimoteConnect != null) {
								OnWiimoteConnect (j);
							}
							break;
						}
					}
					if (!connected) {
						// No room for a new wiimote. Reject.
						WiimoteManager.Cleanup(WiimoteManager.Wiimotes[i]);
					}
				}
			}

			// If not enough wiimotes are connected, attempt to connect additional wiimotes
			if (WiimoteManager.Wiimotes.Count < _numRequiredPlayers) {
				WiimoteManager.FindWiimotes ();  // Not enough wiimotes connected - find more
			}

			yield return new WaitForSeconds(updateInterval);
		}
		yield return null;
	}
}
