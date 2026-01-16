using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    [Header("🔧 Debug Settings")]
    public bool debugMode = false; // ⭐ 勾选这个，就可以在下面手动改数据，不会被存档覆盖

    [Header("Runtime Data")]
    public List<int> levelStars = new List<int>();
    public int unlockedLevelIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ⭐ 如果开启了调试模式，直接跳过读取，使用 Inspector 里的值
        if (debugMode)
        {
            Debug.LogWarning("⚠️ DEBUG MODE ACTIVE: Using Inspector values. Save/Load disabled.");
            
            // 确保列表已初始化，防止报错
            if (levelStars == null) levelStars = new List<int>();
            return; 
        }

        // --- 正常游戏逻辑 ---
        if (SaveSystem.SaveExists(1))
        {
            Debug.Log("<color=green>[GameDataManager] Found save file. Loading...</color>");
            SaveData loadedData = SaveSystem.LoadFromSlot(1);
            
            if (loadedData != null) ApplySaveData(loadedData);
            else ResetData();
        }
        else
        {
            Debug.Log("<color=yellow>[GameDataManager] No save file found. Creating new data.</color>");
            ResetData();
        }
    }

    public void ResetData()
    {
        levelStars.Clear();
        unlockedLevelIndex = 0;
    }

    public void ApplySaveData(SaveData data)
    {
        if (data.levelStars != null) levelStars = new List<int>(data.levelStars);
        else levelStars = new List<int>();

        unlockedLevelIndex = data.unlockedLevelIndex;
    }

    public SaveData ToSaveData()
    {
        SaveData d = new SaveData();
        d.levelStars = new List<int>(levelStars);
        d.unlockedLevelIndex = unlockedLevelIndex;
        return d;
    }

    public void UpdateLevelStars(int levelIndex, int stars)
    {
        // 确保列表足够长
        while (levelStars.Count <= levelIndex) levelStars.Add(0);

        if (stars > levelStars[levelIndex])
        {
            levelStars[levelIndex] = stars;
        }

        if (levelIndex >= unlockedLevelIndex)
        {
            unlockedLevelIndex = levelIndex + 1;
        }

        // ⭐ 如果是调试模式，不要把测试数据写入硬盘！以免弄坏真的存档
        if (!debugMode)
        {
            SaveSystem.SaveToSlot(1);
        }
        else
        {
            Debug.LogWarning("⚠️ Debug Mode: Data updated in memory but NOT saved to disk.");
        }
    }
    
    public int GetStarsForLevel(int levelIndex)
    {
        if (levelIndex < levelStars.Count) return levelStars[levelIndex];
        return 0;
    }
}