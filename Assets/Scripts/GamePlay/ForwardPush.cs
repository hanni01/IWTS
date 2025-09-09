using UnityEngine;

public class ForwardPush : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [Header("Push Force Settings")]
    [SerializeField] private float maxPushForce = 5f;           // �ִ� ��
    [SerializeField] private float duration = 20f;             // ���� �ִ뿡 �����ϴ� �ð�
    [SerializeField]
    private AnimationCurve pushForceCurve =
        AnimationCurve.Linear(0f, 0f, 1f, 1f);              // �⺻ ���� Ŀ��

    private float elapsedTime = 0f;
    public bool IsStop = false;

    void FixedUpdate()
    {
        if (!IsStop)
        {
            elapsedTime += Time.fixedDeltaTime;

            // 0~1 ������ ����ȭ
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Ŀ�� �� ��������
            float curveValue = pushForceCurve.Evaluate(normalizedTime);

            // ���� �� ���
            float currentPushForce = curveValue * maxPushForce;

            // ������ �� ����
            rb.AddForce(transform.forward * currentPushForce, ForceMode.Force);
        }
        
    }
}
