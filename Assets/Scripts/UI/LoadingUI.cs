using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text progressText;

    private float _progressValue = 0f;

    public void UpdateLoadingUI(string text, float progress)
    {
        if(!isValidValue(progress) || !isValidText(text))
        {
            Debug.LogError($"진행 텍스트 또는 진행률이 유효하지 않는 값입니다.");
            return;
        }

        _progressValue = Mathf.Lerp(0f, 1f, progress);

        progressBar.fillAmount = _progressValue;
        progressText.text = "Loading... " + text;
    }

    private bool isValidValue(float value)
    {
        return value >= 0f && value <= 1f;
    }

    private bool isValidText(string text)
    {
        return !string.IsNullOrEmpty(text);
    }
}
