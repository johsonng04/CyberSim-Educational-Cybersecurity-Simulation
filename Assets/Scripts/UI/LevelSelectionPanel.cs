using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectionPanel : MonoBehaviour
{
    [Header("Navigation")]
    public TMP_Dropdown levelDropdown;  // 左侧选关下拉菜单
    public Button playButton;           // 开始游戏按钮
    public Button closeButton;          // 关闭面板按钮
    public GameObject rootPanel;        // 也就是这个面板的内容父物体

    [Header("Loading Settings")]
    public GameObject loadingPanel;     // 拖入 Panel_Loading
    public Animator loadingAnimator;    // ⭐ 拖入 Panel_Loading 上的 Animator 组件
    public float minLoadTime = 2f;      // 随机等待的最短时间
    public float maxLoadTime = 5f;      // 随机等待的最长时间

    [Header("Level Info UI")]
    public TMP_Text levelNumberText;    // 显示 "01", "02" 的数字文本
    public Image[] starImages;          // 3颗星星的 Image
    public Sprite starOn;               // 亮星星图
    public Sprite starOff;              // 暗星星图

    [Header("Customer Previews")]
    public List<CustomerSlotUI> customerSlots; // 3个客人展示位的列表

    // 定义简单的类来管理客人UI槽位
    [System.Serializable]
    public class CustomerSlotUI
    {
        public Image faceImage;
        public TMP_Text nameText;
        public GameObject root; // 整个槽位的父物体 (用来隐藏/显示)
    }

    private int selectedLevelIndex = 0;

    private void Start()
    {
        // 1. 初始化下拉菜单内容
        InitDropdown();

        // 2. 绑定按钮和事件
        levelDropdown.onValueChanged.AddListener(OnLevelChanged);
        playButton.onClick.AddListener(OnPlayClicked);
        
        // 关闭按钮逻辑：隐藏内部内容 (外部由 Start_Scene_Script 控制父物体开关)
        closeButton.onClick.AddListener(() => rootPanel.SetActive(false));

        // 3. 确保一开始 Loading 是关的
        if (loadingPanel) loadingPanel.SetActive(false);

        // 4. 默认显示第1关数据
        OnLevelChanged(0);
    }

    private void OnEnable()
    {
        // ⭐ 关键修复：每次打开面板时，确保内容区域是可见的
        if (rootPanel != null) rootPanel.SetActive(true);

        // 刷新当前选中的关卡数据 (以防星星更新了)
        if (levelDropdown != null) OnLevelChanged(levelDropdown.value);

        // 重置 Loading 状态
        if (loadingPanel) loadingPanel.SetActive(false);
        playButton.interactable = true;
        closeButton.interactable = true;
    }

    // ============================================================
    // 初始化下拉菜单
    // ============================================================
    void InitDropdown()
    {
        levelDropdown.ClearOptions();
        List<string> options = new List<string>();

        // 从 LevelManager 获取关卡总数
        int totalLevels = LevelManager.Instance.levels.Count;
        for (int i = 0; i < totalLevels; i++)
        {
            options.Add($"Level {i + 1}");
        }

        levelDropdown.AddOptions(options);
    }

    // ============================================================
    // 关卡切换逻辑 (刷新 UI)
    // ============================================================
    void OnLevelChanged(int index)
    {
        // ⭐【修复开始】安全检查：防止 LevelManager 还没准备好就调用
        if (LevelManager.Instance == null) 
        {
            Debug.LogWarning("LevelSelectionPanel: LevelManager 尚未初始化，跳过刷新。");
            return;
        }

        if (LevelManager.Instance.levels == null || index >= LevelManager.Instance.levels.Count)
        {
            // 防止索引越界（比如下拉菜单有选项，但 Manager 里没数据）
            return; 
        }
        // ⭐【修复结束】

        selectedLevelIndex = index;
        LevelData data = LevelManager.Instance.levels[index];

        // A. 更新关卡数字
        if (levelNumberText != null)
            levelNumberText.text = data.levelNumber.ToString();

        // B. 更新星星显示
        if (GameDataManager.Instance != null) // 加个判断防止 GameDataManager 也没准备好
        {
            int starsEarned = GameDataManager.Instance.GetStarsForLevel(index);
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null) 
                    starImages[i].sprite = (i < starsEarned) ? starOn : starOff;
            }
        }

        // C. 更新客人预览
        for (int i = 0; i < customerSlots.Count; i++)
        {
            if (i < data.customers.Count)
            {
                customerSlots[i].root.SetActive(true);
                customerSlots[i].faceImage.sprite = data.customers[i].customerSprite;
                customerSlots[i].nameText.text = data.customers[i].customerName;
            }
            else
            {
                customerSlots[i].root.SetActive(false);
            }
        }

        // D. 检查解锁状态
        if (GameDataManager.Instance != null)
        {
            int unlockedIndex = GameDataManager.Instance.unlockedLevelIndex;
            bool isLocked = index > unlockedIndex;

            if (isLocked)
            {
                playButton.interactable = false;
                TMP_Text btnText = playButton.GetComponentInChildren<TMP_Text>();
                if (btnText) btnText.text = "LOCKED";
            }
            else
            {
                playButton.interactable = true;
                TMP_Text btnText = playButton.GetComponentInChildren<TMP_Text>();
                if (btnText) btnText.text = "PLAY";
            }
        }
    }
    
    // ============================================================
    // 点击 Play 后的逻辑 (伪 Loading)
    // ============================================================
    void OnPlayClicked()
    {
        StartCoroutine(FakeLoadingRoutine());
    }

    IEnumerator FakeLoadingRoutine()
    {
        // 1. 锁住按钮，防止重复点击
        playButton.interactable = false;
        closeButton.interactable = false;

        // 2. 打开 Loading 面板
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);

            // ⭐ 强制重置动画状态 (防止上次播放完卡在最后一帧)
            if (loadingAnimator != null)
            {
                loadingAnimator.Rebind();
                loadingAnimator.Update(0f);
            }
        }

        // 3. 计算时间
        // 随机总时间 (例如 4.5秒)
        float totalTime = Random.Range(minLoadTime, maxLoadTime);
        // 动画预留时间 (我们在 Animation 里做的变身动画时长是 2秒)
        float animationDuration = 2f; 
        // 变身前需要发呆等待的时间
        float waitTimeBeforeAnim = totalTime - animationDuration;

        // 安全检查：如果随机到的时间太短，就不等待了，直接播动画
        if (waitTimeBeforeAnim < 0) waitTimeBeforeAnim = 0;

        Debug.Log($"Loading... Total: {totalTime}s | Wait: {waitTimeBeforeAnim}s | Anim: {animationDuration}s");

        // 4. 阶段一：显示旧图 (Loading...)
        yield return new WaitForSeconds(waitTimeBeforeAnim);

        // 5. 阶段二：触发变身动画 (Pop!)
        if (loadingAnimator != null)
        {
            loadingAnimator.SetTrigger("DoSwap");
        }

        // 6. 阶段三：等待动画播完
        yield return new WaitForSeconds(animationDuration);

        // 7. 时间到，切换场景
        LevelManager.Instance.LoadLevel(selectedLevelIndex);
    }
}