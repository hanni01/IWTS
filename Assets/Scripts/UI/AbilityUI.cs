using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private Button AbilityBtn1;
    [SerializeField] private Button AbilityBtn2;
    [SerializeField] private TMP_Text AbilityText1;
    [SerializeField] private TMP_Text AbilityText2;

    private void Awake()
    {
        AbilityBtn1.onClick.AddListener(() =>
        {
            AbilityBtn1.gameObject.SetActive(false);
            GameManager.Accomplishment.ActiveStopLight = true;

            PlayerPrefs.SetString("StopLight", "true");
        });

        AbilityBtn2.onClick.AddListener(() =>
        {
            AbilityBtn2.gameObject.SetActive(false);
            GameManager.Accomplishment.ActiveGuard = true;

            PlayerPrefs.SetString("Guard", "true");
        });
    }

    void Start()
    {
        int totalWine = GameManager.Accomplishment.TotalClearedMission();

        if (PlayerPrefs.GetString("StopLight") == "true")
        {
            AbilityBtn1.gameObject.SetActive(false);
        }
        else
        {
            AbilityText1.text = totalWine.ToString();
            if (totalWine >= 2)
            {
                AbilityBtn1.interactable = true;
                AbilityText1.text = "2";
            }
        }

        if (PlayerPrefs.GetString("Guard") == "true")
        {
            AbilityBtn2.gameObject.SetActive(false);
        }
        else
        {
            AbilityText2.text = totalWine.ToString();
            if (totalWine >= 4)
            {
                AbilityBtn2.interactable = true;
                AbilityText2.text = "4";
            }
        }
    }

    void Update()
    {
        
    }
}
