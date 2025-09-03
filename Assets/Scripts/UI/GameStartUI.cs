using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;
    [SerializeField] private Graphic target;
    private float _speed = 1f;

    private void Awake()
    {
        // 우선 클릭으로도 시작할 수 있게 둔다.
        gameStartBtn.onClick.AddListener(() =>
        {
            GameManager.Scene.LoadScene(Scenes.TEST);
        });
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            GameManager.Scene.LoadScene(Scenes.TEST);
        }

        float t = Mathf.PingPong(Time.time * _speed, 1f);
        float a = Mathf.Lerp(30f / 200f, 1f, t);

        var c = target.color;
        c.a = a;
        target.color = c;
    }
}
