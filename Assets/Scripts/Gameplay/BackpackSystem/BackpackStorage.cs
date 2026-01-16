using UnityEngine;
using System.Collections.Generic;

public class BackpackStorage : MonoBehaviour
{
    public static BackpackStorage Instance { get; private set; }

    // 定义单个文件的数据结构
    [System.Serializable]
    public class BackpackDocument
    {
        public string docName;
        public Sprite icon;
        
        // 详情数据
        public DetailType detailType;
        public string detailText;
        public Sprite detailImage;

        // 任务类型数据
        public string taskType;
        public string subType;
    }

    // ⭐ 核心：文件列表
    public List<BackpackDocument> documentList = new List<BackpackDocument>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // =========================================================
    // 1. 添加文件
    // =========================================================
    public void AddDocument(string name, Sprite icon, DetailType dType, string dText, Sprite dImg, string tType, string tSub)
    {
        BackpackDocument newDoc = new BackpackDocument();
        newDoc.docName = name;
        newDoc.icon = icon;
        newDoc.detailType = dType;
        newDoc.detailText = dText;
        newDoc.detailImage = dImg;
        newDoc.taskType = tType;
        newDoc.subType = tSub;

        documentList.Add(newDoc);
    }

    // =========================================================
    // 2. ⭐ 修复：移除文件 (BackpackSlot 需要调用这个)
    // =========================================================
    public void RemoveDocument(int index)
    {
        if (index >= 0 && index < documentList.Count)
        {
            documentList.RemoveAt(index);
        }
    }

    // =========================================================
    // 3. 检查文件是否存在 (Customer_Manager 需要调用这个)
    // =========================================================
    public bool HasDocument(string name)
    {
        foreach (var doc in documentList)
        {
            if (doc.docName == name)
                return true;
        }
        return false;
    }

    // =========================================================
    // 4. 清空背包
    // =========================================================
    public void ClearAll()
    {
        documentList.Clear();
    }
}