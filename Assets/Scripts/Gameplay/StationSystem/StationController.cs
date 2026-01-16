using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class StationController : MonoBehaviour
{
    [Header("Station Type Settings")]
    public string stationTaskType; 

    [Header("References")]
    public BackpackUI backpackUI;
    public TaskDatabase taskDatabase;
    public Customer_Manager customerManager;

    [Header("Input UI")]
    public GameObject inputGroup;
    public Animator inputAnimator; // ✅ 保留：Input 进出动画
    public Button getButton;
    public Animator getButtonAnimator; // ✅ 保留：Get 按钮进出动画
    public Button deleteButton;
    public Image inputImage;
    public TMP_Text inputName;
    public Sprite successInputSprite; 

    [Header("Control UI")]
    public TMP_Dropdown subtypeDropdown;
    public Button processButton;
    public Image processBtnImage;
    
    [Header("Process Status Images")]
    public Sprite imgProcessNormal;
    public Sprite imgProcessLoading;
    public Sprite imgProcessSuccess;
    public Sprite imgProcessFail;

    [Header("Output UI (Single Slot)")]
    public GameObject outputRoot;
    public Image outIcon;
    public TMP_Text outName;
    public Button outGetBtn;
    public Button deleteOutputButton;

    // 运行时数据
    private BackpackStorage.BackpackDocument currentInputDoc;
    private string currentSelectedSubtype;
    private bool isProcessing = false;

    private void Start()
    {
        InitDropdown();
        
        // 初始重置
        ImmediateReset();

        if (subtypeDropdown) subtypeDropdown.onValueChanged.AddListener(OnDropdownChanged);
        if (getButton) getButton.onClick.AddListener(OnGetClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (processButton) processButton.onClick.AddListener(OnProcessClicked);
        if (deleteOutputButton) deleteOutputButton.onClick.AddListener(OnDeleteOutputClicked);
    }

    // ⭐ 确保面板被激活时状态正确 (防止按钮消失或动画卡住)
    void OnEnable()
    {
        if (currentInputDoc == null && !inputGroup.activeSelf)
        {
            getButton.gameObject.SetActive(true);
            getButton.interactable = true;
            
            if (getButtonAnimator != null) 
            {
                getButtonAnimator.Rebind();
                getButtonAnimator.Update(0f);
                getButtonAnimator.SetTrigger("Show");
            }
        }
    }

    void InitDropdown()
    {
        if (subtypeDropdown == null || taskDatabase == null) return;

        subtypeDropdown.ClearOptions();
        string[] options = new string[0];

        switch (stationTaskType)
        {
            case "kill": options = taskDatabase.virusTypes; break;
            case "protect": options = taskDatabase.protectTypes; break;
            case "encrypt": options = taskDatabase.encryptTypes; break;
            case "decrypt": options = taskDatabase.decryptTypes; break;
        }

        List<string> dropOptions = new List<string>(options);
        subtypeDropdown.AddOptions(dropOptions);

        if (dropOptions.Count > 0) currentSelectedSubtype = dropOptions[0];
    }

    void OnDropdownChanged(int index)
    {
        currentSelectedSubtype = subtypeDropdown.options[index].text;
    }

    // ============================================================
    // 1. 获取文件 (带动画)
    // ============================================================
    void OnGetClicked()
    {
        if (backpackUI != null)
        {
            backpackUI.EnterSelectionMode((selectedDoc) => {
                StartCoroutine(SwitchToInputRoutine(selectedDoc));
            });
        }
    }

    // ⭐ 协程：Get Button 退场 -> Input 进场
    IEnumerator SwitchToInputRoutine(BackpackStorage.BackpackDocument doc)
    {
        // 1. Get Button 播放退场动画
        if (getButtonAnimator != null) getButtonAnimator.SetTrigger("Hide");
        getButton.interactable = false; 
        
        // 等待动画 (0.5s)
        yield return new WaitForSeconds(0.5f);

        // 2. 切换显示
        getButton.gameObject.SetActive(false);
        SetInput(doc);

        // 3. Input 播放进场动画
        if (inputAnimator != null)
        {
            inputAnimator.Rebind();
            inputAnimator.SetTrigger("Show");
        }
    }

    void SetInput(BackpackStorage.BackpackDocument doc)
    {
        currentInputDoc = doc;
        
        inputGroup.SetActive(true);
        inputImage.sprite = doc.icon;
        inputName.text = doc.docName;

        // 确保删除按钮可用
        if (deleteButton) deleteButton.interactable = true;

        Button iconBtn = inputImage.GetComponent<Button>();
        if (iconBtn != null)
        {
            iconBtn.onClick.RemoveAllListeners();
            iconBtn.onClick.AddListener(() => {
                 if (DocumentDetailPanel.Instance != null)
                    DocumentDetailPanel.Instance.Open(doc.docName, doc.detailType, doc.detailText, doc.detailImage);
            });
        }
    }

    // ============================================================
    // 2. 删除文件 (带动画)
    // ============================================================
    void OnDeleteClicked()
    {
        // ⭐ 安全拦截：如果正在处理，禁止删除
        if (isProcessing) return;

        StartCoroutine(SwitchToButtonRoutine());
    }

    // ⭐ 协程：Input 退场 -> Get Button 进场
    IEnumerator SwitchToButtonRoutine()
    {
        // 1. Input 播放退场动画
        if (inputAnimator != null) inputAnimator.SetTrigger("Hide");
        
        // 等待动画 (0.5s)
        yield return new WaitForSeconds(0.5f);

        // 2. 清理数据并切换显示
        currentInputDoc = null;
        inputGroup.SetActive(false);

        getButton.gameObject.SetActive(true);
        getButton.interactable = true;

        // 3. Get Button 播放进场动画
        if (getButtonAnimator != null) getButtonAnimator.SetTrigger("Show");
    }

    // ============================================================
    // 3. 处理逻辑
    // ============================================================
    void OnProcessClicked()
    {
        if (isProcessing) return;

        if (outputRoot.activeSelf)
        {
            StartCoroutine(ShowBtnState(imgProcessFail, 1f));
            return;
        }

        if (currentInputDoc == null)
        {
            StartCoroutine(ShowBtnState(imgProcessFail, 1f));
            return;
        }

        StartCoroutine(ProcessRoutine());
    }

    IEnumerator ProcessRoutine()
    {
        isProcessing = true;
        
        // ⭐ 禁用删除按钮 (变灰)
        if (deleteButton) deleteButton.interactable = false;

        processBtnImage.sprite = imgProcessLoading;
        
        yield return new WaitForSeconds(2f);
        
        processBtnImage.sprite = imgProcessSuccess;
        GenerateResults();
        
        yield return new WaitForSeconds(1f);
        
        processBtnImage.sprite = imgProcessNormal;
        
        // ⭐ 恢复删除按钮
        if (deleteButton) deleteButton.interactable = true;
        isProcessing = false;
    }

    IEnumerator ShowBtnState(Sprite sprite, float time)
    {
        processBtnImage.sprite = sprite;
        yield return new WaitForSeconds(time);
        processBtnImage.sprite = imgProcessNormal;
    }

    void OnDeleteOutputClicked()
    {
        if (outputRoot != null)
            outputRoot.SetActive(false);
    }

    // ============================================================
    // 修复版 GenerateResults (StationController)
    // 增加了：完全错误的文件也会生成 Fail Output
    // ============================================================
    void GenerateResults()
    {
        if (customerManager == null) customerManager = FindFirstObjectByType<Customer_Manager>();
        
        // 1. 尝试找配方
        LevelStep matchedStep = customerManager.GetStepInfoByInput(currentInputDoc.docName);

        // =========================================================
        // ⭐ 修改点：如果找不到配方 (matchedStep == null)
        // =========================================================
        if (matchedStep == null)
        {
            Debug.Log("Input Name mismatch. Generating Default Fail Document.");
            
            // 获取玩家当前所在的步骤 (比如玩家在 Step 2，就给他 Step 2 的失败文件)
            LevelStep currentStep = customerManager.GetCurrentStep();
            
            if (currentStep != null)
            {
                outputRoot.SetActive(true);
                // 强制生成 Fail 结果
                SetupOutputSlot(currentStep.failDocName, currentStep.failIcon, 
                    currentStep.failDetailType, currentStep.failDetailText, currentStep.failDetailImage, 
                    "Result: Fail", stationTaskType, currentSelectedSubtype);
            }
            else
            {
                // 极端情况：连当前步骤都没有，只能闪烁红灯
                StartCoroutine(ShowBtnState(imgProcessFail, 1f));
            }
            return; // 结束
        }

        // =========================================================
        // 下面是：配方找到了，检查机器类型和操作是否正确
        // =========================================================
        
        bool isTaskTypeMatch = (stationTaskType == matchedStep.taskType);
        bool isDropdownMatch = (currentSelectedSubtype == matchedStep.subType);

        bool isCorrect = isTaskTypeMatch && isDropdownMatch;

        if (successInputSprite != null)
            inputImage.sprite = successInputSprite;

        outputRoot.SetActive(true);

        if (isCorrect)
        {
            SetupOutputSlot(matchedStep.successDocName, matchedStep.successIcon, 
                matchedStep.successDetailType, matchedStep.successDetailText, matchedStep.successDetailImage, 
                "Result: Success", stationTaskType, currentSelectedSubtype);
        }
        else
        {
            SetupOutputSlot(matchedStep.failDocName, matchedStep.failIcon, 
                matchedStep.failDetailType, matchedStep.failDetailText, matchedStep.failDetailImage, 
                "Result: Fail", stationTaskType, currentSelectedSubtype);
        }
    }

    // ============================================================
    // ⭐ 修改点 2: 输出槽 (裁判逻辑)
    // ============================================================
    void SetupOutputSlot(string dName, Sprite dIcon, DetailType dType, string dText, Sprite dImg, 
                          string docNamePrefix, string tType, string tSub)
    {
        if (dIcon == null)
        {
            outputRoot.SetActive(false);
            return;
        }

        outIcon.sprite = dIcon;
        outName.text = dName;

        Button iconBtn = outIcon.GetComponent<Button>();
        if (iconBtn != null)
        {
            iconBtn.onClick.RemoveAllListeners();
            iconBtn.onClick.AddListener(() => {
                if (DocumentDetailPanel.Instance != null) 
                    DocumentDetailPanel.Instance.Open(dName, dType, dText, dImg);
            });
        }

        outGetBtn.onClick.RemoveAllListeners();
        outGetBtn.onClick.AddListener(() => {
            if (BackpackStorage.Instance.HasDocument(dName)) return;

            BackpackStorage.Instance.AddDocument(dName, dIcon, dType, dText, dImg, tType, tSub);
            if (backpackUI != null) backpackUI.RefreshView();
            
            // ⭐ NEW: 告诉 Manager 尝试升级
            if (customerManager != null)
                customerManager.TryAdvanceStep(dName);

            outputRoot.SetActive(false); 
        });
    }

    public void ResetStation()
    {
        ImmediateReset();
    }

    // ⭐ 统一重置逻辑，包含 Animator 重置
    private void ImmediateReset()
    {
        currentInputDoc = null;
        
        inputGroup.SetActive(false);
        outputRoot.SetActive(false);
        
        getButton.gameObject.SetActive(true);
        getButton.interactable = true;
        
        // 重置 Animator 状态
        if (getButtonAnimator != null) 
        { 
            getButtonAnimator.Rebind(); 
            getButtonAnimator.Update(0f); 
        }
        if (inputAnimator != null) 
        { 
            inputAnimator.Rebind(); 
        }

        processBtnImage.sprite = imgProcessNormal;
        
        // 防止按钮卡死
        isProcessing = false;
        if (deleteButton) deleteButton.interactable = true;
    }
}