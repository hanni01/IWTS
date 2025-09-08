using UnityEngine;
using UnityEngine.UI;

public class QuitPopupUI : MonoBehaviour
{
    [SerializeField] private Button CheckBtn;
    [SerializeField] private Button CloseBtn;

    private void Awake()
    {
        CheckBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        CloseBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
