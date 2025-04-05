using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[System.Serializable]
public class LeaderboardEntry
{
    public string Name;
    public int Position;
    public float DistanceToFinish;
    public float Time;
    public bool IsLocal;
}

public class UILeaderboard : MonoBehaviour
{
    public TextMeshProUGUI SpeedText;
    [FormerlySerializedAs("PositionText")] public TextMeshProUGUI CheckpointText;
    public TextMeshProUGUI LeaderboardText;

    private PlayerMovement localPlayer;

    public void SetLocalPlayer(PlayerMovement player)
    {
        localPlayer = player;
    }

    private void Update()
    {
        if (localPlayer == null || RaceManager.Instance == null || RaceManager.Instance.Checkpoints == null)
            return;

        float speedKmh = localPlayer.CurrentSpeed * 3.6f;
        SpeedText.text = $"Speed: {speedKmh:F0} km/h";

        int checkpoint = Mathf.Clamp(localPlayer.CurrentCheckpointIndex + 1, 0, RaceManager.Instance.Checkpoints.Length);
        int total = RaceManager.Instance.Checkpoints.Length;
        CheckpointText.text = $"Checkpoint: {checkpoint} / {total}";

        UpdateLeaderboard(
            FindObjectsOfType<PlayerMovement>().ToList(),
            RaceManager.Instance.Checkpoints,
            RaceManager.Instance.Checkpoints.Last().position
        );
    }



    private void UpdateLeaderboard(List<PlayerMovement> players, Transform[] checkpoints, Vector3 finishPos)
    {
        var sorted = players
            .OrderBy(p => CalculateDistanceToFinish(p, checkpoints))
            .Select((p, index) => new LeaderboardEntry
            {
                Name = p.PlayerName,
                Position = index + 1,
                DistanceToFinish = CalculateDistanceToFinish(p, checkpoints),
                Time = p.ElapsedTime,
                IsLocal = p.HasStateAuthority
            }).ToList();

        LeaderboardText.text = "";

        foreach (var entry in sorted)
        {
            string colorTag = entry.IsLocal ? "<color=yellow>" : "";
            string endTag = entry.IsLocal ? "</color>" : "";

            LeaderboardText.text +=
                $"{colorTag} Position:{entry.Position,2}   {entry.Name,-10}  {entry.DistanceToFinish,6:F1} m   {FormatTime(entry.Time)}{endTag}\n";
        }
    }

    private float CalculateDistanceToFinish(PlayerMovement player, Transform[] checkpoints)
    {
        float distance = 0f;
        int currentIndex = Mathf.Clamp(player.CurrentCheckpointIndex + 1, 0, checkpoints.Length - 1);

        if (checkpoints.Length > 0 && currentIndex < checkpoints.Length)
        {
            distance += Vector3.Distance(player.transform.position, checkpoints[currentIndex].position);
        }

        for (int i = currentIndex; i < checkpoints.Length - 1; i++)
        {
            distance += Vector3.Distance(checkpoints[i].position, checkpoints[i + 1].position);
        }

        return distance;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int millis = Mathf.FloorToInt((time * 10f) % 10f);
        return $"{minutes:00}:{seconds:00}.{millis}";
    }
}
