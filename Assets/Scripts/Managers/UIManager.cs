using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class UIManager : IManager
{
    public Transform PopupUITransform {  get; private set; }
    private Dictionary<string, GameObject> _popupUIDictionary = new();

    public void Initialize()
    {
        var popupUI = GameObject.Find("PopupUI");
        PopupUITransform = popupUI.transform;
        Object.DontDestroyOnLoad(popupUI);
    }

    public void Release()
    {
        _popupUIDictionary.Clear();
    }

    public GameObject ShowPopup(string popupName)
    {
        if(!_popupUIDictionary.TryGetValue(popupName + "(Clone)", out var targetPopup))
        {
            var popup = Resources.Load<GameObject>("UI/" + popupName);
            targetPopup = popup;
            var activePopup = Object.Instantiate(targetPopup, PopupUITransform);
            _popupUIDictionary.Add(activePopup.name, activePopup);

            activePopup.SetActive(true);

            return activePopup;
        }

        targetPopup.SetActive(true);

        return targetPopup;
    }

    public GameObject ShowPopupFalse(string popupName)
    {
        if (!_popupUIDictionary.TryGetValue(popupName + "(Clone)", out var targetPopup))
        {
            var popup = Resources.Load<GameObject>("UI/" + popupName);
            targetPopup = popup;
            var activePopup = Object.Instantiate(targetPopup, PopupUITransform);
            _popupUIDictionary.Add(activePopup.name, activePopup);

            return activePopup;
        }

        targetPopup.SetActive(true);

        return targetPopup;
    }

    public bool GetActivePopupUI(string popupName)
    {
        if (!_popupUIDictionary.TryGetValue(popupName + "(Clone)", out var targetPopup))
        {
            Debug.Log("�ش� popup�� �������� �ʽ��ϴ�.");
            return false;
        }

        return true;
    }

    public void HidePopup(string popupName)
    {
        if (!_popupUIDictionary.TryGetValue(popupName + "(Clone)", out var targetPopup))
        {
            Debug.LogError("�ش� popup�� Ȱ��ȭ �Ǿ����� �ʽ��ϴ�.");
        }

        targetPopup.SetActive(false);

        Debug.Log($"�˾� {popupName} ����� �Ϸ�");
    }
}
