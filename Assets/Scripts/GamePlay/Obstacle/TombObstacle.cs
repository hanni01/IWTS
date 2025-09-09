using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class TombObstacle : MonoBehaviour, IObstacle
{
    [SerializeField] private Transform TombDoor;
    [SerializeField] private float speedDegPerSec = 90f; // 초당 도(deg) 속도

    private Quaternion rotClosed; // 닫힘 목표
    private Quaternion rotOpen;   // 열림 목표
    private bool _isMoving;

    private readonly float _closedX = 0f;
    private readonly float _openX = -140f;

    private CancellationToken _sceneToken;

    public bool IsStop { get; set; } = false;

    private void Awake()
    {
        // 현재 로컬 Y/Z는 유지하고 X만 우리가 정의
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
                await UniTask.Yield(PlayerLoopTiming.Update, _sceneToken); //토큰 연결
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("씬 언로드 MoveTo 중단됨");
        }
    }

    private async UniTaskVoid MoveOpenClose()
    {
        int randomDelay = UnityEngine.Random.Range(1000, 3000);
        await UniTask.Delay(randomDelay);
        await MoveTo(rotOpen);   // 열기

        await UniTask.Delay(300);
        await MoveTo(rotClosed); // 닫기
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
