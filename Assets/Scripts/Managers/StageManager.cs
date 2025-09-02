using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : IManager
{
    private static readonly HashSet<string> clearedStages = new();

    public List<string> _allStages;

    public void Initialize()
    {
        _allStages = new List<string>
        {
            Scenes.STEP1,
            Scenes.STEP2,
        };
    }

    public void Release()
    {
        clearedStages.Clear();
    }

    public void ClearedStage(string stageName)
    {
        clearedStages.Add(stageName);
        int currentStageIdx = _allStages.IndexOf(stageName);
        if(currentStageIdx != -1 && currentStageIdx + 1 < _allStages.Count)
        {

        }
    }

    public bool IsStageCleared(string stageName)
    {
        return clearedStages.Contains(stageName);
    }

    public bool AreAllStagesCleared()
    {
        return _allStages.All(stage => clearedStages.Contains(stage));
    }
}
