using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // ⭐ 新增：记录每一关的星星 (Key: LevelIndex, Value: StarsCount)
    // 例如: 第0关 -> 3星, 第1关 -> 2星
    public List<int> levelStars = new List<int>();

    // 解锁进度：目前解锁到了第几关 (0 = 第1关)
    public int unlockedLevelIndex = 0;

    public SaveData()
    {
        levelStars = new List<int>();
    }
}