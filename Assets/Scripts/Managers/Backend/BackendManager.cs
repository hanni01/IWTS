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
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success

            CheckExistUser();

            // _uuid가 없다면, 회원가입이 안된 것으로 판단
            if (string.IsNullOrEmpty(_uuid))
            {
                // 회원가입 진행
                SignUp();
            }
            else
            {
                // _uuid가 있다면 회원이 존재함. 로그인 진행
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
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
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

        // pc 자동로그인용 ID 랜덤 생성, 로컬 저장
        if (File.Exists(savePath))
        {
            // 있다면 가져오기
            _uuid = File.ReadAllText(savePath);
            _pw = PlayerPrefs.GetString("USER_PW");
        }
    }

    private void SignUp()
    {
        // 없다면 계정 임의 생성
        _uuid = System.Guid.NewGuid().ToString();
        File.WriteAllText(savePath, _uuid);

        _pw = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString("USER_PW", _pw);

        BackendLogin.Instance.CustomSignUp(_uuid, _pw);

        // 닉네임 변경 UI 띄우고
        GameManager.UI.ShowPopup(UI_Key.NICKNAME);
    }

    public void RankInsert(int meter, string rowInDate = "")
    {
        if (string.IsNullOrEmpty(rowInDate))
        {
            var bro = Backend.GameData.GetMyData(TableName, new Where());
            if (!bro.IsSuccess()) { Debug.LogError("조회 실패"); return; }
            var rows = bro.FlattenRows();
            rowInDate = rows.Count > 0 ? rows[0]["inDate"].ToString() : string.Empty;

            if (rows.Count == 0)
            {
                var insertBro = Backend.GameData.Insert(TableName);
                if (!insertBro.IsSuccess()) { Debug.LogError("삽입 실패"); return; }
                rowInDate = insertBro.GetInDate();
            }
        }

        Param param = new Param();
        param.Add("totalMeter", meter);

        var rankBro = Backend.Leaderboard.User.UpdateMyDataAndRefreshLeaderboard(RankUUID, TableName, rowInDate, param);
        if (!rankBro.IsSuccess())
        {
            Debug.LogError("랭킹 등록 실패: " + rankBro.ToString());
            return;
        }

        Debug.Log("랭킹 갱신 성공");

        // 여기서 UI 띄움
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