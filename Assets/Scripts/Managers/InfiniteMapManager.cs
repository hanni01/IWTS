using System.Collections.Generic;
using UnityEngine;

public class InfiniteMapManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject firstMapPrefab;
    [SerializeField] private GameObject mapPrefab; // 반복할 맵 프리팹
    [SerializeField] private int maxActiveBlocks = 3; // 화면에 최대 블록 수
    [SerializeField] private float blockLength = 160f; // 블록 길이 (Z 방향)
    [SerializeField] private float spawnThreshold = 140f; // 플레이어 이동 기준 다음 블록 생성 Z

    private List<GameObject> activeBlocks = new List<GameObject>();
    private Queue<GameObject> blockPool = new Queue<GameObject>();

    void Start()
    {
        activeBlocks.Add(firstMapPrefab);
    }

    void Update()
    {
        // 마지막 블록 시작 Z
        float lastBlockStartZ = activeBlocks[activeBlocks.Count - 1].transform.position.z;

        // 플레이어가 다음 블록 생성 기준에 도달했는지 체크
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

        // 풀에 남은 블록이 있으면 재활용, 없으면 새로 생성
        if (blockPool.Count > 0)
        {
            block = blockPool.Dequeue();
        }
        else
        {
            block = Instantiate(mapPrefab);
        }

        // 위치 세팅
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
            oldBlock.SetActive(false);  // 풀링용 비활성화
            blockPool.Enqueue(oldBlock);
            activeBlocks.RemoveAt(0);
        }
    }
}
