using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Any Component (script extending MonoBehaviour) should implement this interface if it needs to run specific code
/// upon being destroyed and reinstantiated.
/// </summary>
public interface IRecyle{

	void Restart();
	void Shutdown();

}

/// <summary>
/// If you want a prefab to be recycled using an ObjectPool, all you need to do is add this component to it and
/// GameObjectUtil takes care of the rest.
/// </summary>
public class RecycleGameObject : MonoBehaviour {

	private List<IRecyle> recycleComponents;

	void Awake(){

		var components = GetComponents<MonoBehaviour> ();
		recycleComponents = new List<IRecyle> ();
		foreach (var component in components) {
			if(component is IRecyle){
				recycleComponents.Add (component as IRecyle);
			}
		}
	}


	public void Restart(){
		gameObject.SetActive (true);

		foreach (var component in recycleComponents) {
			component.Restart();
		}
	}

	public void Shutdown(){
		gameObject.SetActive (false);

		foreach (var component in recycleComponents) {
			component.Shutdown();
		}
	}

}
