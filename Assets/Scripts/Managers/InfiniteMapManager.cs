using System.Collections.Generic;
using UnityEngine;

public class InfiniteMapManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject firstMapPrefab;
    [SerializeField] private GameObject mapPrefab; // �ݺ��� �� ������
    [SerializeField] private int maxActiveBlocks = 3; // ȭ�鿡 �ִ� ��� ��
    [SerializeField] private float blockLength = 160f; // ��� ���� (Z ����)
    [SerializeField] private float spawnThreshold = 140f; // �÷��̾� �̵� ���� ���� ��� ���� Z

    private List<GameObject> activeBlocks = new List<GameObject>();
    private Queue<GameObject> blockPool = new Queue<GameObject>();

    void Start()
    {
        activeBlocks.Add(firstMapPrefab);
    }

    void Update()
    {
        // ������ ��� ���� Z
        float lastBlockStartZ = activeBlocks[activeBlocks.Count - 1].transform.position.z;

        // �÷��̾ ���� ��� ���� ���ؿ� �����ߴ��� üũ
        if (player.position.z >= lastBlockStartZ + spawnThreshold)
        {
            float nextZ = lastBlockStartZ + blockLength;
            SpawnNextBlock(nextZ);
            RemoveOldBlock();
        }
    }

    void SpawnNextBlock(float zPosition)
    {
        GameObject block;

        // Ǯ�� ���� ����� ������ ��Ȱ��, ������ ���� ����
        if (blockPool.Count > 0)
        {
            block = blockPool.Dequeue();
        }
        else
        {
            block = Instantiate(mapPrefab);
        }

        // ��ġ ����
        block.transform.position = new Vector3(0f, 0f, zPosition);
        block.transform.rotation = Quaternion.identity;
        block.SetActive(true);

        activeBlocks.Add(block);
    }

    void RemoveOldBlock()
    {
        if (activeBlocks.Count > maxActiveBlocks)
        {
            GameObject oldBlock = activeBlocks[0];
            oldBlock.SetActive(false);  // Ǯ���� ��Ȱ��ȭ
            blockPool.Enqueue(oldBlock);
            activeBlocks.RemoveAt(0);
        }
    }
}
