using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BookController : MonoBehaviour
{
    // ============================================================
    // 1. 定义每一页的数据结构 (给页面贴标签)
    // ============================================================
    [System.Serializable]
    public class BookPage
    {
        public string pageType;      // 类型标签 (例如: "encrypt", "virus")
        public GameObject pageObj;   // 页面物体
    }

    [Header("UI Panels")]
    public GameObject bookRootPanel; 
    public Button openBookButton;    
    public Button closeBookButton;   

    [Header("Navigation")]
    public Button prevButton;        
    public Button nextButton;        
    public TMP_Text pageNumberText;  

    [Header("Filter Settings")]
    public TMP_Dropdown filterDropdown; // ⭐ 拖入你的下拉菜单
    public string allCategoryName = "All"; // 下拉菜单里代表“全部”的选项名字

    [Header("Pages Inventory (Warehouse)")]
    // ⭐ 把所有页面拖进这里，并填好对应的 pageType
    public List<BookPage> allPages = new List<BookPage>(); 

    // 运行时变量：当前正在展示的页面列表 (活页夹)
    private List<GameObject> currentActivePages = new List<GameObject>();
    private int currentPageIndex = 0;

    private void Start()
    {
        // 初始状态：书是关的
        bookRootPanel.SetActive(false);
        if (openBookButton) openBookButton.gameObject.SetActive(true);

        // 绑定按钮事件
        if (openBookButton) openBookButton.onClick.AddListener(OpenBook);
        if (closeBookButton) closeBookButton.onClick.AddListener(CloseBook);
        if (prevButton) prevButton.onClick.AddListener(OnPrevPage);
        if (nextButton) nextButton.onClick.AddListener(OnNextPage);

        // ⭐ 绑定下拉菜单事件
        if (filterDropdown)
        {
            filterDropdown.onValueChanged.AddListener(OnFilterChanged);
        }
    }

    // ============================================================
    // 打开/关闭书本
    // ============================================================
    public void OpenBook()
    {
        bookRootPanel.SetActive(true);

        // 每次打开书，根据当前 Dropdown 的值来初始化页面
        if (filterDropdown)
        {
            ApplyFilter(filterDropdown.options[filterDropdown.value].text);
        }
        else
        {
            // 如果没配 Dropdown，就显示所有
            ApplyFilter(allCategoryName);
        }
    }

    public void CloseBook()
    {
        bookRootPanel.SetActive(false);
        if (openBookButton) openBookButton.gameObject.SetActive(true);
    }

    // ============================================================
    // ⭐ 筛选逻辑 (核心功能)
    // ============================================================
    void OnFilterChanged(int index)
    {
        // 获取当前选中的文本 (例如 "encrypt")
        string selectedType = filterDropdown.options[index].text;
        ApplyFilter(selectedType);
    }

    void ApplyFilter(string type)
    {
        // 1. 先把仓库里所有的页面都隐藏 (归零)
        foreach (var page in allPages)
        {
            if(page.pageObj != null) 
                page.pageObj.SetActive(false);
        }

        // 2. 清空当前活动列表
        currentActivePages.Clear();

        // 3. 重新筛选：去仓库里找匹配的页
        foreach (var page in allPages)
        {
            // 逻辑：如果选的是 "All" 或者 页面类型匹配
            // 注意：这里用了 ToLower() 忽略大小写，防止你手误写成 Encrypt 和 encrypt
            if (type == allCategoryName || page.pageType.ToLower() == type.ToLower())
            {
                currentActivePages.Add(page.pageObj);
            }
        }

        // 4. 重置页码到第 0 页
        currentPageIndex = 0;

        // 5. 刷新显示
        UpdatePageDisplay();
    }

    // ============================================================
    // 翻页逻辑 (操作的是 currentActivePages)
    // ============================================================
    void OnPrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    void OnNextPage()
    {
        if (currentPageIndex < currentActivePages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    // ============================================================
    // 刷新显示
    // ============================================================
    void UpdatePageDisplay()
    {
        // 如果筛选结果为空 (比如选了 virus 但其实没这张纸)，处理一下防止报错
        if (currentActivePages.Count == 0)
        {
            if (pageNumberText) pageNumberText.text = "0 / 0";
            if (prevButton) prevButton.interactable = false;
            if (nextButton) nextButton.interactable = false;
            return;
        }

        // 1. 只控制 "当前活动列表" 里的页面
        for (int i = 0; i < currentActivePages.Count; i++)
        {
            // 如果 index 匹配，设为 true，否则 false
            currentActivePages[i].SetActive(i == currentPageIndex);
        }

        // 2. 更新按钮
        if (prevButton) prevButton.interactable = (currentPageIndex > 0);
        if (nextButton) nextButton.interactable = (currentPageIndex < currentActivePages.Count - 1);

        // 3. 更新页码
        if (pageNumberText)
        {
            pageNumberText.text = $"{currentPageIndex + 1} / {currentActivePages.Count}";
        }
    }
}