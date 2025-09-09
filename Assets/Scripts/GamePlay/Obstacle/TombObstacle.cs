using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class TombObstacle : MonoBehaviour, IObstacle
{
    [SerializeField] private Transform TombDoor;
    [SerializeField] private float speedDegPerSec = 90f; // �ʴ� ��(deg) �ӵ�

    private Quaternion rotClosed; // ���� ��ǥ
    private Quaternion rotOpen;   // ���� ��ǥ
    private bool _isMoving;

    private readonly float _closedX = 0f;
    private readonly float _openX = -140f;

    private CancellationToken _sceneToken;

    public bool IsStop { get; set; } = false;

    private void Awake()
    {
        // ���� ���� Y/Z�� �����ϰ� X�� �츮�� ����
        var e = TombDoor.localEulerAngles;

        rotClosed = Quaternion.Euler(_closedX, e.y, e.z);
        rotOpen = Quaternion.Euler(_openX, e.y, e.z);
    }

    private void Start()
    {
        _sceneToken = GameManager.Scene.GetSceneToken();
        MoveDoorLoop().Forget();
    }

    // IObstacle
    public void Movement()
    {
        if (_isMoving) return;
        _isMoving = true;
        MoveOpenClose().Forget();
    }

    private async UniTask MoveTo(Quaternion target)
    {
        if (!TombDoor) return;

        try
        {
            while (Quaternion.Angle(TombDoor.localRotation, target) > 0.1f)
            {
                TombDoor.localRotation = Quaternion.RotateTowards(
                    TombDoor.localRotation,
                    target,
                    speedDegPerSec * Time.deltaTime
                );
                await UniTask.Yield(PlayerLoopTiming.Update, _sceneToken); //��ū ����
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("�� ��ε� MoveTo �ߴܵ�");
        }
    }

    private async UniTaskVoid MoveOpenClose()
    {
        int randomDelay = UnityEngine.Random.Range(1000, 3000);
        await UniTask.Delay(randomDelay);
        await MoveTo(rotOpen);   // ����

        await UniTask.Delay(300);
        await MoveTo(rotClosed); // �ݱ�
        _isMoving = false;
    }

    private async UniTaskVoid MoveDoorLoop()
    {
        while (!IsStop)
        {
            Movement();
            await UniTask.Delay(100);
        }
    }
}
