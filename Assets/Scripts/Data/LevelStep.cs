using UnityEngine;
using System;

public enum DetailType
{
    Text,
    Image
}

[Serializable]
public class LevelStep
{
    public string taskType;
    public string subType;

    // ========================================================================
    // 1. INPUT
    // ========================================================================
    [Header("=== STEP INPUT ===")]
    public string inputDocName;
    public Sprite inputIcon;
    public DetailType inputDetailType;
    [TextArea] public string inputDetailText;
    public Sprite inputDetailImage;

    // ========================================================================
    // 2. SUCCESS FEEDBACK (Main Output)
    // ========================================================================
    [Header("=== STEP SUCCESS ===")]
    public string successDocName;
    public Sprite successIcon;
    public DetailType successDetailType;
    [TextArea] public string successDetailText;
    public Sprite successDetailImage;

    // ========================================================================
    // 3. FAIL FEEDBACK (Main Output)
    // ========================================================================
    [Header("=== STEP FAIL ===")]
    public string failDocName;
    public Sprite failIcon;
    public DetailType failDetailType;
    [TextArea] public string failDetailText;
    public Sprite failDetailImage;

    // ========================================================================
    // 4. ANOTHER DOCUMENT 1 (Extra Output) - ⭐ 新增
    // ========================================================================
    [Header("=== ANOTHER DOC 1 ===")]
    public string anotherDoc1Name;
    public Sprite anotherDoc1Icon;
    public DetailType anotherDoc1DetailType;
    [TextArea] public string anotherDoc1Text;
    public Sprite anotherDoc1Image;

    // ========================================================================
    // 5. ANOTHER DOCUMENT 2 (Extra Output) - ⭐ 新增
    // ========================================================================
    [Header("=== ANOTHER DOC 2 ===")]
    public string anotherDoc2Name;
    public Sprite anotherDoc2Icon;
    public DetailType anotherDoc2DetailType;
    [TextArea] public string anotherDoc2Text;
    public Sprite anotherDoc2Image;

    public bool isFinalStep;
}