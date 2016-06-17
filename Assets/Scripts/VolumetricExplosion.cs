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

	[Header("Audio")]
	public bool playAudio = false;

	[Header("Explosive Force")]
	public bool applyForce = false;
	[Range(0.1f, 10.0f)] public float radius = 5.0f;
	[Range(1, 10)] public int power = 10;

	[Header("Flares")]
	public bool createFlares = false;
	public GameObject flaresPrefab;

	[Header("Heat Distortion")]
	public bool createHeatDistortion = false;
	public GameObject heatDistortionPrefab;

	[Header("Lighting")]
	public bool createLighting = false;
	public GameObject lightingPrefab;
	public AnimationCurve lightIntensity = AnimationCurve.EaseInOut(0, 1.0f, 1, 0);

	[Header("Shockwave")]
	public bool createShockwave = false;
	public GameObject shockwavePrefab;

//	[Header("Smoke")]
//	public bool createSmoke = false;

	[Header("Smoke Trails")]
	public bool createSmokeTrails = false;
	public GameObject smokeTrailPrefab;
	public int minSmokeTrails;
	public int maxSmokeTrails;
	[Range(0, 10)] public int explosionBias;

	[Header("Sparks")]
	public bool createSparks = false;
	public GameObject sparksPrefab;

	[Header("Sub Explosions")]
	public bool createSubExplosinos = false;
	public GameObject subExplosionPrefab;
	[Range(1, 6)] public float subExplosions = 2;
	[Range(0.1f, 1)] public float subExplosionVariation = 0.2f;

	[Header("Debugging")]
	public bool debugPos = false;
	public bool loop = false;

	[HideInInspector] public bool detonate = true;
	private float newVariation;
	private float newExplosionDuration;
	private Vector3 initScale;
	private float startTime;
	private float timeFromStart;
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
	[HideInInspector] public List<GameObject> _smokeTrailsGroup;
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

		// Sparks
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

			for (int i = 0; i < maxSmokeTrails; i++) {
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

	void OnEnable() {
		if (killTime > 0) {
			StartCoroutine (KillCountdown ());
		}
	}

	void Update () {

		if (!detonate && timeFromStart < newExplosionDuration || loop) {
			Detonation ();
		}

		if (detonate) {
			if (debugPos) {
				transform.position = startPos;
			}
			Reset ();
		}
	}

	public IEnumerator KillCountdown() {
		yield return new WaitForSeconds (killTime);
		gameObject.SetActive (false);
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
			_shockwave.Play ();
		}

		// Heat distortion
		if (createHeatDistortion && _heatDistortion) {
			_heatDistortion.Play ();
		}

		if (applyForce) {
			ExplosionForce ();
		}

		startTime = Time.time;
		timeFromStart = Time.time - startTime;
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
		timeFromStart = Time.time - startTime;
		vertPos = (newVariation + timeFromStart) / deformSpeed;
		r = Mathf.Sin((vertPos) * (2 * Mathf.PI)) * 0.5f + 0.25f;
		g = Mathf.Sin((vertPos + 0.33333333f) * 2 * Mathf.PI) * 0.5f + 0.25f;
		b = Mathf.Sin((vertPos + 0.66666667f) * 2 * Mathf.PI) * 0.5f + 0.25f;

		// Additionally - do some overall extrusion
		dispVal = displacement.Evaluate(timeFromStart / newExplosionDuration);
		thisMat.SetFloat("_Displacement", dispVal);
	}

	void AdjustDisplacementMap() {
		dispOffsetY = displacementOffsetY.Evaluate(timeFromStart / newExplosionDuration);
		thisMat.SetTextureOffset("_DispTex", new Vector2(0, dispOffsetY));

		// Ensure that their sum = 1)
		correction = 1 / (r + g + b);
		r *= correction;
		g *= correction;
		b *= correction;

		thisMat.SetVector("_ChannelFactor", new Vector4(r,g,b,0));
	}

	void AdjustScale() {
		scaleFactor = scale.Evaluate(timeFromStart / newExplosionDuration);
		scaleFactor = scaleFactor + (scaleFactor * newVariation);
		transform.localScale = initScale * scaleFactor;
	}

	void AdjustClip() {
		endRange = maxRange.Evaluate(timeFromStart / newExplosionDuration);
		endRange = endRange + (endRange * newVariation);

		if (beginRange >= 1.0f) {
			beginRange = 1.0f;
		}

		if (endRange >= 1.0f) {
			endRange = 1.0f;
		}

		thisMat.SetVector("_Range", new Vector4(beginRange, endRange, 0, 0));

		clipVal = clip.Evaluate(timeFromStart / newExplosionDuration);
		thisMat.SetFloat("_ClipRange", clipVal);
	}

	void AdjustLighting() {
		_lighting.intensity = lightIntensity.Evaluate(timeFromStart / newExplosionDuration);
	}

	void ExplosionForce() {
		Vector3 explosionPos = transform.position;

		Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
		foreach (Collider hit in colliders) {

			if (hit.transform.IsChildOf (gameObject.transform)) {
				continue;
			}
				
			Rigidbody rb = hit.GetComponent<Rigidbody>();

			if (rb != null) {
				rb.AddExplosionForce (power * 100, explosionPos, radius, 1.5f);
			}

		}
	}

	void LaunchSmokeTrails() {
		Rigidbody smokeTrailRB;
		ParticleSystem particles;
		Vector3 randomDir;
		float mult = 10;
		int smokeTrailCount = Random.Range (minSmokeTrails, _smokeTrailsGroup.Count);

		// Iterate through array of smoke trails & apply forces
		for (int i = 0; i < smokeTrailCount; i++) {
			_smokeTrailsGroup [i].SetActive (true);
			particles = _smokeTrailsGroup [i].GetComponent<ParticleSystem> ();
			particles.Clear ();

			// Zero out any existing velocity
			smokeTrailRB = _smokeTrailsGroup[i].GetComponent<Rigidbody>();
			smokeTrailRB.velocity = Vector3.zero;

			randomDir = new Vector3 (
				mult * Random.Range(-explosionBias * 5, explosionBias * 5),
				mult * Random.Range(-explosionBias * 5, explosionBias * 10),
				mult * Random.Range(-explosionBias * 5, explosionBias * 5)
			);

			_smokeTrailsGroup [i].transform.position = transform.position;
			smokeTrailRB.AddForce (randomDir);
		}
	}

}