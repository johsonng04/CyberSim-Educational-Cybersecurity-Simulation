using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("UI Reference (自动注册)")]
    public LevelCompletePanel completePanel; 

    [Header("All Levels")]
    public List<LevelData> levels = new List<LevelData>();

    [Header("Runtime State")]
    public int currentLevelIndex = 0;
    public int currentCustomerIndex = 0;
    public int currentEarnedStars = 0;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // ============================================================
    // ⭐ 核心修改：加载指定关卡
    // ============================================================
    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count) return;

        currentLevelIndex = index;
        currentCustomerIndex = 0;
        currentEarnedStars = 0;

        SceneManager.LoadScene("Game_Scene");
    }

    // ============================================================
    // 结算逻辑 (增加存星星)
    // ============================================================
    public void AddStar()
    {
        currentEarnedStars++;
    }

    public void FinishLevel()
    {
        // ⭐ 保存星星到 GameData
        GameDataManager.Instance.UpdateLevelStars(currentLevelIndex, currentEarnedStars);

        if (completePanel != null)
            completePanel.ShowResults(currentEarnedStars);
    }

    public void GoToNextLevel()
    {
        // 自动进下一关
        LoadLevel(currentLevelIndex + 1);
    }

    // ... (Restart, ReturnMenu 等保持不变) ...
    public void RestartCurrentLevel() { LoadLevel(currentLevelIndex); }
    public void ReturnToMenu() { SceneManager.LoadScene("Start_Scene"); }
    
    public void GoToNextCustomer() { currentCustomerIndex++; }
    public LevelData CurrentLevel { get { return levels[currentLevelIndex]; } }
    public LevelCustomer GetCurrentCustomer() {
        if (currentCustomerIndex >= CurrentLevel.customers.Count) return null;
        return CurrentLevel.customers[currentCustomerIndex];
    }
}