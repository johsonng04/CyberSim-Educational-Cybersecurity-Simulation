using UnityEngine;
using UnityEngine.SceneManagement;

public class Start_Scene_Script : MonoBehaviour
{
    [Header("Panels to Toggle")]
    public GameObject levelSelectionPanel; // 关卡面板
    public GameObject settingsPanel;       // 设置面板

    // ❌ 已删除 startMenuRoot，因为不需要隐藏主菜单按钮了

    private void Start()
    {
        // 游戏开始时，确保这两个弹窗是关闭的
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    // ============================================================
    // 按钮事件
    // ============================================================

    // 点击 "START GAME" -> 只需要把关卡面板打开
    public void OnStartButtonClicked()
    {
        if (levelSelectionPanel) 
            levelSelectionPanel.SetActive(true);
    }

    // 点击关卡面板的 "Back" -> 只需要把关卡面板关掉
    public void OnBackFromLevelSelect()
    {
        if (levelSelectionPanel) 
            levelSelectionPanel.SetActive(false);
    }

    // 点击 "SETTINGS" -> 打开设置面板
    public void OpenSettings() 
    { 
        if (settingsPanel) 
            settingsPanel.SetActive(true); 
    }

    // 点击设置面板的 "Back" -> 关闭设置面板
    public void CloseSettings() 
    { 
        if (settingsPanel) 
            settingsPanel.SetActive(false); 
    }
    
    public void Exit_Game()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }
}