using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityController : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private Image ImgFill;
    [SerializeField] private Image StopWatchImg;
    [SerializeField] private Image GuardImg;

    [SerializeField] private LightController lightController;
    [SerializeField] private GameObject Guard;

    private LightSystem lightSystem;
    public bool ActiveStopLight = false;
    public bool ActiveGuard = false;

    private bool isUsingSkill = false;
    private readonly float _increaseStep = 0.1f;

    private readonly float _stopLightGaze = 0.3f;
    private readonly float _guardGaze = 0.7f;

    private readonly Color32 activeColor = new Color32(255, 255, 255, 255);
    private readonly Color32 inactiveColor = new Color32(0, 0, 0, 255);

    private List<IObstacle> _obstacles = new();

    private void Awake()
    {
        ActiveStopLight = GameManager.Accomplishment.ActiveStopLight;
        ActiveGuard = GameManager.Accomplishment.ActiveGuard;

        _obstacles = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                           .OfType<IObstacle>()
                           .ToList();
    }

    void Start()
    {
        StopWatchImg.gameObject.SetActive(ActiveStopLight);
        GuardImg.gameObject.SetActive(ActiveGuard);

        lightSystem = Player.GetComponent<LightSystem>();
    }

    async void Update()
    {
        float currentFill = ImgFill.fillAmount;

        if (ActiveStopLight || ActiveGuard)
        {
            FillGazeIfOnShadow();
        }

        await CheckUseAbility();
        UpdateIcon(ActiveStopLight, currentFill >= _stopLightGaze, StopWatchImg);
        UpdateIcon(ActiveGuard, currentFill >= _guardGaze, GuardImg);
    }

    public void FillGazeIfOnShadow()
    {
        // 스킬 사용중이 아니면서, 그림자 안에 있을 때만 게이지 차오르게
        if (!isUsingSkill && lightSystem.lsOnShadow)
        {
            // 초당 증가량 = 10%/1초 → 0.1f
            ImgFill.fillAmount = Mathf.Min(1f, ImgFill.fillAmount + _increaseStep * Time.deltaTime);
        }
    }

    public async Task CheckUseAbility()
    {
        if (isUsingSkill) return;

        if (ActiveStopLight && Input.GetKeyDown(KeyCode.Q) && ImgFill.fillAmount >= _stopLightGaze)
        {
            // 광원 멈추기 스킬 사용
            isUsingSkill = true;

            ImgFill.fillAmount -= _stopLightGaze;
            lightController.IsStop = true;
            foreach(var obstacle in _obstacles)
            {
                obstacle.IsStop = true;
            }

            await UniTask.Delay(5000);

            lightController.IsStop = false;
            isUsingSkill = false;
            foreach (var obstacle in _obstacles)
            {
                obstacle.IsStop = false;
            }
        }

        if (ActiveGuard && Input.GetKeyDown(KeyCode.E) && ImgFill.fillAmount >= _guardGaze)
        {
            // 무적 스킬 사용
            isUsingSkill = true;
            ImgFill.fillAmount -= _guardGaze;

            Guard.SetActive(true);
            lightSystem.enabled = false;

            await UniTask.Delay(3000);

            Guard.SetActive(false);
            lightSystem.enabled = true;
            isUsingSkill = false;
        }
    }

    public void UpdateIcon(bool isActive, bool condition, Image targetImage)
    {
        if (targetImage == null) return;

        targetImage.color = (isActive && condition) ? activeColor : inactiveColor;
    }
}
