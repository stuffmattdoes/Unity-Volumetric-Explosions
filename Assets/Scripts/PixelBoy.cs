using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/PixelBoy")]
public class PixelBoy : MonoBehaviour
{
    public int w = 720;
    int h;
    protected void Start()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
    }
    void Update() {

        float ratio = ((float)Camera.main.pixelHeight / (float)Camera.main.pixelWidth);
        h = Mathf.RoundToInt(w * ratio);
        
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        source.filterMode = FilterMode.Point;
        RenderTexture buffer = RenderTexture.GetTemporary(w, h, -1);
        buffer.filterMode = FilterMode.Point;
        Graphics.Blit(source, buffer);
        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }
}