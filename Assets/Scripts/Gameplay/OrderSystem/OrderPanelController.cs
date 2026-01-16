using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class OrderPanelController : MonoBehaviour
{
    [Header("References")]
    public BackpackUI backpackUI;
    public Customer_Manager customerManager;

    [Header("UI Components")]
    public Button getButton;
    public Animator getButtonAnimator; 
    public Button deleteButton;
    public Button confirmButton; 
    
    [Header("Input Group")]
    public GameObject inputGroup; 
    public Image inputIcon;
    public TMP_Text inputName;
    public Animator inputAnimator; 

    [Header("Result UI")]
    public TMP_Text resultText;   
    public Animator resultAnimator; 

    // 运行时数据
    private BackpackStorage.BackpackDocument currentDoc;

    private void Start()
    {
        ImmediateReset();

        if (getButton) getButton.onClick.AddListener(OnGetClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (confirmButton) confirmButton.onClick.AddListener(OnConfirmClicked);
    }

    // ============================================================
    // 1. 获取文件 (执行交替动画：Button 退 -> Input 进)
    // ============================================================
    void OnGetClicked()
    {
        if (backpackUI != null)
        {
            backpackUI.EnterSelectionMode((selectedDoc) => {
                // 选完文件后，启动交替协程
                StartCoroutine(SwitchToInputRoutine(selectedDoc));
            });
        }
    }

    // ⭐ 新增协程：处理 "按钮滑出 -> Input滑入" 的序列
    IEnumerator SwitchToInputRoutine(BackpackStorage.BackpackDocument doc)
    {
        // 1. 锁定按钮，防止连点
        getButton.interactable = false;

        // 2. Get Button 滑出
        if (getButtonAnimator != null)
            getButtonAnimator.SetTrigger("Hide");

        // 3. 等待按钮滑出去 (0.5秒)
        yield return new WaitForSeconds(0.5f);

        // 4. 隐藏按钮，准备 Input 数据
        getButton.gameObject.SetActive(false);
        SetInputData(doc);

        // 5. Input Group 激活并滑入
        inputGroup.SetActive(true);
        if (inputAnimator != null)
            inputAnimator.SetTrigger("Show");
    }

    // 辅助函数：只负责填数据，不负责显隐
    void SetInputData(BackpackStorage.BackpackDocument doc)
    {
        currentDoc = doc;
        confirmButton.interactable = true; 

        inputIcon.sprite = doc.icon;
        inputName.text = doc.docName;

        inputIcon.GetComponent<Button>().onClick.RemoveAllListeners();
        inputIcon.GetComponent<Button>().onClick.AddListener(() => {
             DocumentDetailPanel.Instance.Open(doc.docName, doc.detailType, doc.detailText, doc.detailImage);
        });
    }

    // ============================================================
    // 2. 删除 / 重置 (执行交替动画：Input 退 -> Button 进)
    // ============================================================
    void OnDeleteClicked()
    {
        ResetPanel();
    }

    public void ResetPanel()
    {
        confirmButton.interactable = false;
        if (deleteButton) deleteButton.interactable = false;

        bool isInputActive = inputGroup.activeSelf;
        bool isResultActive = (resultAnimator != null && resultAnimator.gameObject.activeSelf);

        if (isInputActive || isResultActive)
        {
            // Input / Result 滑出
            if (isInputActive && inputAnimator != null)
                inputAnimator.SetTrigger("Hide");

            if (isResultActive)
                resultAnimator.SetTrigger("Hide");

            // 启动协程：等它们滚蛋了，Get Button 再出来
            StartCoroutine(WaitAndShowButtonRoutine());
        }
        else
        {
            ImmediateReset();
        }
    }

    // ⭐ 新增协程：处理 "面板滑出 -> 按钮滑入"
    IEnumerator WaitAndShowButtonRoutine()
    {
        // 1. 等待面板滑出
        yield return new WaitForSeconds(0.5f); 

        // 2. 清理旧面板
        currentDoc = null;
        inputGroup.SetActive(false);
        if (resultAnimator != null) resultAnimator.gameObject.SetActive(false);
        if (resultText) resultText.text = "";

        // 3. Get Button 激活并滑入
        getButton.gameObject.SetActive(true);
        getButton.interactable = true; // 恢复点击
        
        if (getButtonAnimator != null)
            getButtonAnimator.SetTrigger("Show");
            
        if (deleteButton) deleteButton.interactable = true;
    }

    private void ImmediateReset()
    {
        currentDoc = null;
        
        inputGroup.SetActive(false);
        
        if (resultAnimator != null)
            resultAnimator.gameObject.SetActive(false);

        // 强制重置按钮状态
        getButton.gameObject.SetActive(true);
        getButton.interactable = true;
        // 如果想让它瞬间回到 Idle，可以在这里 Play("Idle")，或者由 Animator 默认状态处理

        confirmButton.interactable = false;
        if (deleteButton) deleteButton.interactable = true;

        if (resultText) resultText.text = "";
    }

    // ============================================================
    // 3. 确认提交
    // ============================================================
    void OnConfirmClicked()
    {
        if (currentDoc == null) return;

        string correctDocName = customerManager.GetFinalCorrectDocName();
        bool isCorrect = (currentDoc.docName == correctDocName);

        if (isCorrect) resultText.text = "<color=green>CORRECT</color>";
        else resultText.text = "<color=red>WRONG</color>";

        if (resultAnimator != null)
        {
            resultAnimator.gameObject.SetActive(true);
            resultAnimator.SetTrigger("Show");
        }

        customerManager.FinishCustomerInteraction(isCorrect);

        confirmButton.interactable = false;
        deleteButton.interactable = false;
    }
}