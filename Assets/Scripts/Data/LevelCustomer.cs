using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelCustomer
{
    [Header("Customer Info")]
    public string customerName;
    public Sprite customerSprite; // 默认图片
    [TextArea] public string message; 

    // ⭐ 新增：结算时的表情图片
    [Header("Result Images")] 
    public Sprite successCustomerSprite; // 成功时的图片 (比如开心的脸)
    public Sprite failureCustomerSprite; // 失败时的图片 (比如生气的脸)

    [Header("Steps")]
    public List<LevelStep> steps = new List<LevelStep>();

    [Header("Final Settlement Messages")]
    [TextArea] public string finalSuccessMessage; 
    [TextArea] public string finalFailMessage;    
}