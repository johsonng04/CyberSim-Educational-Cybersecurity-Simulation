using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Customer_Manager : MonoBehaviour
{
    [Header("Level System")]
    public LevelManager levelManager; // 引用 LevelManager

    [Header("UI References")]
    public Image customerUIImage;       // 显示顾客图片的 Image 组件
    public RectTransform spawnPoint;    // 出生点 (屏幕外)
    public RectTransform middlePoint;   // 站立点 (屏幕中间)
    public RectTransform backPoint;     // 离开点 (通常和出生点一样或在另一边)
    public MessageBubbleUI messageUI;   // 气泡 UI 控制脚本

    [Header("Panels")]
    public BackpackUI backpackUI;           // 背包 UI
    public OrderPanelController orderPanel; // 提交订单面板

    // 内部变量
    private CustomerUI_Controller controller; // 控制移动动画的脚本
    private LevelCustomer currentCustomer;    // 当前正在接待的顾客数据
    private int currentStepIndex = 0;         // 当前进行到第几步

    void Start()
    {
        // 获取单例引用
        levelManager = LevelManager.Instance;
        
        // 获取移动控制器
        controller = customerUIImage.GetComponent<CustomerUI_Controller>();
        
        // 游戏开始，生成第一个顾客
        SpawnCustomerFromLevel();
    }

    // ============================================================
    // 1. 生成顾客逻辑
    // ============================================================
    public void SpawnCustomerFromLevel()
    {
        // 从 LevelManager 获取当前应该上场的顾客
        currentCustomer = levelManager.GetCurrentCustomer();
        currentStepIndex = 0;

        if (currentCustomer == null)
        {
            Debug.Log("没有更多顾客了，或者关卡数据为空。");
            return;
        }

        // A. 设置外观 (⭐ 重要：每次新顾客来，都要重置回默认表情)
        customerUIImage.sprite = currentCustomer.customerSprite;
        
        // B. 设置初始位置 (瞬移到出生点)
        customerUIImage.rectTransform.anchoredPosition = spawnPoint.anchoredPosition;
        customerUIImage.gameObject.SetActive(true);

        // C. 开始移动进场
        controller.SetTarget(middlePoint.anchoredPosition);
        
        // D. 启动协程等待到达
        StartCoroutine(ShowCustomerWhenArrived());
    }

    // 协程：等待顾客走到中间
    IEnumerator ShowCustomerWhenArrived()
    {
        // 等待距离足够近
        while (Vector2.Distance(customerUIImage.rectTransform.anchoredPosition, middlePoint.anchoredPosition) > 1f)
        {
            yield return null;
        }

        // 到达后：显示名字和开场白
        messageUI.SetCustomerName(currentCustomer.customerName);
        messageUI.ShowTaskMessage(currentCustomer.message);

        // 显示第一步的任务需求 (Input)
        ShowCurrentStepInput();
    }

    // ============================================================
    // 2. 显示当前步骤的需求 (Input)
    // ============================================================
    void ShowCurrentStepInput()
    {
        // 安全检查：防止索引越界
        if (currentStepIndex >= currentCustomer.steps.Count) return;

        LevelStep step = currentCustomer.steps[currentStepIndex];

        // 如果这一步有 Input 文件 (比如顾客递给你一张纸)
        if (step.inputIcon != null)
        {
            // 调用气泡 UI 显示文件，并传入 "Take" 按钮的回调
            messageUI.ShowDocument(
                step.inputDocName,      // 文件名
                step.inputIcon,         // 图标
                step.inputDetailType,   // 详情类型
                step.inputDetailText,   // 详情文字
                step.inputDetailImage,  // 详情图片
                () => OnPlayerTakeDocument(step) // ⭐ 玩家点击 Take 时触发
            );
        }
        else
        {
            // 如果只是纯口头任务，没有东西给玩家
            messageUI.ShowTaskMessage("New Task (No Document provided)");
        }
    }

    // ============================================================
    // 3. 玩家拿取文件的逻辑
    // ============================================================
    void OnPlayerTakeDocument(LevelStep step)
    {
        // A. 检查背包里是否已经有了
        if (BackpackStorage.Instance.HasDocument(step.inputDocName))
        {
            // 提示玩家不要重复拿
            messageUI.ShowAlreadyTakenFeedback();
            return;
        }

        // B. 存入背包
        BackpackStorage.Instance.AddDocument(
            step.inputDocName,
            step.inputIcon,
            step.inputDetailType,
            step.inputDetailText,
            step.inputDetailImage,
            step.taskType, // 记录这步任务需要的类型 (kill, check...)
            step.subType   // 记录子类型
        );

        // C. 刷新背包 UI
        if (backpackUI != null) 
            backpackUI.RefreshView();
    }

    // ============================================================
    // 4. 辅助 Getter
    // ============================================================
    public LevelStep GetCurrentStep()
    {
        if (currentCustomer == null || currentStepIndex >= currentCustomer.steps.Count) 
            return null;
        
        return currentCustomer.steps[currentStepIndex];
    }

    // ============================================================
    // ⭐ NEW: 机器专用 - 查找配方 (Blind Machine Logic)
    // ============================================================
    // 根据输入文件的名字，查找这是否是该顾客某一关(任何一关)的原料
    public LevelStep GetStepInfoByInput(string inputName)
    {
        if (currentCustomer == null) return null;

        // 遍历所有步骤，寻找匹配 inputName 的步骤
        foreach (var step in currentCustomer.steps)
        {
            if (step.inputDocName == inputName)
            {
                return step; // 找到了！把这个配方返回给机器
            }
        }
        return null; // 没找到，说明这个文件不是用来做任务的
    }

    // ============================================================
    // ⭐ NEW: 按钮专用 - 尝试升级 (Referee Logic)
    // ============================================================
    // 当玩家点击 Get 按钮拿到文件时调用
    public void TryAdvanceStep(string obtainedDocName)
    {
        // 1. 获取当前这一步的目标
        LevelStep currentStep = GetCurrentStep();
        if (currentStep == null) return;

        // 2. 检查：玩家拿到的这个文件，是不是当前这一步想要的 Success 文件？
        if (obtainedDocName == currentStep.successDocName)
        {
            Debug.Log($"[Manager] Document Match! Advancing from Step {currentStepIndex}");
            GoToNextStep();
        }
        else
        {
            // 如果名字不对，说明玩家可能是在重做以前的步骤，或者拿错了文件
            // 这里什么都不做，保持当前进度不变
            Debug.Log($"[Manager] Document retrieved ({obtainedDocName}), but it's not for current step target ({currentStep.successDocName}). Progress unchanged.");
        }
    }

    // ============================================================
    // 5. 提交结果 (保持原样，以防你有其他逻辑需要)
    // ============================================================
    public void SubmitPlayerResult()
    {
        if (currentCustomer == null || currentStepIndex >= currentCustomer.steps.Count) return;

        LevelStep step = currentCustomer.steps[currentStepIndex];
        bool foundCorrectOutput = false;

        // 检查背包里是否有符合当前步骤 taskType 和 subType 的文件
        foreach (var doc in BackpackStorage.Instance.documentList)
        {
            if (doc.taskType == step.taskType && doc.subType == step.subType)
            {
                foundCorrectOutput = true;
                break;
            }
        }

        if (foundCorrectOutput)
        {
            Debug.Log($"Step {currentStepIndex + 1} Success!");
            GoToNextStep();
        }
        else
        {
            Debug.Log($"Step {currentStepIndex + 1} Failed!");
        }
    }

    // ============================================================
    // 6. 进入下一步
    // ============================================================
    // ============================================================
    // 6. 进入下一步 (修改版：保留物品，保留UI)
    // ============================================================
    void GoToNextStep()
    {
        // 1. 逻辑进度 +1
        currentStepIndex++;
        
        // 2. 检查是否全部做完
        if (currentStepIndex >= currentCustomer.steps.Count)
        {
            // 全部做完，进入“等待最终提交”状态
            HandleSuccess();
        }
        else
        {
            // 保持当前的气泡和图片不变
            Debug.Log($"[Manager] Silent Advance to Step {currentStepIndex + 1}. UI remains unchanged.");
        }
    }

    // ============================================================
    // 7. 处理反馈 (Success / Fail)
    // ============================================================
    void HandleSuccess()
    {
        Debug.Log("[Manager] All Steps Finished. Waiting for Order Panel Submission. UI remains original.");
    }

    void HandleFail()
    {
        CustomerLeaveAndRespawn();
    }

    // ============================================================
    // 8. 最终结算逻辑 (供 Order Panel 调用)
    // ============================================================
    
    // 获取正确答案的名字 (用于比对)
    public string GetFinalCorrectDocName()
    {
        if (currentCustomer == null || currentCustomer.steps.Count == 0) return "";
        LevelStep lastStep = currentCustomer.steps[currentCustomer.steps.Count - 1];
        return lastStep.successDocName;
    }

    // 结算并换表情
    public void FinishCustomerInteraction(bool isSuccess)
    {
        if (isSuccess)
        {
            // 1. 显示成功对白
            messageUI.ShowTaskMessage(currentCustomer.finalSuccessMessage);
            
            // 2. 加分
            if (levelManager != null) 
                levelManager.AddStar();

            // 3. 换成“开心”的表情图片
            if (currentCustomer.successCustomerSprite != null)
            {
                customerUIImage.sprite = currentCustomer.successCustomerSprite;
            }
        }
        else
        {
            // 1. 显示失败对白
            messageUI.ShowTaskMessage(currentCustomer.finalFailMessage);

            // 2. 换成“生气/失望”的表情图片
            if (currentCustomer.failureCustomerSprite != null)
            {
                customerUIImage.sprite = currentCustomer.failureCustomerSprite;
            }
        }

        // 3. 离开
        CustomerLeaveAndRespawn();
    }

    // ============================================================
    // 9. 离开与重生流程
    // ============================================================
    public void CustomerLeaveAndRespawn()
    {
        StartCoroutine(LeaveAndRespawnRoutine());
    }

    IEnumerator LeaveAndRespawnRoutine()
    {
        // 1. 停留 3 秒展示结果和表情
        yield return new WaitForSeconds(5f);

        // 2. 隐藏气泡
        messageUI.HideAll(); 
        
        // 3. 往回走
        controller.SetTarget(backPoint.anchoredPosition);

        // 4. 等待离开屏幕
        while (Vector2.Distance(customerUIImage.rectTransform.anchoredPosition, backPoint.anchoredPosition) > 1f)
        {
            yield return null;
        }

        // 5. 禁用物体
        customerUIImage.gameObject.SetActive(false);
        
        // 6. 清理背包和面板
        BackpackStorage.Instance.ClearAll(); 
        if (backpackUI != null) backpackUI.RefreshView();
        if (orderPanel != null) orderPanel.ResetPanel();

        // 7. 逻辑切换到下一位
        levelManager.GoToNextCustomer();

        // 8. 检查游戏是否结束
        if (levelManager.GetCurrentCustomer() != null)
        {
            // 还有人，继续生成
            SpawnCustomerFromLevel();
        }
        else
        {
            // 没人了，通关！
            Debug.Log("Level All Done!");
            levelManager.FinishLevel();
        }
    }
}