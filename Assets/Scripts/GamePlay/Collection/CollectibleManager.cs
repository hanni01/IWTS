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
            // �̼� Ʈ��Ŀ�� ���� �Ϸ� �˸�
            StageMissionTracker.RaiseWineCollectedAll(stageName);
        }
    }
}
