using UnityEngine;

public class LightSystem : MonoBehaviour
{
    public RenderTexture[] rTexture;   // 확인할 RenderTexture
    private Texture2D tex2D;
    private bool isExposedToLight = false;
    private int textureSize;
    public float lightThreshold = 0.48f;

    void Start()
    {
        // 텍스쳐 하나 가져와서 사이즈 저장
        textureSize = rTexture[0].width;

        tex2D = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);
    }

    // RenderTexture에서 한 번이라도 완전 흰색 픽셀이 발견되면 true 반환
    private bool CheckWhitePixel()
    {
        foreach (RenderTexture cTexture in rTexture)
        {
            RenderTexture.active = cTexture;
            tex2D.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            tex2D.Apply();
            RenderTexture.active = null;

            Color[] pixels = tex2D.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].r >= lightThreshold && pixels[i].g >= lightThreshold && pixels[i].b >= lightThreshold) // 완전 흰색
                {
                    return true; // 하나라도 발견 시 즉시 true 반환
                }
            }
        }

        return false;
    }

    void Update()
    {
        isExposedToLight = CheckWhitePixel();

        if (isExposedToLight)
        {
            Debug.Log("빛에 노출되었습니다!");
        }
    }
}
