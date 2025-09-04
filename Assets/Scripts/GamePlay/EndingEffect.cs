using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingEffect : MonoBehaviour
{
    [Header("Refs")]
    public GameObject player;
    public Transform coffinTransform;
    public Camera mainCamera;
    public RawImage irisImage;         // Iris 셰이더가 들어간 머티리얼을 할당한 RawImage

    [Header("Timings")]
    public float cameraMoveDuration = 3f;  // 카메라 클로즈업 시간
    public float irisSpeed = 1f;           // 아이리스 닫힘 속도 배수(1이면 대략 1초)

    [Header("Camera Offset")]
    public Vector3 endOffset = new Vector3(0f, 1f, -2f); // 관 기준 카메라 오프셋

    public bool isPlaying { get; private set; }
    public bool isFinished { get; private set; }

    // 셰이더 프로퍼티 가정: _Scale(float: 반지름), _Center(float2: 0..1)
    private Material irisMaterial;

    void Awake()
    {
        if (irisImage != null)
        {
            irisMaterial = irisImage.material;   // 인스턴스화 안 함 (요청 반영)
            irisImage.enabled = false;           // 시작 전 숨김
            irisImage.raycastTarget = false;
            // RawImage 색상은 흰색/알파1 권장(머티리얼이 알파 제어)
            if (irisImage.color.a < 1f) irisImage.color = new Color(1, 1, 1, 1);

            SetIrisScale(1f); // 시작은 화면 완전 오픈 상태
        }
    }

    public void PlayEnding()
    {
        if (isPlaying || mainCamera == null || coffinTransform == null || irisMaterial == null || irisImage == null)
            return;

        isPlaying = true;
        StartCoroutine(EndingCoroutine());
    }

    private IEnumerator EndingCoroutine()
    {
        isFinished = false;

        // 1) 카메라를 관 쪽으로 클로즈업
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = coffinTransform.position + endOffset;
        Quaternion endRot = Quaternion.LookRotation(coffinTransform.position - endPos, Vector3.up);

        float t = 0f;
        float dur = Mathf.Max(0.01f, cameraMoveDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float e = SmoothStep01(t);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, e);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, e);
            yield return null;
        }
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        // 2) 아이리스 아웃: 중심을 "관" 위치로 설정하고 닫기 시작
        irisImage.enabled = true;

        // ★ 중심을 ScreenPoint→UV(0..1)로 변환(캔버스 모드와 무관하게 정확)
        Vector3 sp = RectTransformUtility.WorldToScreenPoint(mainCamera, coffinTransform.position);
        float u = sp.x / Screen.width;
        float v = sp.y / Screen.height;
        irisMaterial.SetVector("_Center", new Vector4(u, v, 0f, 0f));

        // ★ 와이드 화면 가장자리 노출 방지 위해 시작 반지름을 넉넉히
        float startScale = 1.2f; // 1로 부족하면 1.3~1.4까지도 가능
        float endScale = 0f;
        SetIrisScale(startScale);

        t = 0f;
        float speed = Mathf.Max(0.01f, irisSpeed);
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float e = SmoothStep01(t);
            SetIrisScale(Mathf.Lerp(startScale, endScale, e));
            yield return null;
        }
        SetIrisScale(0f); // 완전 닫힘(블랙)

        isFinished = true;
        isPlaying = false;
    }

    private void SetIrisScale(float v)
    {
        if (irisMaterial != null) irisMaterial.SetFloat("_Scale", v);
    }

    private static float SmoothStep01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}
