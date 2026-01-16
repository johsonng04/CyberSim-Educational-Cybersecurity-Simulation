using UnityEngine;

public class UIOrderFixer : MonoBehaviour
{
    [Header("UI References (Order: Bottom to Top)")]
    public RectTransform customerUI;    // 最底层 (Layer 0)
    public RectTransform backpackUI;    // 常驻层 1
    public RectTransform stationPanel;  // 常驻层 2
    
    [Header("Popups (Sorted by Priority)")]
    public RectTransform detailPopup;   // 详情弹窗
    public RectTransform levelComplete; // 结算面板
    public RectTransform BookPanel;     // 书本面板
    
    public RectTransform tutorialPanel; // ⭐ 新增: 教程面板
    
    public RectTransform PausePanel;    // 暂停面板
    public RectTransform loadingPanel;  // Loading 界面 (顶层)

    void LateUpdate()
    {
        // 1. Customer 永远垫底
        if (customerUI != null)
            customerUI.SetSiblingIndex(0);

        // 2. Backpack 在 Customer 上面
        if (backpackUI != null)
            backpackUI.SetAsLastSibling();

        // 3. Station 在 Backpack 上面
        if (stationPanel != null)
            stationPanel.SetAsLastSibling();

        // 4. 详情弹窗
        if (detailPopup != null && detailPopup.gameObject.activeSelf)
        {
            detailPopup.SetAsLastSibling();
        }

        // 5. 结算面板
        if (levelComplete != null && levelComplete.gameObject.activeSelf)
        {
            levelComplete.SetAsLastSibling();
        }

        // 6. 书本面板
        if (BookPanel != null && BookPanel.gameObject.activeSelf)
        {
            BookPanel.SetAsLastSibling();
        }

        // 7. ⭐ 新增: 教程面板 (Tutorial)
        // 放在这里意味着：它会盖住书本和游戏界面，但会被暂停菜单盖住
        if (tutorialPanel != null && tutorialPanel.gameObject.activeSelf)
        {
            tutorialPanel.SetAsLastSibling();
        }

        // 8. 暂停面板 (通常暂停要盖住教程)
        if (PausePanel != null && PausePanel.gameObject.activeSelf)
        {
            PausePanel.SetAsLastSibling();
        }

        // 9. Loading 面板 (绝对顶层)
        if (loadingPanel != null && loadingPanel.gameObject.activeSelf)
        {
            loadingPanel.SetAsLastSibling();
        }
    }
}