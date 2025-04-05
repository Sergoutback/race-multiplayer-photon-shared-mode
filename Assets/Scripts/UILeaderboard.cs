using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class LeaderboardEntry
{
    public string Name;
    public int Position;
    public float Speed;
}

public class UILeaderboard : MonoBehaviour
{
    public TextMeshProUGUI LeaderboardText;

    public void UpdateLeaderboard(List<LeaderboardEntry> entries)
    {
        LeaderboardText.text = "";

        foreach (var entry in entries)
        {
            LeaderboardText.text += $"{entry.Position}. {entry.Name} - {entry.Speed * 3.6f:F0} km/h\n";
        }
    }
}