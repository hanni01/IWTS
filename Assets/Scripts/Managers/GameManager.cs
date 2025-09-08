using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static SceneManager Scene { get; } = new();
    public static StageManager Stage { get; } = new();
    public static UIManager UI { get; } = new(); 
    public static BackendManager Backend { get; } = new();
    public static BackendGameData GameData { get; } = new();
    public static AccomplishmentManager Accomplishment { get; } = new();

    private bool _isDestroyManager;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    public void OnApplicationQuit()
    {
        DestroyManager();
    }

    public static void Initialize()
    {
        var managers = new List<IManager>
        {
            Scene,
            Stage,
            Accomplishment,
            UI,
            GameData,
        };

        foreach (var manager in managers)
        {
            manager.Initialize();
        }
    }

    private void DestroyManager()
    {
        if (_isDestroyManager) return;

        _isDestroyManager = true;
        Release();
    }

    private void Release()
    {
        var managers = new List<IManager>
        {
            Scene,
            Stage,
            Accomplishment,
            UI,
            Backend,
            GameData
        };

        foreach(var manager in managers)
        {
            manager.Release();
        }
    }
}
