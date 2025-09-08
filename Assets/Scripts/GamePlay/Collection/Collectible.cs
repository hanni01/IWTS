using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string stageName = Scenes.NONE;
    public string id;

    public static event Action<Collectible> OnCollected; // ���� �̺�Ʈ

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "FinalPlayer") return;
        OnCollected?.Invoke(this);
        gameObject.SetActive(false);
    }
}
