using TMPro;
using UnityEngine;

public class AchievePopupUI : MonoBehaviour
{
    [SerializeField] private TMP_Text achieveText;

    public void SetText(string text)
    {
        achieveText.text = text;
    }
}
