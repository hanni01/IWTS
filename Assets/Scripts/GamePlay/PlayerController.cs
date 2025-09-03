using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

// 플레이어 캐릭터의 이동과 점프를 제어하는 스크립트입니다.
// Rigidbody 기반의 물리 계산을 사용하며, 일관된 단일 점프 로직을 가집니다.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region 인스펙터 변수

    [Header("MOVEMENT SETTINGS")]
    [Tooltip("캐릭터의 최대 이동 속도입니다.")]
    [SerializeField] private float _moveSpeed = 7f;
    [Tooltip("최대 속도에 도달하기까지의 가속도입니다. 높을수록 빠르게 최대 속도에 도달합니다.")]
    [SerializeField] private float _acceleration = 80f;
    [Tooltip("입력이 없을 때 정지하기까지의 감속도입니다. 높을수록 빠르게 멈춥니다.")]
    [SerializeField] private float _deceleration = 120f;
    [Tooltip("캐릭터가 이동 방향으로 회전하는 속도입니다.")]
    [SerializeField] private float _rotationSpeed = 1080f;

    [Header("JUMP SETTINGS")]
    [Tooltip("점프 시 가해지는 초기 힘의 크기입니다.")]
    [SerializeField] private float _jumpForce = 12f;
    [Tooltip("점프하여 상승하는 동안 적용될 중력 배율입니다. 높을수록 덜 붕 뜹니다.")]
    [SerializeField] private float _jumpGravityMultiplier = 2.0f;
    [Tooltip("점프 정점에서 떨어질 때 적용되는 추가 중력 배율입니다. (쫀득한 점프감)")]
    [SerializeField] private float _fallMultiplier = 3.0f;
    [Tooltip("공중에서 캐릭터를 좌우로 제어할 수 있는 정도입니다. (0: 제어 불가, 1: 지상과 동일)")]
    [SerializeField][Range(0f, 1f)] private float _airControlMultiplier = 0.7f;

    [Header("RESPONSIVENESS BUFFERS")]
    [Tooltip("발판에서 떨어진 직후에도 점프가 가능한 유예 시간입니다.")]
    [SerializeField] private float _coyoteTime = 0.15f;
    [Tooltip("착지 직전에 점프를 미리 입력할 수 있는 유예 시간입니다.")]
    [SerializeField] private float _jumpBufferTime = 0.15f;

    [Header("GROUND DETECTION")]
    [Tooltip("지면을 감지할 위치를 지정하는 Transform 입니다. (미지정 시 캐릭터 위치 사용)")]
    [SerializeField] private Transform _groundCheckPoint;
    [Tooltip("지면 감지 SphereCast의 반지름입니다.")]
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [Tooltip("지면 감지 SphereCast의 길이입니다.")]
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [Tooltip("지면으로 인식할 레이어를 설정합니다.")]
    [SerializeField] private LayerMask _groundLayer;

    #endregion

    #region 내부 상태 변수

    // Rigidbody 컴포넌트(이 스크립트가 제어하는 물리 몸체)
    private Rigidbody _rb;

    // 입력값 및 상태
    private Vector2 _moveInput;            // 최신 이동 입력값 (x: 좌/우, y: 앞/뒤)
    private bool _isGrounded;              // 현재 지면에 닿아있는지 여부
    private float _coyoteTimeCounter;      // 코요테 타이머 카운터
    private float _jumpBufferCounter;      // 점프 버퍼 타이머 카운터

    #endregion

    #region 유니티 라이프사이클

    // Awake: 컴포넌트 초기화, Rigidbody 제약 설정
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Update: 입력/상태 타이머 업데이트
    private void Update()
    {
        HandleState();
        HandleTimers();
    }

    // FixedUpdate: 물리 연산(점프, 이동, 중력) 처리
    private void FixedUpdate()
    {
        HandleJump();
        HandleMovement();
        HandleGravity();
    }

    // 에디터에서 값 입력 시 최소값 검증
    private void OnValidate()
    {
        _jumpForce = Mathf.Max(0f, _jumpForce);
        _jumpGravityMultiplier = Mathf.Max(1f, _jumpGravityMultiplier);
        _fallMultiplier = Mathf.Max(1f, _fallMultiplier);
    }

    #endregion

    #region 입력 처리 (PlayerInput에서 호출)

    // Move 액션 콜백: 이동 입력 업데이트
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // Jump 액션 콜백: performed 시점에 점프 버퍼 활성화
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
    }

    #endregion

    #region 상태 판정 및 타이머

    // 지면 판정: SphereCast를 사용하여 groundLayer에 해당하는 콜라이더를 감지
    private void HandleState()
    {
        Transform groundCheckOrigin = _groundCheckPoint != null ? _groundCheckPoint : transform;
        _isGrounded = Physics.SphereCast(groundCheckOrigin.position, _groundCheckRadius, Vector3.down, out _, _groundCheckDistance, _groundLayer);
    }

    // 코요테 타이머와 점프 버퍼 카운트 감소 처리
    private void HandleTimers()
    {
        if (_isGrounded)
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (_jumpBufferCounter > 0)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    #endregion

    #region 물리 처리 (점프, 이동, 중력)

    // 점프 실행: 코요테 및 버퍼 조건이 만족할 때 한 번의 점프를 수행
    private void HandleJump()
    {
        if (_coyoteTimeCounter > 0f && _jumpBufferCounter > 0f)
        {
            // 수직 속도 초기화 후 즉시 상승 속도 부여
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
            _jumpBufferCounter = 0f;
        }
    }

    // 이동 처리: 월드 기준 입력을 사용하여 목표 속도로 부드럽게 보간
    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(_moveInput.x, 0f, _moveInput.y);
        float controlMultiplier = _isGrounded ? 1f : _airControlMultiplier;
        Vector3 targetVelocity = moveDirection * _moveSpeed * controlMultiplier;

        float accel = moveDirection.magnitude > 0.1f ? _acceleration : _deceleration;
        Vector3 currentPlanarVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        Vector3 newPlanarVelocity = Vector3.MoveTowards(
            currentPlanarVelocity,
            targetVelocity,
            accel * Time.fixedDeltaTime
        );

        _rb.linearVelocity = new Vector3(newPlanarVelocity.x, _rb.linearVelocity.y, newPlanarVelocity.z);

        // 이동 중일 때만 캐릭터가 이동 방향을 바라보도록 회전
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // 중력 보정: 공중에서만 가속도 기반의 중력 변화 적용
    private void HandleGravity()
    {
        if (!_isGrounded)
        {
            float verticalVelocity = _rb.linearVelocity.y;

            // 하강 시 중력 강화
            if (verticalVelocity < 0)
            {
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            // 상승 시 추가 중력 적용(부드러운 상승 억제)
            else if (verticalVelocity > 0)
            {
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_jumpGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
    }

    private void OnParticleCollision(GameObject goal)
    {
        // goal 오브젝트의 태그가 Goal인지 확인
        if (!goal.CompareTag("Goal")) return;

        var currentStageName = UnitySceneManager.GetActiveScene().name;
        Debug.Log($"골인 지점 도달 {currentStageName}");
        GameManager.Stage.ClearedStage(currentStageName);
    }

    #endregion
}

