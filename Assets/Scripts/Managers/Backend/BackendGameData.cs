using UnityEngine;
using BackEnd;

public class UserData
{
    public int totalMeter = 0;
}
public class BackendGameData : IManager
{
    public UserData userData;

    private string gameDataRowInDate = string.Empty;

    public void Initialize()
    {
        
    }

    public void Release()
    {
        
    }

    public void GameDataInsert()
    {
        // 게임 정보 삽입 구현
        if(userData == null)
        {
            userData = new UserData();
        }

        Debug.Log("유저 데이터 초기화");
        userData.totalMeter = 0;

        Debug.Log("뒤끝 업데이트 목록에 데이터 추가");
        Param param = new Param();
        param.Add("totalMeter", userData.totalMeter);

        var bro = Backend.GameData.Insert(GameManager.Backend.TableName, param);

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 데이터 삽입에 성공했습니다.");

            gameDataRowInDate = bro.GetInDate();
        }
        else
        {
            Debug.LogError("게임 정보 데이터 삽입에 실패했습니다: " + bro);
        }
    }

    public void GameDataGet()
    {
        // 게임 정보 불러오기 구현
        Debug.Log("게임 정보 조회 함수를 호출");

        var bro = Backend.GameData.GetMyData(GameManager.Backend.TableName, new Where());

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 조회에 성공 : " + bro);


            LitJson.JsonData gameDataJson = bro.FlattenRows();

            if (gameDataJson.Count <= 0)
            {
                Debug.LogWarning("데이터가 존재하지 않음");
            }
            else
            {
                gameDataRowInDate = gameDataJson[0]["inDate"].ToString();

                userData = new UserData();

                userData.totalMeter = int.Parse(gameDataJson[0]["totalMeter"].ToString());

                Debug.Log(userData.ToString());
            }
        }
        else
        {
            Debug.LogError("게임 정보 조회에 실패 : " + bro);
        }
    }

    public void GameDataUpdate()
    {
        // 게임 정보 수정 구현
        if (userData == null)
        {
            Debug.LogError("업데이트할 데이터가 없습니다. Insert 혹은 Get을 통해 호출해주세요");
            return;
        }

        Param param = new Param();
        param.Add("totalMeter", userData.totalMeter);

        BackendReturnObject bro = string.IsNullOrEmpty(gameDataRowInDate)
            ? Backend.GameData.Update(GameManager.Backend.TableName, new Where(), param)
            : Backend.GameData.UpdateV2(GameManager.Backend.TableName, gameDataRowInDate, Backend.UserInDate, param);

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 데이터 수정 성공 : " + bro);
        }
        else
        {
            Debug.LogError("게임 정보 데이터 수정 실패 : " + bro);
        }
    }

    public void UpdateRecordAndRank(int meter)
    {
        GameDataGet();

        bool isNewRecord = false;

        if(userData == null)
        {
            GameDataInsert();
            userData.totalMeter = meter;
            isNewRecord = true;
        }
        else if (meter > userData.totalMeter)
        {
            userData.totalMeter = meter;
            isNewRecord = true;
        }
        else
        {
            Debug.Log("기존 기록이 더 높거나 같아서 갱신하지 않음");
            var rankUI = GameManager.UI.ShowPopupFalse(UI_Key.RANK);
            rankUI.GetComponent<RankUI>().SetAndShowRankUI();
        }

        if (isNewRecord)
        {
            GameDataUpdate();

            GameManager.Backend.RankInsert(userData.totalMeter, gameDataRowInDate);
        }
    }
}
