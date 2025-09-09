using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NickNamePopupUI : MonoBehaviour
{
    [SerializeField] private Button NickNameBtn;
    [SerializeField] private TMP_InputField NickNameInputField;

    private void Start()
    {
        NickNameBtn.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(NickNameInputField.text))
            {
                NickNameInputField.placeholder.GetComponent<Text>().text = "닉네임을 입력해주세요";

                return;
            }
            else
            {
                BackendLogin.Instance.UpdateNickname(NickNameInputField.text);

                GameManager.UI.HidePopup(UI_Key.NICKNAME);

                GameManager.Scene.LoadScene(Scenes.TUTORIAL);
            }
        });
    }
}
