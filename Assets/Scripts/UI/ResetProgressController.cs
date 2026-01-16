using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ⭐ 需要引用这个来重载场景

public class ResetProgressController : MonoBehaviour
{
    [Header("Settings")]
    public int saveSlotIndex = 0; // 你使用的是第几个存档槽？通常是 0
    public Button resetButton;    // 拖入你的“重置/删除存档”按钮

    [Header("Optional")]
    public bool reloadSceneAfterReset = true; // 重置后是否自动刷新场景

    void Start()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }
    }

    public void OnResetClicked()
    {
        // 1. 删除物理存档文件
        SaveSystem.DeleteSlot(saveSlotIndex);

        // 2. 重置内存中的数据 (GameDataManager)
        // 这一步非常重要！如果不做，虽然文件删了，但游戏里显示的还是旧数据
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.unlockedLevelIndex = 0; // 回到第1关
            
            if (GameDataManager.Instance.levelStars != null)
            {
                GameDataManager.Instance.levelStars.Clear(); // 清空星星记录
            }
            
            Debug.Log("<color=red>内存数据已重置</color>");
        }

        // 3. 刷新场景 (可选)
        // 重新加载场景是为了让 UI (比如选关面板的星星、锁) 立即变回“未解锁”状态
        if (reloadSceneAfterReset)
        {
            // 获取当前场景名字并重新加载
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}