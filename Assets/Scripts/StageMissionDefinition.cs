using UnityEngine;

[CreateAssetMenu(fileName = "Mission_", menuName = "Game/Stage Mission Definition")]
public class StageMissionDefinition : ScriptableObject
{
    public string StageName;

    public bool m1 = true;
    public bool m2 = false;
    public bool m3 = false;
}
