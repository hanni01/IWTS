using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementListUI : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> achieve_text;
    [SerializeField] private List<Image> achieve_Image;

    void Start()
    {
        for(int i = 1;i <= GameManager.Accomplishment.States.Count;i++)
        {
            GameManager.Accomplishment.Defs.TryGetValue(i, out var def);
            GameManager.Accomplishment.States.TryGetValue(i, out var state);
            achieve_text[i - 1].text = def.achievementTitle;
            if (state.unlocked)
            {
                achieve_text[i - 1].fontStyle = FontStyles.Strikethrough;
                achieve_Image[i - 1].color = Color.green;
            }
        }
    }
}
