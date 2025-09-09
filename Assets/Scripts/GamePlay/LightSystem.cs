using UnityEngine;

public class LightSystem : MonoBehaviour
{
    // ü�� �ý���
    [SerializeField]
    [Header("Health")]
    public HealthSystem healthSystem;
    [Header("Particle")]

    [SerializeField] public ParticleSystem headParticleSystem;
    [SerializeField] public float damagePerSecond = 10f;
    [SerializeField] public RenderTexture[] rTexture;   // Ȯ���� RenderTexture
    [SerializeField] public float lightThreshold = 0.48f;
    [SerializeField] public float particleShowSeconds = 0.5f;

    private Texture2D tex2D;
    private bool isExposedToLight = false;
    private int textureSize;
    private bool wasExposedToLight = false; // ��
    public bool IsStop { get; set; } = false;
    public bool lsOnShadow => !isExposedToLight;

    void Start()
    {
        // �ؽ��� �ϳ� �����ͼ� ������ ����
        textureSize = rTexture[0].width;

        tex2D = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        healthSystem = GetComponent<HealthSystem>();
    }

    // RenderTexture���� �� ���̶� ���� ��� �ȼ��� �߰ߵǸ� true ��ȯ
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
                if (pixels[i].r >= lightThreshold && pixels[i].g >= lightThreshold && pixels[i].b >= lightThreshold) // ���� ���
                {
                    return true; // �ϳ��� �߰� �� ��� true ��ȯ
                }
            }
        }

        return false;
    }

    void Update()
    {
        if (IsStop)
        {
            // ���� ���� ȿ�� ����
            if (headParticleSystem != null && headParticleSystem.isPlaying)
                headParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SoundManager.Instance.StopLoopSFX();

            // ���� ����
            wasExposedToLight = false;  // ���� ���� �ʱ�ȭ
            return;
        }

        var ending = FindFirstObjectByType<EndingEffect>();
        if (ending != null && ending.isPlaying) return;

        // ���� �޺� ���� ����
        isExposedToLight = CheckWhitePixel();

        // ���� ������(���� �߿���)
        if (isExposedToLight && healthSystem != null)
            healthSystem.ApplyDamage(damagePerSecond * Time.deltaTime);

        // ���� ���� ����
        if (isExposedToLight && !wasExposedToLight)
        {
            Debug.Log("���� ���� ����!");
            SoundManager.Instance.StartLoopSFX(SoundId.longSizzle);

            if (headParticleSystem != null)
            {
                headParticleSystem.Play();
                CancelInvoke(nameof(DisableHeadParticle));
            }
        }
        else if (!isExposedToLight && wasExposedToLight)
        {
            Debug.Log("�� ���� ����!");
            SoundManager.Instance.StopLoopSFX();

            if (headParticleSystem != null)
            {
                CancelInvoke(nameof(DisableHeadParticle));
                Invoke(nameof(DisableHeadParticle), particleShowSeconds);
            }
        }

        // ���� ����
        wasExposedToLight = isExposedToLight;
    }


    void DisableHeadParticle()
    {
        if (headParticleSystem != null)
            headParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        // �̹� ������ ��ƼŬ�� �ڿ� �Ҹ�, �� ��ƼŬ�� ����
    }

}
