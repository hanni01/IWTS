using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;
    [SerializeField] private Graphic target;

    private float _speed = 1f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Backend.Initialize();
        }

        float t = Mathf.PingPong(Time.time * _speed, 1f);
        float a = Mathf.Lerp(30f / 200f, 1f, t);

        var c = target.color;
        c.a = a;
        target.color = c;
    }
}
