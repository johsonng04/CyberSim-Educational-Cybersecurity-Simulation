using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelCompletePanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panelRoot;
    public Image[] stars; 
    
    // ⭐ 新增：我们需要引用 Next Level 按钮来控制它的显示/隐藏
    public Button nextLevelButton; 

    [Header("Sprites")]
    public Sprite starFilled; 
    public Sprite starEmpty;  

    private void Start()
    {
        // 主动把自己交给 LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.completePanel = this;
        }

        // 初始隐藏
        panelRoot.SetActive(false);
    }

    // ============================================================
    // 显示结算
    // ============================================================
    public void ShowResults(int starCount)
    {
        panelRoot.SetActive(true);

        // 1. 处理星星逻辑
        starCount = Mathf.Clamp(starCount, 0, 3);
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < starCount)
                stars[i].sprite = starFilled;
            else
                stars[i].sprite = starEmpty;
        }

        // 2. ⭐ 处理 Next Level 按钮逻辑
        if (LevelManager.Instance != null && nextLevelButton != null)
        {
            // 计算下一关的索引
            int nextIndex = LevelManager.Instance.currentLevelIndex + 1;
            int totalLevels = LevelManager.Instance.levels.Count;

            // 如果 下一关索引 < 总关卡数，说明还有关卡，显示按钮
            // 否则，说明已经是最后一关了，隐藏按钮
            if (nextIndex < totalLevels)
            {
                nextLevelButton.gameObject.SetActive(true);
            }
            else
            {
                // 最后一关：隐藏 Next Level 按钮
                nextLevelButton.gameObject.SetActive(false);
                Debug.Log("已到达最后一关，隐藏 Next Level 按钮");
            }
        }
    }
}