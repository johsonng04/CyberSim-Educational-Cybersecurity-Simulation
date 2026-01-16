using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Station_Manager : MonoBehaviour
{
    [Header("Station Panels")]
    public RectTransform panelOrder;
    public RectTransform panelKill;     
    public RectTransform panelCheck;    
    public RectTransform panelProtect;  
    public RectTransform panelEncrypt;  
    public RectTransform panelDecrypt;  

    private RectTransform currentPanel;

    [Header("Navigation Buttons")]
    public Button btnOrder;
    public Button btnKill;
    public Button btnCheck;
    public Button btnProtect;
    public Button btnEncrypt;
    public Button btnDecrypt;

    // ⭐ 新增：在这里设置颜色，不用改代码了
    [Header("Button Style Settings")]
    public Color selectedColor = Color.white;   // 选中时的颜色
    public Color unselectedColor = Color.gray;  // 未选中时的颜色

    void Start()
    {
        // 游戏开始默认显示 Order 面板
        currentPanel = panelOrder;
        currentPanel.SetAsLastSibling(); 

        // 初始化按钮状态
        ResetAllButtons(); // 先把所有按钮变灰
        Highlight(btnOrder); // 再高亮第一个
    }

    // ============================================================
    // 核心切换逻辑
    // ============================================================
    private void SwapPanel(RectTransform targetPanel, Button targetButton)
    {
        if (currentPanel == targetPanel) return;

        // 1. 交换位置 
        Vector2 temp = currentPanel.anchoredPosition;
        currentPanel.anchoredPosition = targetPanel.anchoredPosition;
        targetPanel.anchoredPosition = temp;

        // 2. 调整层级 
        targetPanel.SetAsLastSibling();

        // 3. 更新当前记录
        currentPanel = targetPanel;

        // 4. 更新按钮颜色
        ResetAllButtons();
        Highlight(targetButton);
    }

    // ============================================================
    // 按钮点击事件
    // ============================================================
    public void ShowOrder() { SwapPanel(panelOrder, btnOrder); }
    public void ShowKill() { SwapPanel(panelKill, btnKill); }
    public void ShowCheck() { SwapPanel(panelCheck, btnCheck); }
    public void ShowProtect() { SwapPanel(panelProtect, btnProtect); }
    public void ShowEncrypt() { SwapPanel(panelEncrypt, btnEncrypt); }
    public void ShowDecrypt() { SwapPanel(panelDecrypt, btnDecrypt); }


    // ============================================================
    // 视觉效果：按钮高亮/低亮
    // ============================================================
    private void ResetAllButtons()
    {
        Unhighlight(btnOrder);
        Unhighlight(btnKill);
        Unhighlight(btnCheck);
        Unhighlight(btnProtect);
        Unhighlight(btnEncrypt);
        Unhighlight(btnDecrypt);
    }

    private void Highlight(Button btn)
    {
        if (btn == null) return;
        ColorBlock cb = btn.colors;
        
        // ⭐ 使用你在 Inspector 里设置的 selectedColor
        cb.normalColor = selectedColor; 
        cb.selectedColor = selectedColor;
        // 这种风格通常不需要高亮效果，所以把 Highlighted 也设为一样，防止鼠标悬停变色
        cb.highlightedColor = selectedColor; 

        btn.colors = cb;
    }

    private void Unhighlight(Button btn)
    {
        if (btn == null) return;
        ColorBlock cb = btn.colors;

        // ⭐ 使用你在 Inspector 里设置的 unselectedColor
        cb.normalColor = unselectedColor;
        cb.selectedColor = unselectedColor;
        cb.highlightedColor = unselectedColor;

        btn.colors = cb;
    }

    public void Start_New_Game()
    {
        SceneManager.LoadSceneAsync("Summary_Scene");
        Debug.Log("change to summary scene");
    }
}