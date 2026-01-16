using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocumentDetailPanel : MonoBehaviour
{
    public static DocumentDetailPanel Instance { get; private set; }

    [Header("UI Components")]
    public GameObject panelRoot;      // 整个面板的父物体 (用来开关)
    public TMP_Text titleText;        // 标题 (Doc Name)
    public Button closeButton;        // 关闭按钮


    [Header("Content - Image Mode")]
    public GameObject imageContainer; // 图片容器 (方便控制显隐)
    public Image contentImage;        // 显示具体图片的 Image 组件

    [Header("Content - Text Mode")]
    public GameObject textContainer;  // 文字容器 (如果有 ScrollView，就拖 ScrollView)
    public TMP_Text contentText;      // 显示具体文字的 TMP

    private void Awake()
    {
        // 设置单例，保证全局只有一个详情面板
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 确保一开始是隐藏的
        Close();
    }

    private void Start()
    {
        // 绑定关闭事件
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    // ============================================================
    // 打开面板 (供外部调用)
    // ============================================================
    public void Open(string docName, DetailType type, string detailText, Sprite detailImage)
    {
        panelRoot.SetActive(true);
        
        // 1. 设置标题
        titleText.text = string.IsNullOrEmpty(docName) ? "Document Details" : docName;

        // 2. 根据类型切换显示
        if (type == DetailType.Text)
        {
            // 文字模式
            if (textContainer != null) textContainer.SetActive(true);
            if (imageContainer != null) imageContainer.SetActive(false);

            if (contentText != null)
                contentText.text = detailText;
        }
        else
        {
            // 图片模式
            if (textContainer != null) textContainer.SetActive(false);
            if (imageContainer != null) imageContainer.SetActive(true);

            if (contentImage != null)
            {
                contentImage.sprite = detailImage;
                // 保持图片比例，不被拉伸
                contentImage.preserveAspect = true; 
            }
        }
    }

    // ============================================================
    // 关闭面板
    // ============================================================
    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}