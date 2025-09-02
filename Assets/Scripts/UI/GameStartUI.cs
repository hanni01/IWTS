using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartBtn;
    [SerializeField] private Button gameLoadBtn;

    private void Awake()
    {
        gameStartBtn.onClick.AddListener(() =>
        {
            GameManager.Scene.LoadScene(Scenes.TUTORIAL);
        });
    }
}
