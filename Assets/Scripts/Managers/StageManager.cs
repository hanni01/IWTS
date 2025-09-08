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
    /// 스테이지 플레이 중 ESC를 눌렀을 때 로비씬으로 빠져나오기
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
        Debug.Log($"{stageName} 클리어");
        clearedStages.Add(stageName);

        int currentStageIdx = _allStages.IndexOf(stageName);

        // 튜토리얼 포함 모든 스테이지는 클리어시 로비씬으로

        //다음 스테이지가 있는 경우
        if (currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {
            string nextStageName = _allStages[currentStageIdx + 1];

            CheckAccomplishment(stageName);

            // 씬 전환은 무조건 한 번만
            if (UnitySceneManager.GetActiveScene().name != nextStageName)
            {
                GameManager.Scene.LoadScene(nextStageName);
            }
        }
    }

    /// <summary>
    /// 스테이지 클리어 시 호출
    /// </summary>
    public async void ClearedStage(string stageName)
    {
        Debug.Log($"{stageName} 클리어");
        clearedStages.Add(stageName);

        // 튜토리얼 포함 모든 스테이지는 클리어시 로비씬으로

        // 엔딩 연출 실행
        if(stageName != Scenes.TUTORIAL)
        {
            var ending = Object.FindFirstObjectByType<EndingEffect>();
            if (ending != null)
            {
                ending.PlayEnding();

                // 연출 끝날 때까지 대기
                await UniTask.Delay(4000);
            }
        }

        CheckAccomplishment(stageName);
        CheckMissionClear(stageName);

        // 씬 전환은 무조건 한 번만
        if (UnitySceneManager.GetActiveScene().name != Scenes.LOBBY)
        {
            GameManager.Scene.LoadScene(Scenes.LOBBY);
        }
    }

    public bool IsStageCleared(string stageName) => clearedStages.Contains(stageName);

    public async void CheckAccomplishment(string stageName = null)
    {
        if (stageName != null && stageName == Scenes.TUTORIAL) return;

        var player = GameObject.Find("FinalPlayer");
        var hs = player?.GetComponent<HealthSystem>();

        // 업적 조건 체크
        if (hs != null 
            && hs.currentHealth > 70
            && stageName == Scenes.STEP1 
            && !GameManager.Accomplishment.IsUnlocked((int)AchievementKey.POTENTIAL))
        {
            // 업적 달성 팝업 끝까지 기다린 후 다음 씬 로드
            await GameManager.Accomplishment.UnLock((int)AchievementKey.POTENTIAL);
        }

        // ALIVE 업적
        if (!GameManager.Accomplishment.IsUnlocked((int)AchievementKey.ALIVE) && stageName == Scenes.FINAL)
        {
            await GameManager.Accomplishment.UnLock((int)AchievementKey.ALIVE);
        }

        if (hs != null 
            && hs.currentHealth > 50 
            && !GameManager.Accomplishment.IsUnlocked((int)AchievementKey.STRONGER) && stageName == Scenes.FINAL)
        {
            await GameManager.Accomplishment.UnLock((int)AchievementKey.STRONGER);
        }
    }

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
