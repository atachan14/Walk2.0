using JetBrains.Annotations;
using System;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    bool isOpen = false;
    public GameObject menuOpener;
    public TextMeshProUGUI menuOpenerText;

    public GameObject menuValue;

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        menuValue.SetActive(isOpen);
        menuOpenerText.text = isOpen ? "menuÅ£" : "menuÅ•";
    }
    public void HiddenMenu()
    {
        menuOpener.SetActive(false);
    }

    public void ClickDiary()
    {
        StartCoroutine(DieryManager.Instance.OpenDiery());
    }

    public void ClickOption()
    {
        OptionManager.Instance.Open();
    }
    public void ClickRitire()
    {
        ToggleMenu();
        GameData.Instance.EndTime = DateTime.Now;
        StartCoroutine(FirebaseManager.Instance.SetSaveDataCoroutine());
        BGMManager.Instance.PlayRetire();
        MainDisplay.Instance.UpdateDisplay();
    }
}
