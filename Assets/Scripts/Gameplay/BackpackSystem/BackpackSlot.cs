using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BackpackSlot : MonoBehaviour
{
    [Header("Normal UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public Button detailButton;
    public Button actionButton; // 这里是你的 Reset/Delete 按钮

    [Header("Selection UI (覆盖层)")]
    public GameObject selectOverlay;
    public Button selectButton;

    private int myIndex = -1;
    private BackpackStorage.BackpackDocument myData;

    public void Setup(int index, BackpackStorage.BackpackDocument data)
    {
        myIndex = index;
        myData = data;
        gameObject.SetActive(true);

        iconImage.sprite = data.icon;
        nameText.text = data.docName;

        // 详情点击
        detailButton.onClick.RemoveAllListeners();
        detailButton.onClick.AddListener(() => {
            if (DocumentDetailPanel.Instance != null)
                DocumentDetailPanel.Instance.Open(data.docName, data.detailType, data.detailText, data.detailImage);
        });

        // ⭐ 修复：Reset 按钮功能 (删除文件)
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => {
            Debug.Log($"Deleting Item {index}: {data.docName}");
            
            // 1. 从数据中移除
            BackpackStorage.Instance.RemoveDocument(index);
            
            // 2. 刷新背包 UI
            // 尝试找一下父物体上的 BackpackUI 脚本
            BackpackUI ui = GetComponentInParent<BackpackUI>();
            if (ui == null) ui = FindFirstObjectByType<BackpackUI>(); // 备用方案
            
            if (ui != null)
                ui.RefreshView();
        });

        if (selectOverlay != null) selectOverlay.SetActive(false);
    }

    public void EnableSelectionMode(Action<BackpackStorage.BackpackDocument> onSelected)
    {
        if (selectOverlay != null) selectOverlay.SetActive(true);
        
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => {
                onSelected?.Invoke(myData);
            });
        }
    }

    public void DisableSelectionMode()
    {
        if (selectOverlay != null) selectOverlay.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}