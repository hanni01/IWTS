using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private List<Button> StageBtnList;
    [SerializeField] private Image LockImage;
    [SerializeField] private Image LimitModeStageImage;
    [SerializeField] private Button AbilityBtn;
    [SerializeField] private GameObject AbilityPanel;

    private Dictionary<string, List<Image>> _stageStateDic = new();
    private int currentIndex = 0;
    private Material outline;
    private bool _isAbilityUIActive = false;

    private void Awake()
    {
        AbilityBtn.onClick.AddListener(() =>
        {
            AbilityPanel.SetActive(true);
            _isAbilityUIActive = true;
        });

        PlayerPrefs.SetString("Dead", "false");

        if (StageBtnList.Count == 0)
        {
            Debug.LogError("StageBtnList가 비어있습니다!");
            return;
        }

        for(int i = 0;i < StageBtnList.Count;i++)
        {
            List<Image> _eachStageState = new();
            var images = StageBtnList[i].GetComponentsInChildren<Image>(true);
            foreach(var img in images)
            {
                if(img.gameObject != StageBtnList[i].gameObject)
                {
                    _eachStageState.Add(img);
                }
            }

            _stageStateDic.Add(StageBtnList[i].name, _eachStageState);
        }

        outline = new Material(Shader.Find("UI/Outline2D"));
        HighlightCurrent();

        ShowClearState();
    }

    private void Update()
    {
        if (_isAbilityUIActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                AbilityPanel.SetActive(false);
                _isAbilityUIActive = false;
            }

            return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.UI.ShowPopup(UI_Key.QUIT);
            }
        }

        // 방향키 입력 감지
        Vector2 input = Vector2.zero;
        if (Keyboard.current.upArrowKey.wasPressedThisFrame) input.y = 1;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame) input.y = -1;
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) input.x = -1;
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) input.x = 1;

        if (input != Vector2.zero)
        {
            Navigate(input);
        }

        // 스페이스바 선택
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            var stageName = StageBtnList[currentIndex].name;
            GameManager.Scene.LoadScene(stageName);
        }
    }

    private void ShowClearState()
    {
        foreach(var stage in StageBtnList)
        {
            if(_stageStateDic.TryGetValue(stage.name, out var state))
            {
                for(int i = 0; i < state.Count; i++)
                {
                    var id = int.Parse(state[i].name);
                    if (GameManager.Accomplishment.IsClearMission(stage.name, id))
                    {
                        state[i].color = new Color32(255, 255, 255, 255);
                    }
                }
            }
        }

        if(GameManager.Accomplishment.TotalClearedMission() >= 1)
        {
            LockImage.gameObject.SetActive(false);
            LimitModeStageImage.color = new Color32(255, 255, 255, 255);

            var limitModeBtn = LimitModeStageImage.gameObject.GetComponent<Button>();

            StageBtnList.Add(limitModeBtn);
        }
    }

    #region 키보드 Navigate 맵 선택 관련
    private void Navigate(Vector2 dir)
    {
        // 단순 일렬 이동 예제
        if (dir.y > 0 || dir.x < 0) currentIndex--; // Up / Left
        else if (dir.y < 0 || dir.x > 0) currentIndex++; // Down / Right

        if (currentIndex < 0) currentIndex = StageBtnList.Count - 1;
        if (currentIndex >= StageBtnList.Count) currentIndex = 0;

        HighlightCurrent();
    }

    private void HighlightCurrent()
    {
        foreach(var item in StageBtnList)
        {
            item.GetComponent<Image>().material = null;
        }

        // HighlightFrame을 선택된 버튼 위치로 이동
        var image = StageBtnList[currentIndex].GetComponent<Image>();
        image.material = outline;
    }
    #endregion
}
