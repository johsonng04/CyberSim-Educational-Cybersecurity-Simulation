using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game Data/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public List<LevelCustomer> customers = new List<LevelCustomer>();

    [Header("Link Task Database")]
    public TaskDatabase taskDatabase;   // ✅ 序列化字段
}
