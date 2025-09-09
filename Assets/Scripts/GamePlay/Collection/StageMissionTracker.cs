using System;
using UnityEngine;

public class StageMissionTracker : MonoBehaviour
{
    [SerializeField] private string StageName;
    [SerializeField] private MissionWineUI MissionWineView;

    private bool noDeath = true;
    private bool allCollected = false;
    private bool hp50up = true;

    private bool alreadyClearNoDeath = false;
    private bool alreadyClearHp50Up = false;
    private bool alreadyClearAllCollected = false;

    public static event Action<string> OnWineAllCollected; //StageName��

    public string Stage => StageName;

    private void Awake()
    {
        if(!GameManager.Accomplishment.MissionDefs.TryGetValue(StageName, out var missionDefinition))
        {
            Debug.LogError($"Stage {StageName} �̸��� �̼� ������ �������� �ʽ��ϴ�.");
            return;
        }

        GameManager.Accomplishment.MissionStates.TryGetValue(StageName, out var missionState);

        alreadyClearNoDeath = missionState.m1 ? true : false;
        alreadyClearAllCollected = missionState.m2 ? true : false;
        alreadyClearHp50Up = missionState.m3 ? true : false;

        allCollected = missionState.m2 ? true : false;

        if(PlayerPrefs.GetString("Dead") == "true") noDeath = false;

        MissionWineView.SetMissionState(0, noDeath);
        MissionWineView.SetMissionState(1, alreadyClearAllCollected);
        MissionWineView.SetMissionState(2, hp50up);
    }


    /// <summary>
    /// �׾��� �� ȣ��
    /// </summary>
    public void NotifyPlayerDied()
    {
        if(!alreadyClearNoDeath)
        {
            noDeath = false;
            MissionWineView.SetMissionState(0, noDeath);

            PlayerPrefs.SetString("Dead", "true");
        }
    }

    /// <summary>
    /// �÷��̾� HP�� 50���ϰ� �Ǹ� �ٷ� false
    /// </summary>
    public void NotifyPlayerHp50Down()
    {
        if(!alreadyClearHp50Up)
        {
            hp50up = false;
            MissionWineView.SetMissionState(2, hp50up);
        }
    }

    /// <summary>
    /// �� ������ ���������� ȣ��
    /// ���� ü���� Ȯ���Ͽ� �̼� Ŭ���� ���¸� ������Ʈ�Ѵ�.
    /// ��, �̹� �� �� Ŭ������ ������ ��� �״�� �����ȴ�.
    /// </summary>
    /// <param name="currentHP"></param>
    public void NotifyGoalReached(float currentHP)
    {
        if(!alreadyClearHp50Up) hp50up = currentHP >= 50;
        EvaluateAndSave();
    }

    private void OnEnable()
    {
        OnWineAllCollected += HandleWineAll; 
    }

    private void OnDisable()
    {
        OnWineAllCollected -= HandleWineAll;
    }

    /// <summary>
    /// �������� �����, ��� ������ �����Ǿ����� ȣ��
    /// </summary>
    /// <param name="stageName"></param>
    public static void RaiseWineCollectedAll(string stageName)
        => OnWineAllCollected?.Invoke(stageName);

    private void HandleWineAll(string stageName)
    {
        if (stageName != this.StageName) return;

        Debug.Log($"�������� {stageName}�� ������ ��� ��ҽ��ϴ�.");
        allCollected = true;
        MissionWineView.SetMissionState(1, allCollected);
    }

    private void EvaluateAndSave()
    {
        bool m1 = noDeath;
        bool m2 = allCollected;
        bool m3 = hp50up;

        Debug.Log($"mission1: {m1}, mission2: {m2}, mission3: {m3}");

        GameManager.Accomplishment.MissionUpdate(StageName, m1, m2, m3);
    }
}
