using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI References")]
    public GameObject tutorialRoot;
    // ❌ 删除了 backgroundOverlay
    
    [Header("Buttons (Only for Step 0)")]
    public Button startButton;          
    public Button skipButton;           
    
    [Header("Steps")]
    // 你的 Step 0 里面必须自己带有一个全屏背景图来挡住点击
    public List<GameObject> tutorialSteps; 

    private int currentStepIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.currentLevelIndex == 0)
        {
            StartTutorial();
        }
        else
        {
            tutorialRoot.SetActive(false);
        }
    }

    void StartTutorial()
    {
        tutorialRoot.SetActive(true);
        
        if (startButton)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (skipButton)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        currentStepIndex = 0;
        ShowStep(currentStepIndex);
    }

    void ShowStep(int index)
    {
        // 1. 切换页面内容
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i] != null)
                tutorialSteps[i].SetActive(i == index);
        }

        // 2. 控制按钮显示
        if (index == 0)
        {
            // === Step 0: 询问界面 ===
            if (startButton) startButton.gameObject.SetActive(true);
            if (skipButton) skipButton.gameObject.SetActive(true);
            
            // ⚠️ 注意：这里不再代码控制阻挡了
            // 请确保 Step_0_Ask 这个物体里，有一张全屏的 Image (Raycast Target = true)
            // 这样玩家就点不到后面的游戏了
        }
        else
        {
            // === Step 1+: 教学 ===
            if (startButton) startButton.gameObject.SetActive(false);
            if (skipButton) skipButton.gameObject.SetActive(false);
            
            // 既然你在 Step 里自己做了遮罩 Panel，
            // 只要那些 Panel 没有挡住你要点击的按钮（背包/机器），那就没问题！
        }
    }

    void OnStartClicked()
    {
        AdvanceTutorial();
    }

    void OnSkipClicked()
    {
        tutorialRoot.SetActive(false);
    }

    public void AdvanceTutorial()
    {
        if (!tutorialRoot.activeSelf) return;

        currentStepIndex++;

        if (currentStepIndex < tutorialSteps.Count)
        {
            ShowStep(currentStepIndex);
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        tutorialRoot.SetActive(false);
    }
}