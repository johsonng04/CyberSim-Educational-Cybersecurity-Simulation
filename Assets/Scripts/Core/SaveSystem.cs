using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // 辅助方法：统一获取路径，避免重复写代码
    private static string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");
    }

    // ============================================================
    // 保存 (Save)
    // ============================================================
    public static void SaveToSlot(int slot)
    {
        // 安全检查：防止 GameDataManager 还没初始化就调用
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[SaveSystem] GameDataManager Instance is null! Cannot save.");
            return;
        }

        string path = GetSavePath(slot);
        SaveData data = GameDataManager.Instance.ToSaveData();
        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"<color=green>[SaveSystem] Saved successfully to Slot {slot}</color>\nPath: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save to slot {slot}. Error: {e.Message}");
        }
    }

    // ============================================================
    // 读取 (Load)
    // ============================================================
    public static SaveData LoadFromSlot(int slot)
    {
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] No save file found for Slot {slot}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            Debug.Log($"[SaveSystem] Loaded Slot {slot}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load slot {slot}. The file might be corrupted.\nError: {e.Message}");
            return null;
        }
    }

    // ============================================================
    // 检查是否存在
    // ============================================================
    public static bool SaveExists(int slot)
    {
        string path = GetSavePath(slot);
        return File.Exists(path);
    }

    // ============================================================
    // 删除存档
    // ============================================================
    public static void DeleteSlot(int slot)
    {
        string path = GetSavePath(slot);

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveSystem] Deleted Slot {slot}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to delete slot {slot}. Error: {e.Message}");
        }
    }
}