using UnityEngine;

// 중복 방지, DontDestroyOnLoad, 안전 해제 포함 베이스
public abstract class MonoManager<T> : MonoBehaviour where T : MonoManager<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        // 이미 살아있는 인스턴스가 있으면 자신을 제거(중복 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
        DontDestroyOnLoad(gameObject);
        OnManagerAwake();
    }
    
    // 중복 시 해제
    protected virtual void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    // 파생 클래스에서 초기화 코드
    protected virtual void OnManagerAwake()
    {

    }
}
