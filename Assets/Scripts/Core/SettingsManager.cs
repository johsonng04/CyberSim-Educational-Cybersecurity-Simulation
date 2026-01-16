using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI")]
    public Toggle fullscreenToggle;
    public GameObject creditsPanel;
    public TMP_Text creditsText;

    private void Start()
    {
        // 1. 强制设置分辨率为 1920x1080
        // Screen.fullScreen 获取当前是否全屏，保持现有状态
        Screen.SetResolution(1920, 1080, Screen.fullScreen);

        // 2. 初始化 Fullscreen Toggle
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        // 3. 初始化 Credits 面板
        creditsPanel.SetActive(false);
        creditsText.text =
            "Game Developer: XXX\n" +
            "Design: XXX\n" +
            "Version: 1.0.0\n" +
            "Thank you for playing!";
    }

    // -------------------------
    // Fullscreen Function
    // -------------------------
    public void SetFullscreen(bool isFullscreen)
    {
        // 切换全屏时，同时也强制保持 1920x1080
        Screen.SetResolution(1920, 1080, isFullscreen);
    }

    // -------------------------
    // Credits Panel Function
    // -------------------------
    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }
}