using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VolumetricExplosion : MonoBehaviour {

	[Header("Main Explosion")]
	[Range(0.1f, 5.0f)] public float duration = 1;
	[Range (0.0f, 1.0f)] public float variation = 0.1f;
	[Range(0.0f, 1.0f)] public float riseSpeed;
	[Range(0.1f, 5.0f)] public float deformSpeed = 1;
	public AnimationCurve scale = AnimationCurve.EaseInOut(0, 0.2f, 1, 2);
	public AnimationCurve displacement = AnimationCurve.Linear(0, 0, 1, 0.5f);
	public AnimationCurve displacementOffsetY = AnimationCurve.Linear(0, 0, 1, 1);
	public AnimationCurve maxRange = AnimationCurve.Linear(0, 0.2f, 1, 1);
	public AnimationCurve clip = AnimationCurve.Linear(0.5f, 0.7f, 1, 0.5f);
	public float killTime;

	[Header("Sub FX")]
	public bool playAudio = false;
	public bool createLighting = false;
	public bool createSparks = false;
	public bool createFlares = false;
	public bool createSmoke = false;
	public bool createSmokeTrails = false;
	public bool createShockwave = false;
	public bool createHeatDistortion = false;
	public bool applyForce = false;
	public GameObject sparksPrefab;
	public GameObject flaresPrefab;
	public GameObject smokeTrailPrefab;
	public GameObject shockwavePrefab;
	public GameObject heatDistortionPrefab;

	[Header("Lighting")]
	public GameObject lightingPrefab;
	public AnimationCurve lightIntensity = AnimationCurve.EaseInOut(0, 1.0f, 1, 0);

	[Header("Debugging")]
	public bool debugPos = false;
	public bool loop = false;

	[HideInInspector] public bool detonate = true;
	private float newVariation;
	private float newExplosionDuration;
	private Vector3 initScale;
	private float startTime;
	private float timeFromBegin;
	private float vertPos;
	private float r;
	private float g;
	private float b;
	private float correction;
	private float scaleFactor;
	private float beginRange;
	private float endRange;
	private float dispVal;
	private float dispOffsetY;
	private float clipVal;
	private Vector3 thisPos;
	private Vector3 startPos;
	private Material thisMat;
	private Light _lighting;
	private AudioSource _audioClip;
	private ParticleSystem _sparks;
	private ParticleSystem _flares;
	private GameObject _smokeTrail;
	public List<GameObject> _smokeTrailsGroup;
	private ParticleSystem _shockwave;
	private ParticleSystem _heatDistortion;

	void Start () {
		thisMat = GetComponent<Renderer> ().material;


		// ---------------
		// Initiate our FX
		// ---------------

		if (createLighting) {
			GameObject _lightingPrefab = (GameObject)Instantiate (lightingPrefab, transform.position, Quaternion.identity);
			_lightingPrefab.transform.parent = gameObject.transform;
			_lighting = _lightingPrefab.GetComponent<Light> ();
		}

		// Audio
		if (playAudio) {
			_audioClip = GetComponent<AudioSource> ();
		}

		// SPparks
		if (createSparks && sparksPrefab) {
			GameObject _sparksPrefab = (GameObject)Instantiate (sparksPrefab, transform.position, Quaternion.identity);
			_sparksPrefab.transform.parent = gameObject.transform;
			_sparks = _sparksPrefab.GetComponent<ParticleSystem> ();
		}

		// Flares
		if (createFlares && flaresPrefab) {
			GameObject _flaresPrefab = (GameObject)Instantiate (flaresPrefab,
				new Vector3(
					transform.position.x,
					transform.position.y + 0.35f,
					transform.position.z
				), Quaternion.identity);
			_flaresPrefab.transform.parent = gameObject.transform;
			_flares = _flaresPrefab.GetComponent<ParticleSystem> ();
		}

		// Smoke trails
		if (createSmokeTrails && smokeTrailPrefab) {
			GameObject smokeTrailsGroup = new GameObject ("Smoke Trails");
			smokeTrailsGroup.transform.parent = gameObject.transform;

			for (int i = 0; i < 6; i++) {
				GameObject _smokeTrailPrefab = (GameObject)Instantiate (smokeTrailPrefab, transform.position, Quaternion.identity);
				_smokeTrailPrefab.transform.parent = gameObject.transform;
				_smokeTrail = _smokeTrailPrefab.gameObject;
				_smokeTrail.transform.parent = smokeTrailsGroup.transform;
				_smokeTrailsGroup.Add (_smokeTrail);
			}
		}

		// Shockwave
		if (createShockwave && shockwavePrefab) {
			GameObject _shockwavePrefab = (GameObject)Instantiate (shockwavePrefab, transform.position, Quaternion.identity);
			_shockwavePrefab.transform.parent = gameObject.transform;
			_shockwave = _shockwavePrefab.GetComponent<ParticleSystem> ();
		}

		// Heat distortion
		if (createHeatDistortion && heatDistortionPrefab) {
			GameObject _heatDistortionPrefab = (GameObject)Instantiate (heatDistortionPrefab, transform.position, Quaternion.identity);
			_heatDistortionPrefab.transform.parent = gameObject.transform;
			_heatDistortion = _heatDistortionPrefab.GetComponent<ParticleSystem> ();
		}

		// Scaling & positioning
		initScale = transform.localScale;

		startPos = new Vector3 (
			transform.position.x,
			transform.position.y,
			transform.position.z
		);
	}

	void Update () {

		if (!detonate && timeFromBegin < newExplosionDuration || loop) {
			Detonation ();
		}

		if (detonate) {
			if (debugPos) {
				transform.position = startPos;
			}
			Reset ();
		}
	}

	void Detonation () {

		// Slightly translate explosion vertically, because heat rises dummy
		AdjustPosition();

		// Explosion FX:

		// Per-vertex displacement calculations
		// (to be sent to the shader to actually be applied)
		AdjustDisplacement();

		// Offset displacement map for some cool FX
		AdjustDisplacementMap();

		// Adjust the size of our explosion
		AdjustScale();

		// Fade out (clip) the explosion
		AdjustClip();

		// Lighting always sets the mood, man.
		if (createLighting && _lighting) {
			AdjustLighting ();
		}

	}

	void Reset () {

		// -------------
		// Randomize it!
		// -------------

		// Base random variation
		newVariation = Random.Range (-variation, variation);
		newExplosionDuration = duration + (duration * newVariation);

		// Random rotation
		Vector3 euler = transform.eulerAngles;

		// Y-axis Rotation
		euler.y = Random.Range (0.0f, 360.0f);
		transform.eulerAngles = euler;


		// Don't forget about the audio!
		if (playAudio && _audioClip) {
			_audioClip.pitch = 1 + (1 * -newVariation);
			_audioClip.Play ();
		}

		// Sparks
		if (createSparks && _sparks) {
			_sparks.Play();
		}

		// Flares
		if (createFlares && _flares) {
			_flares.Play();
		}

		// Smoke Trails
		if (createSmokeTrails && _smokeTrailsGroup.Count > 0) {
			LaunchSmokeTrails ();
		}

		// Shockwave
		if (createShockwave && _shockwave) {
			Debug.Log ("Shockwave");
			_shockwave.Play ();
		}

		// Heat distortion
		if (createHeatDistortion && _heatDistortion) {
			_heatDistortion.Play ();
		}

		startTime = Time.time;
		timeFromBegin = Time.time - startTime;
		detonate = false;
	}

	void AdjustPosition() {
		thisPos = new Vector3 (
			transform.position.x,
			transform.position.y,
			transform.position.z
		);

		transform.position = new Vector3 (
			thisPos.x,
			thisPos.y + (riseSpeed * Time.deltaTime),
			thisPos.z
		);
	}

	void AdjustDisplacement() {
		timeFromBegin = Time.time - startTime;
		vertPos = (newVariation + timeFromBegin) / deformSpeed;
		r = Mathf.Sin((vertPos) * (2 * Mathf.PI)) * 0.5f + 0.25f;
		g = Mathf.Sin((vertPos + 0.33333333f) * 2 * Mathf.PI) * 0.5f + 0.25f;
		b = Mathf.Sin((vertPos + 0.66666667f) * 2 * Mathf.PI) * 0.5f + 0.25f;

		// Additionally - do some overall extrusion
		dispVal = displacement.Evaluate(timeFromBegin / newExplosionDuration);
		thisMat.SetFloat("_Displacement", dispVal);
	}

	void AdjustDisplacementMap() {
		dispOffsetY = displacementOffsetY.Evaluate(timeFromBegin / newExplosionDuration);
		thisMat.SetTextureOffset("_DispTex", new Vector2(0, dispOffsetY));

		// Ensure that their sum = 1)
		correction = 1 / (r + g + b);
		r *= correction;
		g *= correction;
		b *= correction;

		thisMat.SetVector("_ChannelFactor", new Vector4(r,g,b,0));
	}

	void AdjustScale() {
		scaleFactor = scale.Evaluate(timeFromBegin / newExplosionDuration);
		scaleFactor = scaleFactor + (scaleFactor * newVariation);
		transform.localScale = initScale * scaleFactor;
	}

	void AdjustClip() {
		endRange = maxRange.Evaluate(timeFromBegin / newExplosionDuration);
		endRange = endRange + (endRange * newVariation);

		if (beginRange >= 1.0f) {
			beginRange = 1.0f;
		}

		if (endRange >= 1.0f) {
			endRange = 1.0f;
		}

		thisMat.SetVector("_Range", new Vector4(beginRange, endRange, 0, 0));

		clipVal = clip.Evaluate(timeFromBegin / newExplosionDuration);
		thisMat.SetFloat("_ClipRange", clipVal);
	}

	void AdjustLighting() {
		_lighting.intensity = lightIntensity.Evaluate(timeFromBegin / newExplosionDuration);
	}

	void LaunchSmokeTrails() {
		Debug.Log ("Launch smoke trails!");
		Vector3 randomDir;
		float mult = 10;

		for (int i = 0; i < _smokeTrailsGroup.Count; i++) {
			randomDir = new Vector3 (
				mult * Random.Range(-10.0f, 10.0f),
				mult * Random.Range(-1.0f, 10.0f),
				mult * Random.Range(-10.0f, 10.0f)
			);

			_smokeTrailsGroup [i].GetComponent<Rigidbody> ().AddForce (randomDir);
			Debug.Log (randomDir);
		}
	}

}