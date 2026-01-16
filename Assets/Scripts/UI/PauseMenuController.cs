using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanelRoot;
    public Button resumeButton; 
    // 注意：这里保留变量是为了方便你在 Inspector 里看，
    // 但代码不再通过 AddListener 控制它们了，全靠你在 Unity 按钮上拖拽。
    public Button restartButton; 
    public Button menuButton;
    
    public TMP_Text levelText; 
    public Button openPauseButton;

    private bool isPaused = false;

    private void Start()
    {
        pausePanelRoot.SetActive(false);

        // ✅ 只保留 Resume (继续游戏) 的逻辑，因为它是立即执行的
        resumeButton.onClick.AddListener(ResumeGame);

        // ❌ 删除 restartButton 和 menuButton 的 AddListener
        // 让 Unity Inspector 里的 OnClick() 去控制 Loading 跳转

        if (openPauseButton != null)
            openPauseButton.onClick.AddListener(PauseGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanelRoot.SetActive(true);
        Time.timeScale = 0f; 

        if (levelText != null && LevelManager.Instance != null)
        {
            int displayLevel = LevelManager.Instance.currentLevelIndex + 1;
            levelText.text = $"LEVEL {displayLevel}";
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanelRoot.SetActive(false);
        Time.timeScale = 1f; 
    }


}