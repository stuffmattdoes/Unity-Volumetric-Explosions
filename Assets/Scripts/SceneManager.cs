using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour {

	public int explSelect;
	public GameObject[] explosions;
	VolumetricExplosion volExp;

	// Use this for initialization
	void Start () {

		volExp = explosions[explSelect].GetComponent<VolumetricExplosion> ();
	}
	
	// Update is called once per frame
	void Update () {

		// Spacebar trigger
		if (Input.GetKeyDown(KeyCode.Space)) {
			TriggerExplosion ();
		}

		if (Input.GetMouseButtonDown (0)) {
			ClickExplosion ();
		}
	}

	void TriggerExplosion() {
		volExp.detonate = true;
	}

	void ClickExplosion() {

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (ray, out hit, 100.0f)) {
			GameObject newExplosion = (GameObject)Instantiate (explosions[explSelect], hit.point, Quaternion.identity);
			newExplosion.GetComponent<VolumetricExplosion> ().detonate = true;

		}
	}
}
