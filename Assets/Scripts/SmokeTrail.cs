﻿using UnityEngine;
using System.Collections;

public class SmokeTrail : MonoBehaviour {

	// Variables
	[Range (0.1f, 2.0f)] public float duration = 1.0f;
	[Range (0.1f, 1.0f)] public float variation = 1.0f;
	public AnimationCurve particleScale = AnimationCurve.EaseInOut (0, 1.0f, 1.0f, 0);
	public AnimationCurve particleFade = AnimationCurve.Linear (0, 1.0f, 1.0f, 0);
	public LayerMask collideWith;

	private float newDuration;
	private float newVariation;
	private float startTime;
	private float timeFromStart;
	private float scaleFactor;
	private ParticleSystem particles;
	private Color _particleColor;

	void OnEnable() {
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
			particleFade.Evaluate (timeFromStart / newDuration)
		);
	}
		
}