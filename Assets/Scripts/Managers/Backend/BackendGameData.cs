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
        // ���� ���� ���� ����
        if(userData == null)
        {
            userData = new UserData();
        }

        Debug.Log("���� ������ �ʱ�ȭ");
        userData.totalMeter = 0;

        Debug.Log("�ڳ� ������Ʈ ��Ͽ� ������ �߰�");
        Param param = new Param();
        param.Add("totalMeter", userData.totalMeter);

        var bro = Backend.GameData.Insert(GameManager.Backend.TableName, param);

        if (bro.IsSuccess())
        {
            Debug.Log("���� ���� ������ ���Կ� �����߽��ϴ�.");

            gameDataRowInDate = bro.GetInDate();
        }
        else
        {
            Debug.LogError("���� ���� ������ ���Կ� �����߽��ϴ�: " + bro);
        }
    }

    public void GameDataGet()
    {
        // ���� ���� �ҷ����� ����
        Debug.Log("���� ���� ��ȸ �Լ��� ȣ��");

        var bro = Backend.GameData.GetMyData(GameManager.Backend.TableName, new Where());

        if (bro.IsSuccess())
        {
            Debug.Log("���� ���� ��ȸ�� ���� : " + bro);


            LitJson.JsonData gameDataJson = bro.FlattenRows();

            if (gameDataJson.Count <= 0)
            {
                Debug.LogWarning("�����Ͱ� �������� ����");
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
            Debug.LogError("���� ���� ��ȸ�� ���� : " + bro);
        }
    }

    public void GameDataUpdate()
    {
        // ���� ���� ���� ����
        if (userData == null)
        {
            Debug.LogError("������Ʈ�� �����Ͱ� �����ϴ�. Insert Ȥ�� Get�� ���� ȣ�����ּ���");
            return;
        }

        Param param = new Param();
        param.Add("totalMeter", userData.totalMeter);

        BackendReturnObject bro = string.IsNullOrEmpty(gameDataRowInDate)
            ? Backend.GameData.Update(GameManager.Backend.TableName, new Where(), param)
            : Backend.GameData.UpdateV2(GameManager.Backend.TableName, gameDataRowInDate, Backend.UserInDate, param);

        if (bro.IsSuccess())
        {
            Debug.Log("���� ���� ������ ���� ���� : " + bro);
        }
        else
        {
            Debug.LogError("���� ���� ������ ���� ���� : " + bro);
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
            Debug.Log("���� ����� �� ���ų� ���Ƽ� �������� ����");
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
