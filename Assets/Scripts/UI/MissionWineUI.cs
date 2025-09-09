using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionWineUI : MonoBehaviour
{
    [SerializeField] private List<Image> missionIcons;

    public void SetMissionState(int idx, bool clear)
    {
        var targetImg = missionIcons[idx];

        if (targetImg != null)
        {
            if(clear) targetImg.color = Color.white;
            if(!clear) targetImg.color = new Color32(100, 100, 100, 255);
        }
    }
}
