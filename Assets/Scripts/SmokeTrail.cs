using UnityEngine;
using System.Collections;

public class SmokeTrail : MonoBehaviour {

	// Variables
	[Range (0.1f, 2.0f)] public float duration = 1.0f;
	[Range (0.1f, 1.0f)] public float variation = 1.0f;
	public AnimationCurve particleScale = AnimationCurve.EaseInOut (0, 1.0f, 1.0f, 0);
	public AnimationCurve particleFade = AnimationCurve.Linear (0, 1.0f, 1.0f, 0);

	private float newDuration;
	private float newVariation;
	private float startTime;
	private float timeFromStart;
	private float scaleFactor;
	private ParticleSystem particles;
	private Color _particleColor;
	private Vector3 initScale;

	// Use this for initialization
	void Start () {
		initScale = transform.localScale;
		Debug.Log (transform.localScale);
	}

	void OnEnable() {
//		transform.localScale = initScale;
		particles = gameObject.GetComponent<ParticleSystem> ();
		Reset ();
	}

	// Update is called once per frame
	void Update () {
		if (timeFromStart < newDuration) {
			StartSmoking ();
		} else {
			gameObject.SetActive (false);
		}

	}

	void Reset() {
		startTime = Time.time;

		// Randomized variation
		newVariation = Random.Range (
			-variation,
			variation
		);

		newDuration = duration + (duration * newVariation);
		StartSmoking ();
	}


	void StartSmoking() {
		timeFromStart = Time.time - startTime;

		// Fade out our particles over time
		particles.startColor = new Color (
			particles.startColor.r,
			particles.startColor.g,
			particles.startColor.b,
			particleFade.Evaluate (timeFromStart / (newDuration * 0.75f))
		);

		// Decrease scale of particle system over time
		scaleFactor = particleScale.Evaluate (timeFromStart / newDuration);
//		transform.localScale = initScale * scaleFactor;

	}
}