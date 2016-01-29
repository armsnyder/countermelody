using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WiimoteApi;

/// <summary>
/// Guitar input manager. This component continuously runs and manages connection with wiimotes. It handles connecting 
/// and reconnecting with wiimotes, and provides access to the Guitar Hero extension input.
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
	private List<GuitarData> _guitars;  // TODO: Provide access to Wiimote objects (for rumble, D-pad, etc)
	private List<bool> isWiimoteConnected;

	/// <summary>
	/// Gets the connected guitars as a list of GuitarData objects, which contain the input state of the connected 
	/// guitar. If a wiimote is connected but does not have the guitar extension attached, it will not appear in this
	/// list.
	/// </summary>
	/// <value>The guitars.</value>
	public ReadOnlyCollection<GuitarData> guitars {
		get {
			return _guitars.AsReadOnly ();
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
						_guitars.Add (null);
						isWiimoteConnected.Add (false);
					}
					StartManagingWiimotes ();
				} else {
					// If the value decreases numRequiredPlayers, decrease the size of guitars, and disconnect any 
					// connected wiimotes that exceed the requirement.
					for (int i = _numRequiredPlayers - 1; i >= value; i--) {
						if (WiimoteManager.Wiimotes.Count > i && WiimoteManager.Wiimotes [i] != null) {
							WiimoteManager.Wiimotes [i].SendPlayerLED (false, false, false, false);
							WiimoteManager.Cleanup (WiimoteManager.Wiimotes [i]);
							Debug.Log ("wiimote disconnected");
							if (OnWiimoteDisconnect != null) {
								OnWiimoteDisconnect (i);
							}
						}
						_guitars.RemoveAt (_guitars.Count - 1);
						isWiimoteConnected.RemoveAt (isWiimoteConnected.Count - 1);
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
		get {
			for (int i = 0; i < _numRequiredPlayers; i++) {
				if (_guitars [i] == null || !isWiimoteConnected [i]) {
					return false;
				}
			}
				return true;
		}
	}

	private void Awake() {
		_guitars = new List<GuitarData> (_numRequiredPlayers);
		isWiimoteConnected = new List<bool> (_numRequiredPlayers);
		for (int i = 0; i < _numRequiredPlayers; i++) {
			_guitars.Add (null);
			isWiimoteConnected.Add (false);
		}
	}

	private void Update() {
		// TODO: Detect extension in Update loop (here) rather than Search loop (below)
		for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++) {
			if (WiimoteManager.Wiimotes [i] != null) {
				int ret;
				do {
					ret = WiimoteManager.Wiimotes [i].ReadWiimoteData();
				} while (ret > 0);
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

		// Clear all guitar data
		for (int i = 0; i < _guitars.Count; i++) {
			if (_guitars [i] != null) {
				_guitars[i] = null;
				Debug.Log ("extension disconnected");
				if (OnGuitarExtensionDisconnect != null) {
					OnGuitarExtensionDisconnect (i);
				}
			}
		}

		// Disconnect all wiimotes
		for (int i = 0; i < WiimoteManager.Wiimotes.Count; i++) {
			if (WiimoteManager.Wiimotes [i] != null) {
				WiimoteManager.Wiimotes [i].SendPlayerLED (false, false, false, false);
				// TODO: Figure out a way to not have to do the thread-sleep thing. Sometimes causes crash.
				System.Threading.Thread.Sleep (50);
				WiimoteManager.Cleanup (WiimoteManager.Wiimotes [i]);
				Debug.Log ("wiimote disconnected: " + i);
				if (OnWiimoteDisconnect != null) {
					OnWiimoteDisconnect (i);
				}
			}
		}
		for (int i = 0; i < isWiimoteConnected.Count; i++) {
			isWiimoteConnected [i] = false;
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
		StopCoroutine ("SearchForWiimotes");
		isSearching = false;
	}

	/// <summary>
	/// Coroutine that periodically checks on the state of wiimotes
	/// </summary>
	private IEnumerator SearchForWiimotes() {
		// TODO: Check for concurrency problems, probably lock a few things
		// TODO: When a wiimote is disconnected, its index is shifted in WiimoteManager. Make sure this is reflected.
		isSearching = true;
		while (isSearching) {
			
			// If not enough wiimotes are connected, attempt to connect additional wiimotes
			if (WiimoteManager.Wiimotes.Count < _numRequiredPlayers) {
				WiimoteManager.FindWiimotes ();  // Not enough wiimotes connected - find more
			}

			// Manage connection state of wiimotes
			for (int i = 0; i < isWiimoteConnected.Count; i++) {
				if (i < WiimoteManager.Wiimotes.Count && WiimoteManager.Wiimotes [i] != null && 
					!isWiimoteConnected [i]) {
					// Connect newly discovered wiimote
					isWiimoteConnected [i] = true;
					WiimoteManager.Wiimotes [i].SendPlayerLED (i == 0, i == 1, i == 2, i == 3);
					// Allow extensions to send data
					WiimoteManager.Wiimotes [i].SendDataReportMode (InputDataType.REPORT_BUTTONS_EXT8);
					Debug.Log ("wiimote connected: " + i);
					if (OnWiimoteConnect != null) {
						OnWiimoteConnect (i);
					}
				} else if ((i >= WiimoteManager.Wiimotes.Count || WiimoteManager.Wiimotes [i] == null) && 
					isWiimoteConnected [i]) {
					// Disconnect wiimote if it cannot be reached
					// TODO: Problem: this doesn't fire on a spontaneous disconnect. How should we detect it?
					isWiimoteConnected [i] = false;
					Debug.Log ("wiimote disconnected");
					if (OnWiimoteDisconnect != null) {
						OnWiimoteDisconnect (i);
					}
				}
			}

			// Manage guitar extension state of wiimotes
			for (int i = 0; i < _guitars.Count; i++) {
				if (WiimoteManager.Wiimotes.Count > i && WiimoteManager.Wiimotes [i] != null &&
				    WiimoteManager.Wiimotes [i].current_ext == ExtensionController.GUITAR && _guitars [i] == null) {
					_guitars [i] = WiimoteManager.Wiimotes [i].Guitar;
					Debug.Log ("extension connected: " + i);
					if (OnGuitarExtensionConnect != null) {
						OnGuitarExtensionConnect (i);
					}
				} else if ((i >= WiimoteManager.Wiimotes.Count || WiimoteManager.Wiimotes [i] == null ||
				           WiimoteManager.Wiimotes [i].current_ext != ExtensionController.GUITAR) &&
				           _guitars [i] != null) {
					_guitars [i] = null;
					Debug.Log ("extension disconnected: " + i);
					if (OnGuitarExtensionDisconnect != null) {
						OnGuitarExtensionDisconnect (i);
					}
				}
			}

			yield return new WaitForSeconds(updateInterval);
		}
		yield return null;
	}
}
