using UnityEngine;

[CreateAssetMenu(fileName = "TaskDatabase", menuName = "Game Data/Task Database")]
public class TaskDatabase : ScriptableObject
{
    [Header("Kill Station Subtypes")]
    public string[] virusTypes = {
        "Worm",
        "Trojan",
        "Ransomware",
        "Spyware"
    };

    [Header("Check Station Subtypes")]
    public string[] checkTypes = {
        "Check" // 只有一个默认选项
    };

    [Header("Protect Station Subtypes")]
    public string[] protectTypes = {
        "7zip",
        "Firewall",
        "Antivirus",
        "Anti-Spyware Tools",
        "IDS/IPS"
    };

    [Header("Encrypt Station Subtypes")]
    public string[] encryptTypes = {
        "Base64",
        "Hex",
        "Rot13",
        "Caesar cipher",
        "Morse"
    };

    [Header("Decrypt Station Subtypes")]
    public string[] decryptTypes = {
        "Base64",
        "Hex",
        "Rot13",
        "Caesar cipher",
        "Morse"
    };
}