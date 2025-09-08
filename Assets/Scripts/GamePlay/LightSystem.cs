using UnityEngine;

public class LightSystem : MonoBehaviour
{
    // 체력 시스템
    [SerializeField]
    [Header("Health")]
    public HealthSystem healthSystem;
    [Header("Particle")]

    [SerializeField] public ParticleSystem headParticleSystem;
    [SerializeField] public float damagePerSecond = 10f;
    [SerializeField] public RenderTexture[] rTexture;   // 확인할 RenderTexture
    [SerializeField] public float lightThreshold = 0.48f;
    [SerializeField] public float particleShowSeconds = 0.5f;

    private Texture2D tex2D;
    private bool isExposedToLight = false;
    private int textureSize;
    private bool wasExposedToLight = false; // 빛
    public bool IsStop { get; set; } = false;
    public bool lsOnShadow => !isExposedToLight;

    void Start()
    {
        // 텍스쳐 하나 가져와서 사이즈 저장
        textureSize = rTexture[0].width;

        tex2D = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        healthSystem = GetComponent<HealthSystem>();
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
        if (IsStop)
        {
            // 진행 중인 효과 중지
            if (headParticleSystem != null && headParticleSystem.isPlaying)
                headParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SoundManager.Instance.StopLoopSFX();

            // 상태 갱신
            wasExposedToLight = false;  // 노출 상태 초기화
            return;
        }

        var ending = FindFirstObjectByType<EndingEffect>();
        if (ending != null && ending.isPlaying) return;

        // 현재 햇빛 노출 판정
        isExposedToLight = CheckWhitePixel();

        // 지속 데미지(노출 중에만)
        if (isExposedToLight && healthSystem != null)
            healthSystem.ApplyDamage(damagePerSecond * Time.deltaTime);

        // 상태 전이 감지
        if (isExposedToLight && !wasExposedToLight)
        {
            Debug.Log("빛에 노출 시작!");
            SoundManager.Instance.StartLoopSFX(SoundId.longSizzle);

            if (headParticleSystem != null)
            {
                headParticleSystem.Play();
                CancelInvoke(nameof(DisableHeadParticle));
            }
        }
        else if (!isExposedToLight && wasExposedToLight)
        {
            Debug.Log("빛 노출 종료!");
            SoundManager.Instance.StopLoopSFX();

            if (headParticleSystem != null)
            {
                CancelInvoke(nameof(DisableHeadParticle));
                Invoke(nameof(DisableHeadParticle), particleShowSeconds);
            }
        }

        // 상태 갱신
        wasExposedToLight = isExposedToLight;
    }


    void DisableHeadParticle()
    {
        if (headParticleSystem != null)
            headParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        // 이미 생성된 파티클은 자연 소멸, 새 파티클만 중지
    }

}
