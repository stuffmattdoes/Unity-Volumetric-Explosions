using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
//[AddComponentMenu("Image Effects/PixelBoy")]
public class PixelBoy : MonoBehaviour {

	// Variables
	public int widthResolution = 720;
	public FilterMode filterMode = FilterMode.Point;

    private int heightResolution;
    
	protected void Start() {
        
		if (!SystemInfo.supportsImageEffects) {
            enabled = false;
            return;
        }

    }

    void Update() {

        float ratio = ((float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth);
		heightResolution = Mathf.RoundToInt(widthResolution * ratio);
        
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
		source.filterMode = filterMode;
		RenderTexture buffer = RenderTexture.GetTemporary(widthResolution, heightResolution, -1);
		buffer.filterMode = filterMode;
        Graphics.Blit(source, buffer);
        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }

}