using BackEnd;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> RankTextPanel;
    [SerializeField] private GameObject MyRankPanel;

    public void SetAndShowRankUI()
    {
        GameManager.Backend.RankGet(4, rank =>
        {
            for (int i = 0; i < rank.Count; i++)
            {
                var nickname = rank[i].nickname;
                var meter = rank[i].meter;
                if (string.IsNullOrEmpty(rank[i].nickname) || string.IsNullOrEmpty(rank[i].meter))
                {
                    nickname = string.Empty;
                    meter = string.Empty;
                }
                RankTextPanel[i].GetComponentsInChildren<TMP_Text>()[1].text = nickname;
                RankTextPanel[i].GetComponentsInChildren<TMP_Text>()[2].text = meter;
            }

            var myNickname = GameManager.Backend.GetUserNickname();
            if (!rank.Any(r => r.nickname == myNickname))
            {
                GameManager.Backend.GetMyRankInfo(info =>
                {
                    MyRankPanel.GetComponentsInChildren<TMP_Text>()[0].text = info.rank;
                    MyRankPanel.GetComponentsInChildren<TMP_Text>()[1].text = myNickname;
                    MyRankPanel.GetComponentsInChildren<TMP_Text>()[2].text = info.meter;
                });
            }

            this.gameObject.SetActive(true);
        });
    }
}
