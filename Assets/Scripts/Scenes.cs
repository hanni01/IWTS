using UnityEngine;

public static class Scenes
{
    public const string NONE = "None";
    public const string LOAD = "00_LoadingScene";
    public const string START = "01_StartScene";
    public const string LOBBY = "02_LobbyScene";
    public const string TUTORIAL = "03_TutorialScene";
    public const string STEP1 = "04_Step1Scene";
    public const string STEP2 = "05_Step2Scene";
    public const string STEP3 = "06_Step3Scene";
    public const string FINAL = "07_FinalStepScene";
}

public enum AchievementKey
{
    NONE = 0,
    ALIVE = 1,
    POTENTIAL = 2,
    STRONGER = 3,
    HIDDEN = 4,
}

public static class UI_Key
{
    public const string ACHIEVEMENT = "accomplishmentPanel";
    public const string NICKNAME = "NickNamePanel";
    public const string RANK = "RankPanel";
    public const string QUIT = "QuitPanel";
}