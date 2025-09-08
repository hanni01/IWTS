using UnityEngine;

public class ForwardPush : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [Header("Push Force Settings")]
    [SerializeField] private float maxPushForce = 5f;           // 최대 힘
    [SerializeField] private float duration = 20f;             // 힘이 최대에 도달하는 시간
    [SerializeField]
    private AnimationCurve pushForceCurve =
        AnimationCurve.Linear(0f, 0f, 1f, 1f);              // 기본 선형 커브

    private float elapsedTime = 0f;
    public bool IsStop = false;

    void FixedUpdate()
    {
        if (!IsStop)
        {
            elapsedTime += Time.fixedDeltaTime;

            // 0~1 범위로 정규화
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // 커브 값 가져오기
            float curveValue = pushForceCurve.Evaluate(normalizedTime);

            // 현재 힘 계산
            float currentPushForce = curveValue * maxPushForce;

            // 앞으로 힘 적용
            rb.AddForce(transform.forward * currentPushForce, ForceMode.Force);
        }
        
    }
}
