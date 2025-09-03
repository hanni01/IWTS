using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// HitFeedback: 화면에 피격 효과(빨간 오버레이)와 카메라 쉐이크를 재생하는 유틸리티 클래스
// - UI Image를 런타임에 생성하여 지정 Canvas에 풀스크린 오버레이를 표시
// - 페이드 인/홀드/아웃 타이밍과 최대 알파를 조절 가능
// - 카메라 쉐이크 옵션을 통해 월드 위치 기준으로 흔들림 재생 (기존 localPosition 문제 수정)
public class HitFeedback : MonoBehaviour
{
    [Header("Sprite Source")]
    public Sprite hitSprite; // 오버레이에 사용할 스프라이트

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float maxAlpha = 0.8f; // 최대 알파값
    public float fadeInTime = 0.3f;   // 천천히 붉어짐 (페이드 인 시간)
    public float holdTime = 0.2f;   // 조금 유지 (최대 알파 유지 시간)
    public float fadeOutTime = 0.1f;   // 빨리 사라짐 (페이드 아웃 시간)
    public bool useUnscaledTime = false; // 타이밍에 UnscaledTime 사용 여부 (일시정지 시에도 효과를 보이려면 true)

    [Header("Canvas Target")]
    public Canvas targetCanvas; // 오버레이를 추가할 Canvas (비어있으면 씬의 첫 Canvas를 사용)

    [Header("Trigger Control")]
    [Tooltip("너무 빈번한 호출을 줄이기 위한 최소 간격(초)")]
    public float minInterval = 0.03f; // 연속 호출 제한을 위한 최소 간격
    float _lastPlayTime; // 마지막 재생 시간

    [Header("Camera Shake")]
    public bool enableShake = true; // 카메라 쉐이크 활성화 여부
    public Camera targetCamera;                 // 비우면 Camera.main
    public Transform cameraToShake;             // 실제로 흔들릴 Transform (비우면 targetCamera.transform)
    public float shakeDuration = 0.12f; // 흔들림 지속 시간
    public float shakeMagnitude = 0.08f;       // 흔들림 세기
    [Range(0f, 2f)] public float shakeDamping = 1.2f; // 흔들림 감쇠 계수

    Image _image; // 런타임에 생성되는 UI Image (오버레이)
    Coroutine _fadeCo, _shakeCo; // 페이드와 쉐이크용 코루틴 핸들
    Vector3 _basePos; // 카메라의 원래 월드 위치를 저장 (localPosition 대신 world position 사용)
    bool _cachedBase; // 원래 위치가 캐시되었는지 여부



    void Awake()
    {
        // 타겟 Canvas가 지정되지 않았다면 씬에서 첫 번째 Canvas를 찾음
        if (targetCanvas == null) targetCanvas = FindObjectOfType<Canvas>();

        // UI Image 생성: Canvas 하위에 풀스크린 오버레이로 추가
        GameObject go = new GameObject("HitSpriteUI");
        go.transform.SetParent(targetCanvas.transform, false);

        _image = go.AddComponent<Image>();
        _image.sprite = hitSprite;
        _image.color = new Color(1, 1, 1, 0); // 알파 0으로 시작하여 보이지 않음
        _image.raycastTarget = false; // 입력 차단하지 않도록 설정

        var rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero; // 풀스크린으로 Stretch

        // 카메라 참조 기본값 설정: targetCamera 또는 Camera.main 사용
        if (targetCamera == null) targetCamera = Camera.main;
        if (cameraToShake == null && targetCamera != null) cameraToShake = targetCamera.transform;

        // Awake 시점에 위치를 고정으로 캐시하지 않음
        // 카메라의 위치는 런타임 중 변경될 수 있으므로 실제 쉐이크 시작 시점에 기준 위치를 다시 가져온다
        _cachedBase = false;
    }

    /// <summary>피격 시 호출</summary>
    public void Play()
    {
        if (_image == null || hitSprite == null) return; // 준비되지 않았으면 무시

        // 너무 빈번한 호출 방지: 마지막 재생 시간과 비교
        float now = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (now - _lastPlayTime < minInterval) return;
        _lastPlayTime = now;

        _image.sprite = hitSprite; // 스프라이트를 재설정 (변경 가능성 대비)

        // 현재 알파를 시작점으로 사용하여 자연스러운 페이드 연결을 지원
        float currentA = _image.color.a;

        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(ShowCoFrom(currentA));

        // 카메라 쉐이크는 페이드와 병렬로 실행
        if (enableShake && cameraToShake != null)
        {
            // 쉐이크 시작 시점에 현재 월드 위치를 기준으로 베이스 위치를 캡처
            _basePos = cameraToShake.position;
            _cachedBase = true;

            if (_shakeCo != null) StopCoroutine(_shakeCo);
            _shakeCo = StartCoroutine(ShakeCo());
        }
    }

    IEnumerator ShowCoFrom(float startAlpha)
    {
        _image.enabled = true; // 이미지 표시

        float baseA = Mathf.Clamp01(startAlpha);
        float targetA = Mathf.Clamp01(maxAlpha);

        // Fade In: 현재 알파 → maxAlpha (EaseOutCubic 보간 사용)
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeInTime));
            SetAlpha(Mathf.Lerp(baseA, targetA, EaseOutCubic(k)));
            yield return null;
        }
        SetAlpha(targetA);

        // Hold: 일정 시간 유지
        if (holdTime > 0f)
        {
            float h = 0f; while (h < holdTime) { h += Dt(); yield return null; }
        }

        // Fade Out: maxAlpha → 0 (EaseOutCubic 보간 사용)
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Dt();
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, fadeOutTime));
            SetAlpha(Mathf.Lerp(targetA, 0f, EaseOutCubic(k)));
            yield return null;
        }
        SetAlpha(0f);
        _image.enabled = false; // 끝나면 비활성화

        _fadeCo = null; // 코루틴 핸들 초기화
    }

    IEnumerator ShakeCo()
    {
        // 카메라 월드 위치를 기준으로 랜덤 오프셋을 더해 흔들림을 재생
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Dt();
            float p = Mathf.Clamp01(t / Mathf.Max(0.0001f, shakeDuration));
            float amp = Mathf.Max(0f, 1f - p * shakeDamping);       // 시간에 따라 감쇠
            Vector3 rnd = Random.insideUnitSphere * (shakeMagnitude * amp);
            cameraToShake.position = _basePos + rnd; // 월드 위치로 적용
            yield return null;
        }
        // 종료 시 원래 위치로 복원 (월드 위치 기준)
        if (_cachedBase) cameraToShake.position = _basePos;
        _shakeCo = null;
    }

    // 즉시 효과를 초기화: 알파 리셋 및 카메라 위치 복원
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
            cameraToShake.position = _basePos; // 카메라 원위치 복원 (월드 위치)
    }

    // 내부 유틸: 이미지 알파 설정
    void SetAlpha(float a)
    {
        var c = _image.color; c.a = a; _image.color = c;
    }

    // 내부 유틸: DeltaTime 선택 (Unscaled 또는 Normal)
    float Dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    // 내부 유틸: 부드러운 Ease-Out Cubic 함수
    float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
}
