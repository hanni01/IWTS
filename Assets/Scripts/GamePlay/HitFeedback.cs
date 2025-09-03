using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitFeedback : MonoBehaviour
{
    [Header("Sprite Source")]
    public Sprite hitSprite;

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float maxAlpha = 0.8f;
    public float fadeInTime = 0.3f;   // 천천히 붉어짐
    public float holdTime = 0.2f;   // 조금 유지
    public float fadeOutTime = 0.1f;   // 빨리 사라짐
    public bool useUnscaledTime = false;

    [Header("Canvas Target")]
    public Canvas targetCanvas;

    [Header("Trigger Control")]
    [Tooltip("너무 빈번한 호출을 줄이기 위한 최소 간격(초)")]
    public float minInterval = 0.03f;
    float _lastPlayTime;

    [Header("Camera Shake")]
    public bool enableShake = true;
    public Camera targetCamera;                 // 비우면 Camera.main
    public Transform cameraToShake;             // 비우면 targetCamera.transform
    public float shakeDuration = 0.12f;
    public float shakeMagnitude = 0.08f;       // 카메라 로컬 좌표 기준
    [Range(0f, 2f)] public float shakeDamping = 1.2f;

    Image _image;
    Coroutine _fadeCo, _shakeCo;
    Vector3 _baseLocalPos;
    bool _cachedBase;



    void Awake()
    {
        if (targetCanvas == null) targetCanvas = FindObjectOfType<Canvas>();

        // UI Image 생성
        GameObject go = new GameObject("HitSpriteUI");
        go.transform.SetParent(targetCanvas.transform, false);

        _image = go.AddComponent<Image>();
        _image.sprite = hitSprite;
        _image.color = new Color(1, 1, 1, 0); // 알파 0 시작
        _image.raycastTarget = false;

        var rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 카메라 참조 기본값
        if (targetCamera == null) targetCamera = Camera.main;
        if (cameraToShake == null && targetCamera != null) cameraToShake = targetCamera.transform;
        if (cameraToShake != null)
        {
            _baseLocalPos = cameraToShake.localPosition;
            _cachedBase = true;
        }
    }

    /// <summary>피격 시 호출</summary>
    public void Play()
    {
        if (_image == null || hitSprite == null) return;

        // 너무 빈번한 호출 방지
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (now - _lastPlayTime < minInterval) return;
        _lastPlayTime = now;

        _image.sprite = hitSprite;

        // 현재 알파를 시작점으로 사용
        float currentA = _image.color.a;

        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(ShowCoFrom(currentA));

        // 카메라 쉐이크 병렬 실행
        if (enableShake && cameraToShake != null)
        {
            if (!_cachedBase)
            {
                _baseLocalPos = cameraToShake.localPosition;
                _cachedBase = true;
            }
            if (_shakeCo != null) StopCoroutine(_shakeCo);
            _shakeCo = StartCoroutine(ShakeCo());
        }
    }

    IEnumerator ShowCoFrom(float startAlpha)
    {
        _image.enabled = true;

        float baseA = Mathf.Clamp01(startAlpha);
        float targetA = Mathf.Clamp01(maxAlpha);

        // Fade In: 현재 알파 → maxAlpha
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeInTime));
            SetAlpha(Mathf.Lerp(baseA, targetA, EaseOutCubic(k)));
            yield return null;
        }
        SetAlpha(targetA);

        // Hold
        if (holdTime > 0f)
        {
            float h = 0f; while (h < holdTime) { h += Dt(); yield return null; }
        }

        // Fade Out: maxAlpha → 0
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOutTime));
            SetAlpha(Mathf.Lerp(targetA, 0f, EaseOutCubic(k)));
            yield return null;
        }
        SetAlpha(0f);
        _image.enabled = false;

        _fadeCo = null;
    }

    IEnumerator ShakeCo()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Dt();
            float p = Mathf.Clamp01(t / Mathf.Max(0.0001f, shakeDuration));
            float amp = Mathf.Max(0f, 1f - p * shakeDamping);       // 감쇠
            Vector3 rnd = Random.insideUnitSphere * (shakeMagnitude * amp);
            cameraToShake.localPosition = _baseLocalPos + rnd;
            yield return null;
        }
        // 원위치
        if (_cachedBase) cameraToShake.localPosition = _baseLocalPos;
        _shakeCo = null;
    }

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
            cameraToShake.localPosition = _baseLocalPos;
    }

    void SetAlpha(float a)
    {
        var c = _image.color; c.a = a; _image.color = c;
    }

    float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
