using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public bool IsDead => currentHealth <= 0f;
    public UnityEngine.Events.UnityEvent onDamaged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"체력: {currentHealth}");

        // 이벤트 호출 지점
        onDamaged?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("사망! 스테이지 처음으로 돌아갑니다.");
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); // 지금 플레이한 씬 처음 불러오기
    }

    public void RestoreFull()
    {
        currentHealth = maxHealth;
    }
}
