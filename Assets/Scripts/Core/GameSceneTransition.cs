using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameSceneTransition : MonoBehaviour
{
    [Header("Loading Components")]
    public GameObject loadingPanel;     // 拖入 Game Scene 里的 Panel_Loading
    public Animator loadingAnimator;    // 拖入 Panel_Loading 上的 Animator
    
    [Header("Time Settings")]
    public float minLoadTime = 3f;      // 随机最小时间
    public float maxLoadTime = 5f;      // 随机最大时间

    // 定义枚举，用来区分我们要去哪里
    public enum TargetDestination
    {
        Restart,
        MainMenu,
        NextLevel
    }

    private void Start()
    {
        // 确保进游戏时 Loading 面板是关的
        if (loadingPanel) loadingPanel.SetActive(false);
    }

    // ============================================================
    // 供按钮调用的公共方法
    // ============================================================

    // 1. 暂停界面/通关界面的 "Restart" 按钮绑定这个
    public void OnRestartBtnClicked()
    {
        StartCoroutine(LoadingRoutine(TargetDestination.Restart));
    }

    // 2. 暂停界面/通关界面的 "Menu" 按钮绑定这个
    public void OnMenuBtnClicked()
    {
        StartCoroutine(LoadingRoutine(TargetDestination.MainMenu));
    }

    // 3. 通关界面的 "Next Level" 按钮绑定这个
    public void OnNextLevelBtnClicked()
    {
        StartCoroutine(LoadingRoutine(TargetDestination.NextLevel));
    }

    // ============================================================
    // 核心 Loading 逻辑 (和 Start Scene 一模一样)
    // ============================================================
    IEnumerator LoadingRoutine(TargetDestination target)
    {
        // 1. 恢复时间 (防止是从暂停界面点的，时间还是停滞状态)
        Time.timeScale = 1f;

        // 2. 打开 Loading 面板
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingAnimator != null)
            {
                loadingAnimator.Rebind();
                loadingAnimator.Update(0f);
            }
        }

        // 3. 计算随机等待时间
        float totalTime = Random.Range(minLoadTime, maxLoadTime);
        float animTime = 2f; // 变身动画时长
        float waitTime = totalTime - animTime;
        if (waitTime < 0) waitTime = 0;

        Debug.Log($"Game Transition -> {target} | Wait: {waitTime}s");

        // 4. 等待 (显示旧图)
        yield return new WaitForSeconds(waitTime);

        // 5. 触发动画 (Pop!)
        if (loadingAnimator != null)
        {
            loadingAnimator.SetTrigger("DoSwap");
        }

        // 6. 等待动画播完
        yield return new WaitForSeconds(animTime);

        // 7. ⭐ 根据目标执行真正的跳转
        switch (target)
        {
            case TargetDestination.Restart:
                LevelManager.Instance.RestartCurrentLevel();
                break;

            case TargetDestination.MainMenu:
                LevelManager.Instance.ReturnToMenu();
                break;

            case TargetDestination.NextLevel:
                // 获取当前关卡索引 + 1
                int nextIndex = LevelManager.Instance.currentLevelIndex + 1;
                // 检查是否超出总关卡数
                if (nextIndex < LevelManager.Instance.levels.Count)
                {
                    LevelManager.Instance.LoadLevel(nextIndex);
                }
                else
                {
                    Debug.Log("已经是最后一关了，直接回主菜单");
                    LevelManager.Instance.ReturnToMenu();
                }
                break;
        }
    }
}