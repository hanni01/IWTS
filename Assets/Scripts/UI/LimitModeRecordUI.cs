using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LimitModeRecordUI : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private TMP_Text meterText;
    [SerializeField] private ForwardPush ForwardPush;

    private Vector3 startPoint;
    private PlayerController controller;
    private HealthSystem healthSystem;
    private LightSystem lightSystem;
    private bool _isEnd = false;
    private List<IObstacle> _obstacles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Player != null)
        {
            startPoint = Player.transform.position;
            controller = Player.GetComponent<PlayerController>();
            healthSystem = Player.GetComponent<HealthSystem>();
            lightSystem = Player.GetComponent<LightSystem>();
        }

        _obstacles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                           .OfType<IObstacle>()
                           .ToList();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null || meterText == null) return;

        float distance = Vector3.Distance(
            new Vector3(startPoint.x, 0, startPoint.z),
            new Vector3(Player.position.x, 0 , Player.position.z)
        );

        meterText.text = $"{Mathf.FloorToInt(distance)}";

        if(healthSystem.currentHealth <= 0)
        {
            if (_isEnd) return;
            controller.IsStop = true;
            healthSystem.IsStop = true;
            lightSystem.IsStop = true;
            ForwardPush.IsStop = true;

            foreach (var obstacle in _obstacles)
            {
                obstacle.IsStop = true;
            }

            _isEnd = true;
            Debug.Log("체력 0 이하 게임 종료! 기록을 저장합니다.");

            GameManager.GameData.UpdateRecordAndRank((int)distance);
        }
    }
}
