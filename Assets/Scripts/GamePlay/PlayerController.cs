using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

// �÷��̾� ĳ������ �̵��� ������ �����ϴ� ��ũ��Ʈ�Դϴ�.
// Rigidbody ����� ���� ����� ����ϸ�, �ϰ��� ���� ���� ������ �����ϴ�.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region �ν����� ����

    [Header("MOVEMENT SETTINGS")]
    [Tooltip("ĳ������ �ִ� �̵� �ӵ��Դϴ�.")]
    [SerializeField] private float _moveSpeed = 7f;
    [Tooltip("�ִ� �ӵ��� �����ϱ������ ���ӵ��Դϴ�. �������� ������ �ִ� �ӵ��� �����մϴ�.")]
    [SerializeField] private float _acceleration = 80f;
    [Tooltip("�Է��� ���� �� �����ϱ������ ���ӵ��Դϴ�. �������� ������ ����ϴ�.")]
    [SerializeField] private float _deceleration = 120f;
    [Tooltip("ĳ���Ͱ� �̵� �������� ȸ���ϴ� �ӵ��Դϴ�.")]
    [SerializeField] private float _rotationSpeed = 1080f;

    [Header("JUMP SETTINGS")]
    [Tooltip("���� �� �������� �ʱ� ���� ũ���Դϴ�.")]
    [SerializeField] private float _jumpForce = 12f;
    [Tooltip("�����Ͽ� ����ϴ� ���� ����� �߷� �����Դϴ�. �������� �� �� ��ϴ�.")]
    [SerializeField] private float _jumpGravityMultiplier = 2.0f;
    [Tooltip("���� �������� ������ �� ����Ǵ� �߰� �߷� �����Դϴ�. (�˵��� ������)")]
    [SerializeField] private float _fallMultiplier = 3.0f;
    [Tooltip("���߿��� ĳ���͸� �¿�� ������ �� �ִ� �����Դϴ�. (0: ���� �Ұ�, 1: ����� ����)")]
    [SerializeField][Range(0f, 1f)] private float _airControlMultiplier = 0.7f;

    [Header("RESPONSIVENESS BUFFERS")]
    [Tooltip("���ǿ��� ������ ���Ŀ��� ������ ������ ���� �ð��Դϴ�.")]
    [SerializeField] private float _coyoteTime = 0.15f;
    [Tooltip("���� ������ ������ �̸� �Է��� �� �ִ� ���� �ð��Դϴ�.")]
    [SerializeField] private float _jumpBufferTime = 0.15f;

    [Header("GROUND DETECTION")]
    [Tooltip("������ ������ ��ġ�� �����ϴ� Transform �Դϴ�. (������ �� ĳ���� ��ġ ���)")]
    [SerializeField] private Transform _groundCheckPoint;
    [Tooltip("���� ���� SphereCast�� �������Դϴ�.")]
    [SerializeField] private float _groundCheckRadius = 0.2f;
    [Tooltip("���� ���� SphereCast�� �����Դϴ�.")]
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [Tooltip("�������� �ν��� ���̾ �����մϴ�.")]
    [SerializeField] private LayerMask _groundLayer;

    #endregion

    #region ���� ���� ����
    public bool IsStop = false;

    // Rigidbody ������Ʈ(�� ��ũ��Ʈ�� �����ϴ� ���� ��ü)
    private Rigidbody _rb;

    // �Է°� �� ����
    private Vector2 _moveInput;            // �ֽ� �̵� �Է°� (x: ��/��, y: ��/��)
    private bool _isGrounded;              // ���� ���鿡 ����ִ��� ����
    private float _coyoteTimeCounter;      // �ڿ��� Ÿ�̸� ī����
    private float _jumpBufferCounter;      // ���� ���� Ÿ�̸� ī����

    private bool _isCollided = false;

    #endregion

    #region ����Ƽ ����������Ŭ

    // Awake: ������Ʈ �ʱ�ȭ, Rigidbody ���� ����
    private void Awake()
    {
        _isCollided = false;
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        // Enable interpolation so rendered transform stays smooth between physics updates
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Update: �Է�/���� Ÿ�̸� ������Ʈ
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Stage.StopStage();
        }

        if (IsStop) return;

        HandleState();
        HandleTimers();
    }

    // FixedUpdate: ���� ����(����, �̵�, �߷�) ó��
    private void FixedUpdate()
    {
        if (IsStop) return;

        HandleJump();
        HandleMovement();
        HandleGravity();
    }

    // �����Ϳ��� �� �Է� �� �ּҰ� ����
    private void OnValidate()
    {
        _jumpForce = Mathf.Max(0f, _jumpForce);
        _jumpGravityMultiplier = Mathf.Max(1f, _jumpGravityMultiplier);
        _fallMultiplier = Mathf.Max(1f, _fallMultiplier);
    }

    #endregion

    #region �Է� ó�� (PlayerInput���� ȣ��)

    // Move �׼� �ݹ�: �̵� �Է� ������Ʈ
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // Jump �׼� �ݹ�: performed ������ ���� ���� Ȱ��ȭ
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
    }

    #endregion

    #region ���� ���� �� Ÿ�̸�

    // ���� ����: SphereCast�� ����Ͽ� groundLayer�� �ش��ϴ� �ݶ��̴��� ����
    private void HandleState()
    {
        Transform groundCheckOrigin = _groundCheckPoint != null ? _groundCheckPoint : transform;
        _isGrounded = Physics.SphereCast(groundCheckOrigin.position, _groundCheckRadius, Vector3.down, out _, _groundCheckDistance, _groundLayer);
    }

    // �ڿ��� Ÿ�̸ӿ� ���� ���� ī��Ʈ ���� ó��
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

    #region ���� ó�� (����, �̵�, �߷�)

    // ���� ����: �ڿ��� �� ���� ������ ������ �� �� ���� ������ ����
    private void HandleJump()
    {
        if (_coyoteTimeCounter > 0f && _jumpBufferCounter > 0f)
        {
            // ���� �ӵ� �ʱ�ȭ �� ��� ��� �ӵ� �ο�
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
            _jumpBufferCounter = 0f;
        }
    }

    // �̵� ó��: ���� ���� �Է��� ����Ͽ� ��ǥ �ӵ��� �ε巴�� ����
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

        // �̵� ���� ���� ĳ���Ͱ� �̵� ������ �ٶ󺸵��� ȸ��
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // �߷� ����: ���߿����� ���ӵ� ����� �߷� ��ȭ ����
    private void HandleGravity()
    {
        if (!_isGrounded)
        {
            float verticalVelocity = _rb.linearVelocity.y;

            // �ϰ� �� �߷� ��ȭ
            if (verticalVelocity < 0)
            {
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            // ��� �� �߰� �߷� ����(�ε巯�� ��� ����)
            else if (verticalVelocity > 0)
            {
                _rb.linearVelocity += Vector3.up * Physics.gravity.y * (_jumpGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
    }

    private async void OnParticleCollision(GameObject goal)
    {
        if (_isCollided) return;

        _isCollided = true;

        if (goal.CompareTag("Hidden"))
        {
            Debug.Log("���� �� ����");
            if (!GameManager.Accomplishment.IsUnlocked((int)AchievementKey.HIDDEN))
            {
                await GameManager.Accomplishment.UnLock((int)AchievementKey.HIDDEN);

                if (UnitySceneManager.GetActiveScene().name != Scenes.START)
                {
                    GameManager.Scene.LoadScene(Scenes.START);

                    return;
                }
            }
        }

        if (goal.CompareTag("Goal"))
        {
            var currentStageName = UnitySceneManager.GetActiveScene().name;
            Debug.Log($"���� ���� ���� {currentStageName}");
            GameManager.Stage.ClearedStage(currentStageName);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "MirrorSectionStart")
        {
            Debug.Log("Mirror Section ����");
            MirrorObstacle[] mirrorObstacle = FindObjectsByType<MirrorObstacle>(FindObjectsSortMode.None);

            foreach(var mirror in mirrorObstacle)
            {
                mirror.IsStop = false;
            }
        }
    }

    #endregion
}

