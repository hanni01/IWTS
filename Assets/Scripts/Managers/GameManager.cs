using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static SceneManager Scene { get; } = new();
    public static StageManager Stage { get; } = new();

    private bool _isDestroyManager;

    private void Awake()
    {
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
        };

        foreach(var manager in managers)
        {
            manager.Release();
        }
    }
}
