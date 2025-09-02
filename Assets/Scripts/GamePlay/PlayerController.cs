using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 이동 관련 설정
    [Header("Movement Settings")]
    // 최대 이동 속도 (Inspector에서 조정 가능)
    public float _moveSpeed = 5f;
    // 정지에서 최대 속도에 도달할 때까지의 가속량(클수록 더 빨리 속도 증가)
    public float _acceleration = 20f;
    // 입력 해제 시 감속률(클수록 더 빠르게 멈춤)
    public float _deceleration = 25f;
    // 캐릭터가 이동 방향을 바라보는 회전 속도(도/초)
    public float _rotationSpeed = 720f; // degrees per second

    // 입력 관련
    [Header("Input")]
    // PlayerControls 에서 만든 'Move' 액션을 참조할 InputActionReference.
    // 이 필드에 에셋에서 생성한 Move 액션을 드래그해서 연결하세요 (Vector2).
    public InputActionReference moveAction;

    // 내부 상태 저장용 필드
    Rigidbody _rb; // 이 스크립트가 제어할 Rigidbody 컴포넌트
    Vector2 _moveInput; // 최신 입력값 (x: 좌/우, y: 앞/뒤)
    Vector3 _currentPlanarVelocity; // X-Z 평면에서 현재 목표 속도(중력 영향은 Y 컴포넌트로 별도 유지)

    // Awake: 컴포넌트 초기화 및 Rigidbody 제약 적용
    void Awake()
    {
        // Rigidbody 컴포넌트 획득
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            // 없으면 런타임에 추가하고 경고 출력
            Debug.LogWarning("PlayerController requires a Rigidbody. Adding one at runtime.");
            _rb = gameObject.AddComponent<Rigidbody>();
        }

        // 사양에 따른 Rigidbody 제약 설정
        // Y 위치 고정은 제거하여 중력이 동작하도록 함. 다만 X/Z 축 회전은 고정하여 흔들림 방지.
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 초기 평면 속도는 Rigidbody의 현재 속도에서 X,Z만 취함
        // (Y 성분은 중력/점프 등 물리 시스템에 맡김)
        _currentPlanarVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
    }

    // OnEnable: InputAction 콜백 등록 및 활성화
    void OnEnable()
    {
        // moveAction이 유효하면 performed와 canceled 이벤트를 구독
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
            moveAction.action.Enable();
        }
        else
        {
            // 연결 누락 시 경고
            Debug.LogWarning("Move ActionReference is not assigned on PlayerController. Assign the PlayerControls 'Move' action in the Inspector.");
        }
    }

    // OnDisable: 콜백 해제 및 비활성화
    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }
    }

    // 입력이 발생했을 때 호출되는 콜백
    // performed 이벤트에서 입력 벡터를 읽어 _moveInput에 저장
    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    // 입력이 취소되었을 때 호출되는 콜백
    // 입력을 초기화하여 감속 로직으로 자연스럽게 멈추게 함
    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _moveInput = Vector2.zero;
    }

    // FixedUpdate: 모든 물리 처리(속도 보간 및 회전)를 여기서 수행
    void FixedUpdate()
    {
        // 목표 평면 속도 계산 (입력 x->X, 입력 y->Z)
        Vector3 desiredPlanar = new Vector3(_moveInput.x, 0f, _moveInput.y) * _moveSpeed;

        // 현재와 목표 속도의 크기 비교로 가속/감속 여부 결정
        float currentMag = _currentPlanarVelocity.magnitude;
        float desiredMag = desiredPlanar.magnitude;

        // 한 프레임에서 허용할 최대 속도 변화량 계산
        float maxDelta = (desiredMag > currentMag) ? _acceleration * Time.fixedDeltaTime : _deceleration * Time.fixedDeltaTime;

        // 속도를 부드럽게 목표 속도로 이동시킴 (갑작스러운 변화 방지)
        _currentPlanarVelocity = Vector3.MoveTowards(_currentPlanarVelocity, desiredPlanar, maxDelta);

        // Rigidbody의 velocity를 직접 설정하되 Y 성분은 기존 값을 유지하여 중력/낙하를 허용
        _rb.linearVelocity = new Vector3(_currentPlanarVelocity.x, _rb.linearVelocity.y, _currentPlanarVelocity.z);

        // 이동 중일 때만 캐릭터가 이동 방향을 바라보도록 회전 적용
        if (_currentPlanarVelocity.sqrMagnitude > 0.001f)
        {
            Vector3 lookDir = _currentPlanarVelocity.normalized;
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            // 부드러운 회전 (최대 회전량은 _rotationSpeed * deltaTime)
            Quaternion newRot = Quaternion.RotateTowards(_rb.rotation, targetRot, _rotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(newRot);
        }
    }

    // 에디터에서 값 입력 시 최소값 보정
    void OnValidate()
    {
        _moveSpeed = Mathf.Max(0f, _moveSpeed);
        _acceleration = Mathf.Max(0f, _acceleration);
        _deceleration = Mathf.Max(0f, _deceleration);
        _rotationSpeed = Mathf.Max(0f, _rotationSpeed);
    }
}
