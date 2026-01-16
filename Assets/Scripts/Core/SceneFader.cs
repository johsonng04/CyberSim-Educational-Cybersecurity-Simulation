using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Settings")]
    public Image fadeImage;       // 拖入那个黑色的 Image
    public float fadeDuration = 1.5f; // 淡入需要几秒

    private void Start()
    {
        // 游戏开始，立刻执行淡入
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        // 1. 确保图片是显示的，并且是全黑的
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1f; // Alpha = 1 (完全不透明)
        fadeImage.color = color;

        float timer = 0f;

        // 2. 循环减少 Alpha 值
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 使用 Lerp 插值，从 1 变到 0
            float newAlpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            
            color.a = newAlpha;
            fadeImage.color = color;

            yield return null; // 等待下一帧
        }

        // 3. 确保最后完全透明
        color.a = 0f;
        fadeImage.color = color;

        // 4. ⭐ 关键：隐藏物体！
        // 如果不隐藏，虽然它是透明的，但它还是会挡住鼠标点击下面的按钮
        fadeImage.gameObject.SetActive(false);
    }
}