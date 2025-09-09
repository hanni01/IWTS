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
        ToggleLoop().Forget(); // �ݺ� ����
    }

    private async UniTaskVoid ToggleLoop()
    {
        try
        {
            while (true)
            {
                // �� ��ȯ ��� üũ
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
            Debug.Log("�� ��ε� ToggleLoop �ߴܵ�");
        }
    }

    public void Movement()
    {
        if (!_meshRenderer) return; // MeshRenderer�� �̹� Destroy �ƴ��� üũ

        if (!_isBrown)
        {
            _meshRenderer.material = brownM;   // �׳� ��ü
            if (_light) _light.SetActive(false);
            _isBrown = true;
        }
        else
        {
            _meshRenderer.material = lightM; // ���� ��Ƽ����� ����
            if (_light) _light.SetActive(true);
            _isBrown = false;
        }
    }

}
