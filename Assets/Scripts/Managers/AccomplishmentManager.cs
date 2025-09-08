using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class AchievementState
{
    public int id;
    public bool unlocked;
}

[System.Serializable]
public class MissionState
{
    public string stageName = Scenes.NONE;
    public bool m1;
    public bool m2;
    public bool m3;
}

[System.Serializable]
public class AchievementSave
{
    public List<AchievementState> stateList = new List<AchievementState>();
}

[Serializable]
public class MissionSave
{
    public List<MissionState> missionStates = new List<MissionState>();
}

public class AccomplishmentManager : IManager
{

    private string fileName = "achievements.json";
    private string missionFileName = "missions.json";
    private string FilePath => Path.Combine(Application.persistentDataPath, fileName);
    private string missionFilePath => Path.Combine(Application.persistentDataPath, missionFileName);

    public Dictionary<int, AchievementState> States { get; private set; } = new();
    public Dictionary<int, AchievementDefinition> Defs { get; private set; } = new();
    public Dictionary<string, MissionState> MissionStates { get; private set; } = new();
    public Dictionary<string, StageMissionDefinition> MissionDefs { get; private set; } = new();

    public bool ActiveStopLight = false;
    public bool ActiveGuard = false;

    public void Initialize()
    {
        LoadDefinitionsFromResources();
        LoadOrInitStates();

        ActiveStopLight = PlayerPrefs.GetString("StopLight") == "true";
        ActiveGuard = PlayerPrefs.GetString("Guard") == "true";
    }

    public int TotalClearedMission()
    {
        if (MissionStates.Count <= 0) return -1;

        return MissionStates.Values.Sum(m =>
        (m.m1 ? 1 : 0) +
        (m.m2 ? 1 : 0) +
        (m.m3 ? 1 : 0)
        );
    }

    private void LoadDefinitionsFromResources()
    {
        Defs.Clear();
        var defs = Resources.LoadAll<AchievementDefinition>("Achievements");
        foreach (var def in defs)
        {
            if (Defs.ContainsKey(def.id))
            {
                Debug.LogWarning("업적 중복 ID");
                continue;
            }
            Defs[def.id] = def;
        }
        Debug.Log("업적 에셋 로드 완료");

        // =========================================================================

        MissionDefs.Clear();
        var missionDefs = Resources.LoadAll<StageMissionDefinition>("Missions");
        foreach (var def in missionDefs)
        {
            if (MissionDefs.ContainsKey(def.StageName))
            {
                Debug.LogWarning("미션 스테이지 중복 이름");
                continue;
            }
            MissionDefs[def.StageName] = def;
        }
        Debug.Log("미션 스테이지 에셋 로드 완료");
    }

    private void LoadOrInitStates()
    {
        States.Clear();
        MissionStates.Clear();

        AchievementSave save = null;
        MissionSave missionSave = null;

        // 업적 파일 찾기
        if(File.Exists(FilePath))
        {
            try
            {
                var json = File.ReadAllText(FilePath);
                save = JsonUtility.FromJson<AchievementSave>(json);
            }catch(System.Exception e)
            {
                Debug.LogError($"업적 저장 파일 파싱 실패, 새로 만듭니다...{e}");
            }
        }

        // 미션 파일 찾기
        if (File.Exists(missionFilePath))
        {
            try
            {
                var json = File.ReadAllText(missionFilePath);
                missionSave = JsonUtility.FromJson<MissionSave>(json);
            }catch(System.Exception e)
            {
                Debug.LogError($"미션 저장 파일 파싱 실패, 새로 만듭니다...{e}");
            }
        }

        // 업적 상태 저장
        if (save == null) save = new AchievementSave();

        foreach(var s in save.stateList)
        {
            States[s.id] = s;
        }

        foreach(var id in Defs.Keys)
        {
            if (!States.ContainsKey(id))
            {
                States[id] = new AchievementState { id = id, unlocked = false };
            }
        }

        if(missionSave == null) missionSave = new MissionSave();

        foreach(var s in missionSave.missionStates)
        {
            MissionStates[s.stageName] = s;
        }

        foreach(var name in MissionDefs.Keys)
        {
            if (!MissionStates.ContainsKey(name))
            {
                MissionStates[name] = new MissionState { stageName = name, m1 = false, m2 = false, m3 = false };
            }
        }

        AccomplishmentSave();
        MissionUpdateSave();
    }

    private void AccomplishmentSave()
    {
        var save = new AchievementSave { stateList = new List<AchievementState>(States.Values) };
        var json = JsonUtility.ToJson(save, true);
        File.WriteAllText(FilePath, json);
    }

    private void MissionUpdateSave()
    {
        var saveMission = new MissionSave { missionStates = new List<MissionState>(MissionStates.Values) };
        var jsonMission = JsonUtility.ToJson(saveMission, true);
        File.WriteAllText(missionFilePath, jsonMission);
    }

    public void MissionUpdate(string name, bool mission1, bool mission2, bool mission3)
    {
        if(!MissionDefs.ContainsKey(name)) return;

        if(!MissionStates.TryGetValue(name, out var mState))
        {
            mState = new MissionState { stageName = name, m1 = mission1, m2 = mission2, m3 = mission3 };
            MissionStates[name] = mState;
        }

        if(!mState.m1) mState.m1 = mission1;
        if(!mState.m2) mState.m2 = mission2;
        if(!mState.m3) mState.m3 = mission3;

        MissionUpdateSave();
    }

    public async UniTask UnLock(int id)
    {
        if (!Defs.ContainsKey(id)) return;

        if (!States.TryGetValue(id, out var state))
        {
            state = new AchievementState { id = id, unlocked = false };
            States[id] = state;
        }

        if (!state.unlocked)
        {
            state.unlocked = true;
            AccomplishmentSave();
            await OnUnlocked(Defs[id].achievementTitle);
        }
    }

    private async UniTask OnUnlocked(string text)
    {
        var instance = GameManager.UI.ShowPopup(UI_Key.ACHIEVEMENT);
        instance.GetComponent<AchievePopupUI>().SetText(text);
        Object.DontDestroyOnLoad(instance);
        instance.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(3f), DelayType.UnscaledDeltaTime);

        if(instance != null)
        {
            Object.Destroy(instance);
        }
    }

    public bool IsUnlocked(int id) => States.TryGetValue(id, out var state) && state.unlocked;

    public bool IsClearMission(string StageName, int id)
    {
        MissionStates.TryGetValue(StageName, out var state);

        switch (id) { case 0: return state.m1; case 1: return state.m2; case 2: return state.m3; default: return false; }
    }

    public void Release()
    {
        
    }
}
