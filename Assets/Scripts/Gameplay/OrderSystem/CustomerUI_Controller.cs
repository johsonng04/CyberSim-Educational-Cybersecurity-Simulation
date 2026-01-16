using UnityEngine;

//walk
public class CustomerUI_Controller : MonoBehaviour
{
    public float moveSpeed = 300f;

    private RectTransform rect;
    private Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetTarget(Vector2 pos)
    {
        targetPos = pos;
    }

    void Update()
    {
        rect.anchoredPosition = Vector2.MoveTowards(
            rect.anchoredPosition,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }
}
