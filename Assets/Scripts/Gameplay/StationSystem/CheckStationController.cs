using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CheckStationController : MonoBehaviour
{
    [Header("References")]
    public BackpackUI backpackUI;
    public Customer_Manager customerManager;

    [Header("Input UI")]
    public GameObject inputGroup;
    public Animator inputAnimator; 
    public Button getButton;
    public Animator getButtonAnimator; 
    public Button deleteButton; 
    public Image inputImage;
    public TMP_Text inputName;
    public Sprite successInputSprite; 

    [Header("Process UI")]
    public Button processButton;
    public Image processBtnImage;
    public Sprite imgProcessNormal;
    public Sprite imgProcessLoading;
    public Sprite imgProcessSuccess;
    public Sprite imgProcessFail;

    [Header("Output UI")]
    public GameObject outputRoot; 
    public Button clearAllOutputButton;

    // Output Slots (Check Station 特有的 3 个槽位)
    [Header("Output Slots")]
    public GameObject out1Group; public Image out1Icon; public TMP_Text out1Name; public Button out1GetBtn;
    public GameObject out2Group; public Image out2Icon; public TMP_Text out2Name; public Button out2GetBtn;
    public GameObject out3Group; public Image out3Icon; public TMP_Text out3Name; public Button out3GetBtn;

    private BackpackStorage.BackpackDocument currentInputDoc;
    private bool isProcessing = false;
    private bool isFeedbackRunning = false; 

    private class OutputData
    {
        public string dName; public Sprite dIcon; public DetailType dType; public string dText; public Sprite dImg; public string tType; public string tSub;
    }

    private void Start()
    {
        ImmediateReset();

        if (getButton) getButton.onClick.AddListener(OnGetClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (processButton) processButton.onClick.AddListener(OnProcessClicked);
        if (clearAllOutputButton) clearAllOutputButton.onClick.AddListener(OnClearAllOutputClicked);
    }

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

    // ============================================================
    // 1. 获取文件
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

    IEnumerator SwitchToInputRoutine(BackpackStorage.BackpackDocument doc)
    {
        if (getButtonAnimator != null) getButtonAnimator.SetTrigger("Hide");
        getButton.interactable = false; 
        yield return new WaitForSeconds(0.5f);
        getButton.gameObject.SetActive(false);
        SetInput(doc);
        if (inputAnimator != null) { inputAnimator.Rebind(); inputAnimator.SetTrigger("Show"); }
    }

    void SetInput(BackpackStorage.BackpackDocument doc)
    {
        currentInputDoc = doc;
        inputGroup.SetActive(true);
        inputImage.sprite = doc.icon;
        inputName.text = doc.docName;
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
    // 2. 删除文件
    // ============================================================
    void OnDeleteClicked()
    {
        if (isProcessing) return;
        StartCoroutine(SwitchToButtonRoutine());
    }

    IEnumerator SwitchToButtonRoutine()
    {
        if (inputAnimator != null) inputAnimator.SetTrigger("Hide");
        yield return new WaitForSeconds(0.5f);
        currentInputDoc = null;
        inputGroup.SetActive(false);
        getButton.gameObject.SetActive(true);
        getButton.interactable = true;
        if (getButtonAnimator != null) getButtonAnimator.SetTrigger("Show");
    }

    // ============================================================
    // 3. 处理逻辑
    // ============================================================
    void OnProcessClicked()
    {
        if (isProcessing) return;
        if (outputRoot.activeSelf) { StartCoroutine(ShowBtnState(imgProcessFail, 1f)); return; }
        if (currentInputDoc == null) { StartCoroutine(ShowBtnState(imgProcessFail, 1f)); return; }
        StartCoroutine(ProcessRoutine());
    }

    IEnumerator ProcessRoutine()
    {
        isProcessing = true;
        if (deleteButton) deleteButton.interactable = false;
        processBtnImage.sprite = imgProcessLoading;
        yield return new WaitForSeconds(2f);
        processBtnImage.sprite = imgProcessSuccess;
        
        GenerateResults(); 

        yield return new WaitForSeconds(1f);
        processBtnImage.sprite = imgProcessNormal;
        
        if (deleteButton) deleteButton.interactable = true;
        isProcessing = false;
    }

    IEnumerator ShowBtnState(Sprite sprite, float time)
    {
        processBtnImage.sprite = sprite;
        yield return new WaitForSeconds(time);
        processBtnImage.sprite = imgProcessNormal;
    }

    // ============================================================
    // 修复版 GenerateResults (CheckStationController)
    // 增加了：完全错误的文件也会生成 Fail Output
    // ============================================================
    void GenerateResults()
    {
        if (customerManager == null) customerManager = FindFirstObjectByType<Customer_Manager>();
        
        // 1. 尝试找配方
        LevelStep matchedStep = customerManager.GetStepInfoByInput(currentInputDoc.docName);
        LevelStep targetStepForOutput = matchedStep; // 用于决定输出什么文件的 Step 数据
        bool isCorrect = false;

        // =========================================================
        // ⭐ 修改点：判定逻辑
        // =========================================================
        if (matchedStep == null)
        {
            // Case A: 文件名完全不对 (垃圾文件) -> 强制判错，并使用当前步骤的数据来生成失败文件
            targetStepForOutput = customerManager.GetCurrentStep();
            isCorrect = false;

            if (targetStepForOutput == null) 
            {
                StartCoroutine(ShowBtnState(imgProcessFail, 1f));
                return;
            }
        }
        else
        {
            // Case B: 文件名对上了 -> 检查是不是 Check 任务
            targetStepForOutput = matchedStep;
            isCorrect = (matchedStep.taskType == "check");
        }

        // =========================================================
        // 生成结果
        // =========================================================

        if (successInputSprite != null) inputImage.sprite = successInputSprite;
        
        List<OutputData> results = new List<OutputData>();

        if (isCorrect) 
        {
            // 成功
            results.Add(new OutputData { dName = targetStepForOutput.successDocName, dIcon = targetStepForOutput.successIcon, dType = targetStepForOutput.successDetailType, dText = targetStepForOutput.successDetailText, dImg = targetStepForOutput.successDetailImage, tType = "Result: Success", tSub = "check" });
        }
        else 
        {
            // 失败 (无论是垃圾文件，还是类型不对，都走这里)
            results.Add(new OutputData { dName = targetStepForOutput.failDocName, dIcon = targetStepForOutput.failIcon, dType = targetStepForOutput.failDetailType, dText = targetStepForOutput.failDetailText, dImg = targetStepForOutput.failDetailImage, tType = "Result: Fail", tSub = "check" });
        }
        
        // 添加干扰项 (使用 targetStep 的数据)
        results.Add(new OutputData { dName = targetStepForOutput.anotherDoc1Name, dIcon = targetStepForOutput.anotherDoc1Icon, dType = targetStepForOutput.anotherDoc1DetailType, dText = targetStepForOutput.anotherDoc1Text, dImg = targetStepForOutput.anotherDoc1Image, tType = "check", tSub = "check" });
        results.Add(new OutputData { dName = targetStepForOutput.anotherDoc2Name, dIcon = targetStepForOutput.anotherDoc2Icon, dType = targetStepForOutput.anotherDoc2DetailType, dText = targetStepForOutput.anotherDoc2Text, dImg = targetStepForOutput.anotherDoc2Image, tType = "check", tSub = "check" });

        ShuffleList(results);

        outputRoot.SetActive(true);

        if (results.Count >= 1) SetupOutputSlot(out1Group, out1Icon, out1Name, out1GetBtn, results[0]);
        else out1Group.SetActive(false);

        if (results.Count >= 2) SetupOutputSlot(out2Group, out2Icon, out2Name, out2GetBtn, results[1]);
        else out2Group.SetActive(false);

        if (results.Count >= 3) SetupOutputSlot(out3Group, out3Icon, out3Name, out3GetBtn, results[2]);
        else out3Group.SetActive(false);
    }

    void OnClearAllOutputClicked()
    {
        if (outputRoot != null) outputRoot.SetActive(false);
    }

    // ============================================================
    // ⭐ 修改点 2: 绑定按钮事件 (修复后的裁判逻辑)
    // ============================================================
    void SetupOutputSlot(GameObject group, Image icon, TMP_Text name, Button getBtn, OutputData data)
    {
        if (data.dIcon == null) { group.SetActive(false); return; }
        
        group.SetActive(true); 
        icon.sprite = data.dIcon;
        name.text = data.dName;

        Button iconBtn = icon.GetComponent<Button>();
        if (iconBtn != null)
        {
            iconBtn.onClick.RemoveAllListeners();
            iconBtn.onClick.AddListener(() => {
                if (DocumentDetailPanel.Instance != null)
                    DocumentDetailPanel.Instance.Open(data.dName, data.dType, data.dText, data.dImg);
            });
        }

        getBtn.onClick.RemoveAllListeners();
        getBtn.onClick.AddListener(() => {
            // A. 检查背包
            if (BackpackStorage.Instance.HasDocument(data.dName)) 
            {
                StartCoroutine(ButtonFeedbackRoutine(getBtn)); 
                return; 
            }

            // B. 放入背包
            BackpackStorage.Instance.AddDocument(data.dName, data.dIcon, data.dType, data.dText, data.dImg, data.tType, data.tSub);
            if (backpackUI != null) backpackUI.RefreshView();

            // C. ⭐ 关键修复：告诉 Manager 尝试升级 Step
            if (customerManager != null)
                customerManager.TryAdvanceStep(data.dName);
        });
    }

    IEnumerator ButtonFeedbackRoutine(Button targetBtn)
    {
        if (isFeedbackRunning) yield break;
        if (targetBtn == null) yield break;

        isFeedbackRunning = true;

        Image btnImg = targetBtn.image;
        
        TMP_Text tmpText = targetBtn.GetComponentInChildren<TMP_Text>(); 
        Text legacyText = targetBtn.GetComponentInChildren<Text>(); 

        Color originalColor = Color.white; 
        if (btnImg != null) originalColor = btnImg.color;
        
        string originalString = "";
        if (tmpText != null) originalString = tmpText.text;
        else if (legacyText != null) originalString = legacyText.text;

        if (btnImg != null) btnImg.color = Color.red; 
        
        if (tmpText != null) tmpText.text = "In Bag!";
        else if (legacyText != null) legacyText.text = "In Bag!";

        yield return new WaitForSeconds(0.5f);

        if (btnImg != null) btnImg.color = originalColor;
        
        if (tmpText != null) tmpText.text = originalString;
        else if (legacyText != null) legacyText.text = originalString;

        isFeedbackRunning = false;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++) { T temp = list[i]; int randomIndex = Random.Range(i, list.Count); list[i] = list[randomIndex]; list[randomIndex] = temp; }
    }

    public void ResetStation()
    {
        ImmediateReset();
    }

    private void ImmediateReset()
    {
        currentInputDoc = null;
        inputGroup.SetActive(false);
        outputRoot.SetActive(false);
        getButton.gameObject.SetActive(true);
        getButton.interactable = true;
        
        if (getButtonAnimator != null) { getButtonAnimator.Rebind(); getButtonAnimator.Update(0f); }
        if (inputAnimator != null) { inputAnimator.Rebind(); }
            
        processBtnImage.sprite = imgProcessNormal;
        isProcessing = false;
        if (deleteButton) deleteButton.interactable = true;
        isFeedbackRunning = false;
    }
}