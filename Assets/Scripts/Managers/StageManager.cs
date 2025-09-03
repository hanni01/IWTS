using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : IManager
{
    /// <summary>
    /// 통과한 스테이지 씬 이름을 담아두는 변수, 솔직히 필요없긴 한데 일단 넣음
    /// </summary>
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
    /// 스테이지 통과하였을때 호출하는 메서드
    /// </summary>
    /// <param name="stageName">스테이지 씬 이름</param>
    public void ClearedStage(string stageName)
    {
        Debug.Log($"{stageName} 클리어");
        clearedStages.Add(stageName);
        int currentStageIdx = _allStages.IndexOf(stageName);
        if(currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {
            string nextStageName = _allStages[currentStageIdx + 1];
            GameManager.Scene.LoadScene(nextStageName);
        }

        if(stageName == Scenes.FINAL)
        {
            FinalClear();
        }
    }

    public bool IsStageCleared(string stageName)
    {
        return clearedStages.Contains(stageName);
    }

    private void FinalClear()
    {
        //마지막 연출 이후 메인씬으로 이동
        GameManager.Scene.LoadScene(Scenes.START);
    }
}
