using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;
    public float smoothTime = 0.1f;

    private Vector3 defaultScale; // 记录原始大小
    private Vector3 targetScale;
    private Vector3 currentVelocity;
    
    // 增加一个标记，确保初始化完成
    private bool isInitialized = false;

    void Awake() // ⭐ 改用 Awake，比 Start 更早运行
    {
        InitializeScale();
    }

    void OnEnable() // ⭐ 每次物体激活时，再次检查，防止数据丢失
    {
        InitializeScale();
        // 激活时重置目标，防止它卡在上次缩放的状态
        targetScale = defaultScale;
        transform.localScale = defaultScale;
    }

    void InitializeScale()
    {
        if (isInitialized) return;

        // 获取当前缩放
        Vector3 current = transform.localScale;

        // ⭐ 关键修复：如果当前缩放是 0 (说明可能出BUG了或者还没初始化好)
        // 我们就强制认为它的默认大小应该是 (1, 1, 1)
        if (current.x == 0 || current.y == 0)
        {
            defaultScale = Vector3.one;
        }
        else
        {
            defaultScale = current;
        }

        targetScale = defaultScale;
        isInitialized = true;
    }

    void Update()
    {
        // 平滑缩放
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref currentVelocity, smoothTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsInteractable())
            targetScale = defaultScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = defaultScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable())
            targetScale = defaultScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.hovered.Contains(gameObject))
            targetScale = defaultScale * hoverScale;
        else
            targetScale = defaultScale;
    }

    void OnDisable()
    {
        // 禁用时瞬间恢复大小，不留残影
        transform.localScale = defaultScale;
    }

    // 辅助检查：按钮是否可交互
    private bool IsInteractable()
    {
        var btn = GetComponent<UnityEngine.UI.Button>();
        return btn != null && btn.interactable;
    }
}