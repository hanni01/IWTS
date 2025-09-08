using System;
using UnityEngine;

public class MissionTrackerManager : IManager
{
    public int totalClearedMission { get; private set; }

    public event Action<string> OnWineAllCollected;

    public void Initialize()
    {
        
    }

    public void Release()
    {
        
    }
}
