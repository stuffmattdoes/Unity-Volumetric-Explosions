using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

	#region member
	public static ObjectPool instance { get; private set; }
	
	// Member class for a prefab entered into the object pool
	// "Serializable" is used to embed a class with sub properties in the inspector.
	[System.Serializable]
	public class ObjectPoolEntry {
		
		// The object to pre instantiate
		[SerializeField]
		public GameObject Prefab;
		
		// Quantity of object to pre-instantiate
		[SerializeField]
		public int Amount;
	}
	#endregion

	// Array of properties for our pooled objects
	public bool GrowIfUnavailable = true;
	public ObjectPoolEntry[] Entries;
	public string containerSuffix = "(Pool)";

	// The pooled objecst currently available
	// This list is composed of even more lists (one for each prefab entry)
	// which then contains the quantity of prefab entry to be pooled
//	[HideInInspector]s
	public List<GameObject> AvailablePool;

	// Array that will be used to create empty game objects to serve
	// as containers for each pooled object type. Purely for organization :D
	protected GameObject[] ContainerObject;

	void OnEnable() {
		instance = this;
	}

	void Start() {

		// Resize our container array equal to the number of our prefab entries in the inspector
		ContainerObject = new GameObject[Entries.Length];

		// Let's loop through our complete prefab entry array
		for (int i = 0; i < Entries.Length; i ++) {

			if (Entries[i] != null) {
				// Create a new game object container, and rename it based on the prefab's name
				ContainerObject[i] = new GameObject (containerSuffix + " " + Entries[i].Prefab.name);

				// Add each of our prefab entries to this here
				var objectPrefab = Entries[i];

				// Loop through each prefab entry by the specified amount for each
				for (int n = 0; n < objectPrefab.Amount; n++) {
					var newObj = Instantiate(objectPrefab.Prefab) as GameObject;
					newObj.name = objectPrefab.Prefab.name;
					PoolObject(newObj);
				}
			}
		}
	}

	// Used to create the prefabs to be pooled
	public void PoolObject(GameObject obj) {

		for (int i = 0; i < Entries.Length; i++) {

			// Keep iterating though our entries until our new prefab's name
			// matches that of our entry's existing prefab name
			// This ensures that we'll spawn the new prefab in its appropriate
			// container & that it gets assigned to the correct list & so on
//			if (Entries[i].Prefab.name != obj.name) {
			if (obj.name != Entries[i].Prefab.name) {
				continue;
			}

			// Deactivate it before anything happens; otherwise, chaos.
			obj.SetActive(false);
//			obj.transform.parent = ContainerObject[i].transform;
			obj.transform.SetParent(ContainerObject[i].transform, false);

			// Add it to our available pooled objects list
			AvailablePool.Add (obj);
			return;
		}
	}

	public GameObject GetPooledObject(GameObject pooledObject) {

		if (!pooledObject) {
			Debug.Log ("No pooled object");
		}

		// Loop through our pool of available objects
		for (int i = 0; i < AvailablePool.Count; i++) {

			// Finds the first prefab with the same name
			if (pooledObject.name != AvailablePool [i].name) {
				continue;
			}
		
			if (!AvailablePool [i].activeInHierarchy) {
				return AvailablePool [i];
			}
		}

		// Auto-resize our pool if we need to
		if (GrowIfUnavailable) {
			// Create a new object and add it to our pool
			GameObject newObj = Instantiate (pooledObject) as GameObject;
			newObj.name = pooledObject.name;
			PoolObject(newObj);
			return newObj;
		}

		return null;
	}
}