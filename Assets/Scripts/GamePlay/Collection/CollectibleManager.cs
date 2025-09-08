using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [SerializeField] private string stageName;
    [SerializeField] private CollectionUI collectionView;

    private const int totalWine = 3;
    private int collectedWine;

    private void OnEnable()
    {
        Collectible.OnCollected += HandleCollected;
    }

    private void OnDisable()
    {
        Collectible.OnCollected -= HandleCollected;
    }

    private void Start()
    {
        collectedWine = 0;
        collectionView.SetCount(collectedWine);
    }

    private void HandleCollected(Collectible collectible)
    {
        if (collectible.stageName == null) return;
        collectedWine++;
        collectionView.SetCount(collectedWine);

        if(collectedWine == totalWine)
        {
            // 미션 트래커에 수집 완료 알림
            StageMissionTracker.RaiseWineCollectedAll(stageName);
        }
    }
}
