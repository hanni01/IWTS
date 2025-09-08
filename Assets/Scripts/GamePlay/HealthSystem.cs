using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }
    public bool IsStop { get; set; } = false;

    public bool IsDead => currentHealth <= 0f;
    public UnityEngine.Events.UnityEvent onDamaged;
    public Renderer playerRenderer;

    private bool _isNotified = false;

    [SerializeField] private StageMissionTracker missionTracker;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (IsStop) return;

        ChangePlayerColor(playerRenderer);

        if(currentHealth < 50f)
        {
            if(!_isNotified)
            {
                if (missionTracker == null) return; 
                missionTracker.NotifyPlayerHp50Down();
                _isNotified = true;
            }
        }
    }

    public void ApplyDamage(float amount)
    {
        var ending = FindFirstObjectByType<EndingEffect>();
        if (ending != null && ending.isPlaying) return;

        currentHealth -= amount;
        Debug.Log($"ü��: {currentHealth}");

        // �̺�Ʈ ȣ�� ����
        onDamaged?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("���! �������� ó������ ���ư��ϴ�.");

        if (missionTracker == null) return;

        missionTracker.NotifyPlayerDied();

        UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); // ���� �÷����� �� ó�� �ҷ�����
    }

    public void RestoreFull()
    {
        currentHealth = maxHealth;
    }

    public void ChangePlayerColor(Renderer playerRenderer)
    {
        if (playerRenderer == null) return;

        var ending = FindFirstObjectByType<EndingEffect>();
        if (ending != null && ending.isPlaying) return;


        // ü���� 0 �Ʒ��ε� �� �� �����Ƿ� Clamp
        float clampedHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // ü�� �����
        float healthPercent = clampedHealth / maxHealth; // 1.0 ~ 0.0

        // R�� �׻� 255 ����, G/B�� �ۼ�Ʈ��ŭ ���� (255 �� 0)
        float r = 255f;
        float g = 255f * healthPercent;
        float b = 255f * healthPercent;

        // 0~255 �� 0~1 ��ȯ
        Color newColor = new Color(r / 255f, g / 255f, b / 255f, 1f);

        playerRenderer.material.color = newColor;
    }

}
