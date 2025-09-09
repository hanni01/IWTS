using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class StageManager : IManager
{
    private static readonly HashSet<string> clearedStages = new();
    public List<string> _allStages;

    public void Initialize()
    {
        _allStages = new List<string>
        {
            Scenes.TUTORIAL,
            Scenes.STEP1,
            Scenes.FINAL,
        };
    }

    public void Release()
    {
        clearedStages.Clear();
    }

    /// <summary>
    /// �������� �÷��� �� ESC�� ������ �� �κ������ ����������
    /// </summary>
    public void StopStage()
    {
        if (GameManager.UI.GetActivePopupUI(UI_Key.RANK))
        {
            GameManager.UI.HidePopup(UI_Key.RANK);
        }
        GameManager.Scene.LoadScene(Scenes.LOBBY);
    }

    public void ContinueClearStage(string stageName)
    {
        Debug.Log($"{stageName} Ŭ����");
        clearedStages.Add(stageName);

        int currentStageIdx = _allStages.IndexOf(stageName);

        // Ʃ�丮�� ���� ��� ���������� Ŭ����� �κ������

        //���� ���������� �ִ� ���
        if (currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {
            string nextStageName = _allStages[currentStageIdx + 1];

            // �� ��ȯ�� ������ �� ����
            if (UnitySceneManager.GetActiveScene().name != nextStageName)
            {
                GameManager.Scene.LoadScene(nextStageName);
            }
        }
    }

    /// <summary>
    /// �������� Ŭ���� �� ȣ��
    /// </summary>
    public async void ClearedStage(string stageName)
    {
        Debug.Log($"{stageName} Ŭ����");
        clearedStages.Add(stageName);

        // Ʃ�丮�� ���� ��� ���������� Ŭ����� �κ������

        // ���� ���� ����
        if(stageName != Scenes.TUTORIAL)
        {
            var ending = Object.FindFirstObjectByType<EndingEffect>();
            if (ending != null)
            {
                ending.PlayEnding();

                // ���� ���� ������ ���
                await UniTask.Delay(4000);
            }
        }

        CheckMissionClear(stageName);

        // �� ��ȯ�� ������ �� ����
        if (UnitySceneManager.GetActiveScene().name != Scenes.LOBBY)
        {
            GameManager.Scene.LoadScene(Scenes.LOBBY);
        }
    }

    public bool IsStageCleared(string stageName) => clearedStages.Contains(stageName);

    public void CheckMissionClear(string stageName)
    {
        if (stageName == Scenes.TUTORIAL) return;

        string currentScene = UnitySceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(currentScene) && currentScene == stageName)
        {
            var missionTracker = Object.FindAnyObjectByType<StageMissionTracker>();
            var hpSystem = Object.FindAnyObjectByType<HealthSystem>();

            if(missionTracker != null && hpSystem != null)
            {
                missionTracker.NotifyGoalReached(hpSystem.currentHealth);
            }
        }
    }
}
