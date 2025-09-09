using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MirrorObstacle : MonoBehaviour, IObstacle
{
    public bool IsStop { get; set; } = true;
    [SerializeField] private Material brownM;
    [SerializeField] private Material lightM;

    private MeshRenderer _meshRenderer;
    private GameObject _light;
    private bool _isBrown = true;

    private CancellationToken _sceneToken;

    private void Awake()
    {
        Debug.Log("IsStop: " + IsStop);
        _meshRenderer = GetComponent<MeshRenderer>();
        _light = GetComponentInChildren<Light>().gameObject;

        if (IsStop)
        {
            _meshRenderer.material = brownM;
            _isBrown = true;
            _light.SetActive(false);
        }
    }

    private void Start()
    {
        _sceneToken = GameManager.Scene.GetSceneToken();
        ToggleLoop().Forget(); // 반복 시작
    }

    private async UniTaskVoid ToggleLoop()
    {
        try
        {
            while (true)
            {
                // 씬 전환 취소 체크
                _sceneToken.ThrowIfCancellationRequested();

                if (!IsStop)
                {
                    Movement();
                }

                int randomDelay = UnityEngine.Random.Range(1000, 3000);
                await UniTask.Delay(randomDelay, cancellationToken: _sceneToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("씬 언로드 ToggleLoop 중단됨");
        }
    }

    public void Movement()
    {
        if (!_meshRenderer) return; // MeshRenderer가 이미 Destroy 됐는지 체크

        if (!_isBrown)
        {
            _meshRenderer.material = brownM;   // 그냥 교체
            if (_light) _light.SetActive(false);
            _isBrown = true;
        }
        else
        {
            _meshRenderer.material = lightM; // 원래 머티리얼로 복구
            if (_light) _light.SetActive(true);
            _isBrown = false;
        }
    }

}
