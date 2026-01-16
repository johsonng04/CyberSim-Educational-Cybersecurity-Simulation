using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class MessageBubbleUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text customerNameText;
    public TMP_Text messageText;

    [Header("Document Display Group")]
    public GameObject documentContainer; 
    public Button documentButton;        // 点击查看详情 (Icon)
    public Image documentIconImage;      
    public TMP_Text documentNameText;    // 显示文件名字

    [Header("Interaction")]
    public Button takeButton;            // Take 按钮

    // ⭐ 修复关键：防止连点导致颜色逻辑错误的开关
    private bool isFeedbackActive = false;

    private void Start()
    {
        HideAll();
    }

    // ============================================================
    // 基础信息设置
    // ============================================================
    public void SetCustomerName(string name)
    {
        if (customerNameText != null)
        {
            customerNameText.text = name;
            customerNameText.gameObject.SetActive(true);
        }
    }

    public void ShowTaskMessage(string msg)
    {
        gameObject.SetActive(true);
        if (messageText != null)
        {
            messageText.text = msg;
            messageText.gameObject.SetActive(true);
        }
        
        // 默认隐藏文档区域
        if (documentContainer != null) documentContainer.SetActive(false);
    }

    // ============================================================
    // 显示文档 + 绑定 Take 事件
    // ============================================================
    public void ShowDocument(string docName, Sprite icon, DetailType type, string detailText, Sprite detailImage, Action onTakeCallback)
    {
        if (documentContainer != null) documentContainer.SetActive(true);

        // 1. 设置名字
        if (documentNameText != null)
            documentNameText.text = docName;

        // 2. 设置图标
        if (documentIconImage != null)
        {
            documentIconImage.sprite = icon;
            documentIconImage.gameObject.SetActive(true);
        }

        // 3. 绑定 "点击图标查看详情"
        if (documentButton != null)
        {
            documentButton.interactable = true;
            documentButton.onClick.RemoveAllListeners();
            documentButton.onClick.AddListener(() => {
                if (DocumentDetailPanel.Instance != null)
                    DocumentDetailPanel.Instance.Open(docName, type, detailText, detailImage);
            });
        }

        // 4. 绑定 "Take" 按钮
        if (takeButton != null)
        {
            takeButton.gameObject.SetActive(true); // 始终显示
            takeButton.onClick.RemoveAllListeners();
            takeButton.onClick.AddListener(() => {
                // 触发回调 (通知 Manager 去处理逻辑)
                onTakeCallback?.Invoke();
            });
        }
    }

    // ============================================================
    // ⭐ 提示 "已经拿过了" (带防连点锁)
    // ============================================================
    public void ShowAlreadyTakenFeedback()
    {
        // 🔒 如果已经在播放动画，直接无视这次点击，防止颜色卡死
        if (isFeedbackActive) return;

        StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        if (takeButton == null) yield break;

        // 🔒 上锁
        isFeedbackActive = true;

        // 1. 记录原始状态 (这时候一定是正常的颜色，因为有锁保护)
        Color originalColor = takeButton.image.color;
        
        TMP_Text btnText = takeButton.GetComponentInChildren<TMP_Text>();
        string originalText = "";
        if (btnText != null) originalText = btnText.text;

        // 2. 变成警告状态 (变红)
        takeButton.image.color = Color.red; 
        if (btnText != null) btnText.text = "In Bag!";

        // 3. 等待 0.5 秒
        yield return new WaitForSeconds(0.5f);

        // 4. 恢复原始状态
        takeButton.image.color = originalColor;
        if (btnText != null) btnText.text = originalText;

        // 🔓 解锁
        isFeedbackActive = false;
    }

    // ============================================================
    // 隐藏所有
    // ============================================================
    public void HideAll()
    {
        gameObject.SetActive(false);
        if (documentContainer != null) documentContainer.SetActive(false);
        
        // 重置 Take 按钮状态 (以防万一隐藏时还是红色的)
        if (takeButton != null)
        {
            takeButton.gameObject.SetActive(false);
            takeButton.image.color = Color.white; // 假设默认是白色，或者你可以在Start里记录默认色
        }
        
        isFeedbackActive = false; // 重置锁
    }
}