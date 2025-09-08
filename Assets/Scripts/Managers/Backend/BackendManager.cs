using BackEnd;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class BackendManager : IManager
{
    private string _uuid;
    private string _pw;
    private string savePath;

    public string RankUUID { get; private set; } = "019924b6-2f98-7d1c-9748-508ac214231c";
    public string TableName { get; private set; } = "USER_DATA";

    public void Initialize()
    {
        var bro = Backend.Initialize(); // �ڳ� �ʱ�ȭ

        // �ڳ� �ʱ�ȭ�� ���� ���䰪
        if (bro.IsSuccess())
        {
            Debug.Log("�ʱ�ȭ ���� : " + bro); // ������ ��� statusCode 204 Success

            CheckExistUser();

            // _uuid�� ���ٸ�, ȸ�������� �ȵ� ������ �Ǵ�
            if (string.IsNullOrEmpty(_uuid))
            {
                // ȸ������ ����
                SignUp();
            }
            else
            {
                // _uuid�� �ִٸ� ȸ���� ������. �α��� ����
                BackendLogin.Instance.CustomLogin(_uuid, _pw);

                GameManager.GameData.GameDataGet();

                if(GameManager.GameData.userData == null)
                {
                    GameManager.GameData.GameDataInsert();
                }
            }
        }
        else
        {
            Debug.LogError("�ʱ�ȭ ���� : " + bro); // ������ ��� statusCode 400�� ���� �߻�
        }
    }

    public void Release()
    {
        
    }

    public string GetUserNickname()
    {
        var bro = Backend.BMember.GetUserInfo();

        if(bro.IsSuccess())
        {
            string nickname = bro.GetReturnValuetoJSON()["row"]["nickname"].ToString();

            if (!string.IsNullOrEmpty(nickname))
            {
                return nickname;
            }
        }

        return string.Empty;
    }

    private void CheckExistUser()
    {
        savePath = Path.Combine(Application.persistentDataPath, "uuid.txt");

        // pc �ڵ��α��ο� ID ���� ����, ���� ����
        if (File.Exists(savePath))
        {
            // �ִٸ� ��������
            _uuid = File.ReadAllText(savePath);
            _pw = PlayerPrefs.GetString("USER_PW");
        }
    }

    private void SignUp()
    {
        // ���ٸ� ���� ���� ����
        _uuid = System.Guid.NewGuid().ToString();
        File.WriteAllText(savePath, _uuid);

        _pw = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString("USER_PW", _pw);

        BackendLogin.Instance.CustomSignUp(_uuid, _pw);

        // �г��� ���� UI ����
        GameManager.UI.ShowPopup(UI_Key.NICKNAME);
    }

    public void RankInsert(int meter, string rowInDate = "")
    {
        if (string.IsNullOrEmpty(rowInDate))
        {
            var bro = Backend.GameData.GetMyData(TableName, new Where());
            if (!bro.IsSuccess()) { Debug.LogError("��ȸ ����"); return; }
            var rows = bro.FlattenRows();
            rowInDate = rows.Count > 0 ? rows[0]["inDate"].ToString() : string.Empty;

            if (rows.Count == 0)
            {
                var insertBro = Backend.GameData.Insert(TableName);
                if (!insertBro.IsSuccess()) { Debug.LogError("���� ����"); return; }
                rowInDate = insertBro.GetInDate();
            }
        }

        Param param = new Param();
        param.Add("totalMeter", meter);

        var rankBro = Backend.Leaderboard.User.UpdateMyDataAndRefreshLeaderboard(RankUUID, TableName, rowInDate, param);
        if (!rankBro.IsSuccess())
        {
            Debug.LogError("��ŷ ��� ����: " + rankBro.ToString());
            return;
        }

        Debug.Log("��ŷ ���� ����");

        // ���⼭ UI ���
        var rankUI = GameManager.UI.ShowPopupFalse(UI_Key.RANK);
        rankUI.GetComponent<RankUI>().SetAndShowRankUI();
    }



    public void RankGet(int count, Action<List<(string nickname, string meter)>> callback)
    {
        Backend.Leaderboard.User.GetLeaderboard(RankUUID, count, bro =>
        {
            var result = new List<(string nickname, string meter)>();
            if (!bro.IsSuccess()) { callback(result); return; }

            foreach (var item in bro.GetUserLeaderboardList())
            {
                result.Add((item.nickname, item.score.ToString()));
            }

            callback(result);
        });
    }

    public void GetMyRankInfo(Action<(string rank, string meter)> callback)
    {
        (string, string) info = (string.Empty, string.Empty);
        Backend.Leaderboard.User.GetMyLeaderboard(RankUUID, bro =>
        {
            if (bro.IsSuccess() == false) return;

            foreach (var item in bro.GetUserLeaderboardList())
            {
                info =  (item.rank, item.score);
            }

            callback(info);
        });
    }

}