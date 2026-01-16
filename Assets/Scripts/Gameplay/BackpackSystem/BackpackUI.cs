using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System; 

public class BackpackUI : MonoBehaviour
{
    [Header("UI Slots")]
    public List<BackpackSlot> slots;

    [Header("Selection UI")]
    public Button cancelSelectionButton; // ⭐ 新增：取消选择按钮

    private void OnEnable()
    {
        RefreshView();
    }

    public void RefreshView()
    {
        if (BackpackStorage.Instance == null) return;

        List<BackpackStorage.BackpackDocument> dataList = BackpackStorage.Instance.documentList;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < dataList.Count)
                slots[i].Setup(i, dataList[i]);
            else
                slots[i].Hide();
        }
        
        // 刷新时重置状态
        ExitSelectionMode();
    }

    // ============================================================
    // 进入选择模式
    // ============================================================
    public void EnterSelectionMode(Action<BackpackStorage.BackpackDocument> callback)
    {
        // 1. 显示取消按钮
        if (cancelSelectionButton != null)
        {
            cancelSelectionButton.gameObject.SetActive(true);
            cancelSelectionButton.onClick.RemoveAllListeners();
            cancelSelectionButton.onClick.AddListener(() => {
                // 点击取消：直接退出模式，不执行 callback
                ExitSelectionMode();
            });
        }

        // 2. 开启所有格子的选择遮罩
        foreach (var slot in slots)
        {
            if (slot.gameObject.activeSelf) 
            {
                slot.EnableSelectionMode((doc) => {
                    // 选中文件：执行回调 + 退出模式
                    callback?.Invoke(doc); 
                    ExitSelectionMode();   
                });
            }
        }
    }

    // ============================================================
    // 退出选择模式
    // ============================================================
    public void ExitSelectionMode()
    {
        // 1. 隐藏取消按钮
        if (cancelSelectionButton != null)
            cancelSelectionButton.gameObject.SetActive(false);

        // 2. 关闭所有格子的遮罩
        foreach (var slot in slots)
        {
            slot.DisableSelectionMode();
        }
    }
}