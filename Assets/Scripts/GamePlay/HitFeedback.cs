using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// HitFeedback: ȭ�鿡 �ǰ� ȿ��(���� ��������)�� ī�޶� ����ũ�� ����ϴ� ��ƿ��Ƽ Ŭ����
// - UI Image�� ��Ÿ�ӿ� �����Ͽ� ���� Canvas�� Ǯ��ũ�� �������̸� ǥ��
// - ���̵� ��/Ȧ��/�ƿ� Ÿ�ְ̹� �ִ� ���ĸ� ���� ����
// - ī�޶� ����ũ �ɼ��� ���� ���� ��ġ �������� ��鸲 ��� (���� localPosition ���� ����)
public class HitFeedback : MonoBehaviour
{
    [Header("Sprite Source")]
    public Sprite hitSprite; // �������̿� ����� ��������Ʈ

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float maxAlpha = 0.8f; // �ִ� ���İ�
    public float fadeInTime = 0.3f;   // õõ�� �Ӿ��� (���̵� �� �ð�)
    public float holdTime = 0.2f;   // ���� ���� (�ִ� ���� ���� �ð�)
    public float fadeOutTime = 0.1f;   // ���� ����� (���̵� �ƿ� �ð�)
    public bool useUnscaledTime = false; // Ÿ�ֿ̹� UnscaledTime ��� ���� (�Ͻ����� �ÿ��� ȿ���� ���̷��� true)

    [Header("Canvas Target")]
    public Canvas targetCanvas; // �������̸� �߰��� Canvas (��������� ���� ù Canvas�� ���)

    [Header("Trigger Control")]
    [Tooltip("�ʹ� ����� ȣ���� ���̱� ���� �ּ� ����(��)")]
    public float minInterval = 0.03f; // ���� ȣ�� ������ ���� �ּ� ����
    float _lastPlayTime; // ������ ��� �ð�

    [Header("Camera Shake")]
    public bool enableShake = true; // ī�޶� ����ũ Ȱ��ȭ ����
    public Camera targetCamera;                 // ���� Camera.main
    public Transform cameraToShake;             // ������ ��鸱 Transform (���� targetCamera.transform)
    public float shakeDuration = 0.12f; // ��鸲 ���� �ð�
    public float shakeMagnitude = 0.08f;       // ��鸲 ����
    [Range(0f, 2f)] public float shakeDamping = 1.2f; // ��鸲 ���� ���

    Image _image; // ��Ÿ�ӿ� �����Ǵ� UI Image (��������)
    Coroutine _fadeCo, _shakeCo; // ���̵�� ����ũ�� �ڷ�ƾ �ڵ�
    Vector3 _basePos; // ī�޶��� ���� ���� ��ġ�� ���� (localPosition ��� world position ���)
    bool _cachedBase; // ���� ��ġ�� ĳ�õǾ����� ����



    void Awake()
    {
        // Ÿ�� Canvas�� �������� �ʾҴٸ� ������ ù ��° Canvas�� ã��
        if (targetCanvas == null) targetCanvas = FindFirstObjectByType<Canvas>();

        // UI Image ����: Canvas ������ Ǯ��ũ�� �������̷� �߰�
        GameObject go = new GameObject("HitSpriteUI");
        go.transform.SetParent(targetCanvas.transform, false);

        _image = go.AddComponent<Image>();
        _image.sprite = hitSprite;
        _image.color = new Color(1, 1, 1, 0); // ���� 0���� �����Ͽ� ������ ����
        _image.raycastTarget = false; // �Է� �������� �ʵ��� ����

        var rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero; // Ǯ��ũ������ Stretch

        // ī�޶� ���� �⺻�� ����: targetCamera �Ǵ� Camera.main ���
        if (targetCamera == null) targetCamera = Camera.main;
        if (cameraToShake == null && targetCamera != null) cameraToShake = targetCamera.transform;

        // Awake ������ ��ġ�� �������� ĳ������ ����
        // ī�޶��� ��ġ�� ��Ÿ�� �� ����� �� �����Ƿ� ���� ����ũ ���� ������ ���� ��ġ�� �ٽ� �����´�
        _cachedBase = false;
    }

    /// <summary>�ǰ� �� ȣ��</summary>
    public void Play()
    {
        if (_image == null || hitSprite == null) return; // �غ���� �ʾ����� ����

        // �ʹ� ����� ȣ�� ����: ������ ��� �ð��� ��
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (now - _lastPlayTime < minInterval) return;
        _lastPlayTime = now;

        _image.sprite = hitSprite; // ��������Ʈ�� �缳�� (���� ���ɼ� ���)

        // ���� ���ĸ� ���������� ����Ͽ� �ڿ������� ���̵� ������ ����
        float currentA = _image.color.a;

        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(ShowCoFrom(currentA));

        // ī�޶� ����ũ�� ���̵�� ���ķ� ����
        if (enableShake && cameraToShake != null)
        {
            // ����ũ ���� ������ ���� ���� ��ġ�� �������� ���̽� ��ġ�� ĸó
            _basePos = cameraToShake.position;
            _cachedBase = true;

            if (_shakeCo != null) StopCoroutine(_shakeCo);
            _shakeCo = StartCoroutine(ShakeCo());
        }
    }

    IEnumerator ShowCoFrom(float startAlpha)
    {
        _image.enabled = true; // �̹��� ǥ��

        float baseA = Mathf.Clamp01(startAlpha);
        float targetA = Mathf.Clamp01(maxAlpha);

        // Fade In: ���� ���� �� maxAlpha (EaseOutCubic ���� ���)
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeInTime));
            SetAlpha(Mathf.Lerp(baseA, targetA, EaseOutCubic(k)));
            yield return null;
        }
        SetAlpha(targetA);

        // Hold: ���� �ð� ����
        if (holdTime > 0f)
        {
            float h = 0f; while (h < holdTime) { h += Dt(); yield return null; }
        }

        // Fade Out: maxAlpha �� 0 (EaseOutCubic ���� ���)
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOutTime));
            SetAlpha(Mathf.Lerp(targetA, 0f, EaseOutCubic(k)));
            yield return null;
        }
        SetAlpha(0f);
        _image.enabled = false; // ������ ��Ȱ��ȭ

        _fadeCo = null; // �ڷ�ƾ �ڵ� �ʱ�ȭ
    }

    IEnumerator ShakeCo()
    {
        // ī�޶� ���� ��ġ�� �������� ���� �������� ���� ��鸲�� ���
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Dt();
            float p = Mathf.Clamp01(t / Mathf.Max(0.0001f, shakeDuration));
            float amp = Mathf.Max(0f, 1f - p * shakeDamping);       // �ð��� ���� ����
            Vector3 rnd = Random.insideUnitSphere * (shakeMagnitude * amp);
            cameraToShake.position = _basePos + rnd; // ���� ��ġ�� ����
            yield return null;
        }
        // ���� �� ���� ��ġ�� ���� (���� ��ġ ����)
        if (_cachedBase) cameraToShake.position = _basePos;
        _shakeCo = null;
    }

    // ��� ȿ���� �ʱ�ȭ: ���� ���� �� ī�޶� ��ġ ����
    public void ResetAlpha()
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = null;
        if (_image != null)
        {
            var c = _image.color; c.a = 0f; _image.color = c;
            _image.enabled = false;
        }

        if (_shakeCo != null) StopCoroutine(_shakeCo);
        _shakeCo = null;
        if (_cachedBase && cameraToShake != null)
            cameraToShake.position = _basePos; // ī�޶� ����ġ ���� (���� ��ġ)
    }

    // ���� ��ƿ: �̹��� ���� ����
    void SetAlpha(float a)
    {
        var c = _image.color; c.a = a; _image.color = c;
    }

    // ���� ��ƿ: DeltaTime ���� (Unscaled �Ǵ� Normal)
    float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    // ���� ��ƿ: �ε巯�� Ease-Out Cubic �Լ�
    float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
