using UnityEngine;

/// <summary>
/// Directional Light를 수평으로 회전시키되, 고도는 중앙값(fixedElevation)을 기준으로
/// 설정한 진폭만큼 위아래로 주기적으로 변하게 합니다.
/// - 수평 회전은 누적 방식(초당 각도)으로 계산하여 일시정지 등에서 더 안정적입니다.
/// - 고도는 사인파로 변화하며, 인스펙터에서 중앙값(fixedElevation), 진폭(elevationAmplitudeDeg), 속도(elevationSpeed)를 조절할 수 있습니다.
/// </summary>
[RequireComponent(typeof(Light))]
public class SimpleSunController : MonoBehaviour
{
    [Header("태양 설정")]
    [Tooltip("태양이 수평으로 회전하는 속도입니다. (단위: 도/초)")]
    public float rotationSpeed = 10f;

    [Tooltip("태양의 고정된 중앙 높이(고도)입니다. 이 값을 중심으로 위/아래로 진동합니다.")]
    [Range(0f, 90f)]
    public float fixedElevation = 45f;

    [Tooltip("fixedElevation을 기준으로 위/아래로 흔들리는 진폭(도). 실제 고도는 fixedElevation ± amplitude가 됩니다.")]
    public float elevationAmplitudeDeg = 15f;

    [Tooltip("고도 진동 속도(주파수, 단위: 주기/초). 값이 클수록 고도 변화가 빨라집니다.")]
    public float elevationSpeed = 0.2f;

    // 내부 누적 각도(도) - Time.time 대신 deltaTime 누적으로 회전 상태를 관리
    private float _azimuthAngleDeg;
    // 내부 시간 누적(고도 사인 계산용)
    private float _time;

    void Update()
    {
        // 시간 누적
        _time += Time.deltaTime;

        // 아지무스(수평) 각 누적: deltaTime 기반으로 더해줌
        _azimuthAngleDeg += rotationSpeed * Time.deltaTime;

        // 아지무스 각을 0-360 범위로 래핑하여 수치가 계속 커지지 않도록 함
        _azimuthAngleDeg = Mathf.Repeat(_azimuthAngleDeg, 360f);

        // 수평 회전 계산
        Quaternion horizontalRotation = Quaternion.Euler(0f, _azimuthAngleDeg, 0f);

        // 고도(엘리베이션)를 사인파로 변화시키기
        // elevationSpeed는 '주기/초'로 해석하여 내부적으로 2π를 곱해 라디안 입력으로 사용
        float elevationOffset = Mathf.Sin(_time * elevationSpeed * Mathf.PI * 2f) * elevationAmplitudeDeg;
        float currentElevation = fixedElevation + elevationOffset;

        // 수직 회전(고도) 계산
        Quaternion verticalRotation = Quaternion.Euler(currentElevation, 0f, 0f);

        // 결합된 회전 적용 (수평 * 수직)
        transform.rotation = horizontalRotation * verticalRotation;
    }

    private void OnValidate()
    {
        // 안전 범위로 보정
        rotationSpeed = rotationSpeed;
        fixedElevation = Mathf.Clamp(fixedElevation, 0f, 90f);
        elevationAmplitudeDeg = Mathf.Max(0f, elevationAmplitudeDeg);
        elevationSpeed = Mathf.Max(0f, elevationSpeed);

        // 진폭이 너무 커서 고도가 음수나 지나치게 큰 값이 되지 않도록 조정 권장(자동 보정은 하지 않음)
    }
}