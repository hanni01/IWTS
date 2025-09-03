using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;
    [SerializeField] private Button achievementBtn;
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private Graphic target;

    private float _speed = 1f;

    private void Awake()
    {
        achievementBtn.onClick.AddListener(() =>
        {
            achievementPanel.SetActive(true);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (achievementPanel.activeSelf)
            {
                achievementPanel.SetActive(false);
            }
            else
            {
                GameManager.Scene.LoadScene(Scenes.TUTORIAL);
            }
        }

        float t = Mathf.PingPong(Time.time * _speed, 1f);
        float a = Mathf.Lerp(30f / 200f, 1f, t);

        var c = target.color;
        c.a = a;
        target.color = c;
    }
}
