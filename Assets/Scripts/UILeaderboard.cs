using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Fusion;

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
    [Header("HUD UI")]
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI PositionText;

    [Header("Leaderboard")]
    public TextMeshProUGUI LeaderboardText;

    private PlayerMovement localPlayer;

    private void Start()
    {
        StartCoroutine(FindLocalPlayer());
    }

    private IEnumerator FindLocalPlayer()
    {
        while (localPlayer == null)
        {
            localPlayer = FindObjectsOfType<PlayerMovement>()
                .FirstOrDefault(p => p.HasStateAuthority);

            if (localPlayer == null)
                yield return null;
        }
    }

    private void Update()
    {
        if (localPlayer != null)
        {
            // Velocity
            float speedKmh = localPlayer.CurrentSpeed * 3.6f;
            SpeedText.text = $"Speed: {speedKmh:F0} km/h";

            if (RaceManager.Instance != null)
            {
                int pos = RaceManager.Instance.GetPlayerPosition(localPlayer);
                int total = RaceManager.Instance.GetTotalPlayers();
                PositionText.text = $"Position: {pos} / {total}";
            }
        }

        if (RaceManager.Instance != null && RaceManager.Instance.FinishLine != null)
        {
            UpdateLeaderboard(RaceManager.Instance.GetPlayers(), RaceManager.Instance.Checkpoints, RaceManager.Instance.FinishLine.position);
        }
    }

    private void UpdateLeaderboard(List<PlayerMovement> players, Transform[] checkpoints, Vector3 finishPos)
    {
        var sorted = players
            .OrderBy(p =>
            {
                int nextIndex = Mathf.Clamp(p.CurrentCheckpointIndex + 1, 0, checkpoints.Length - 1);
                Vector3 targetPos = nextIndex < checkpoints.Length ? checkpoints[nextIndex].position : finishPos;
                return Vector3.Distance(p.transform.position, targetPos);
            })
            .Select((p, index) =>
            {
                int nextIndex = Mathf.Clamp(p.CurrentCheckpointIndex + 1, 0, checkpoints.Length - 1);
                Vector3 targetPos = nextIndex < checkpoints.Length ? checkpoints[nextIndex].position : finishPos;

                return new LeaderboardEntry
                {
                    Name = p.PlayerName,
                    Position = index + 1,
                    DistanceToFinish = Vector3.Distance(p.transform.position, targetPos),
                    Time = p.ElapsedTime,
                    IsLocal = p.HasStateAuthority
                };
            }).ToList();

        LeaderboardText.text = "";

        foreach (var entry in sorted)
        {
            string colorTag = entry.IsLocal ? "<color=yellow>" : "";
            string endTag = entry.IsLocal ? "</color>" : "";

            LeaderboardText.text +=
                $"{colorTag}{entry.Position,2}. {entry.Name,-10}  {entry.DistanceToFinish,6:F1} m   {FormatTime(entry.Time)}{endTag}\n";
        }
    }


    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int millis = Mathf.FloorToInt((time * 10f) % 10f);
        return $"{minutes:00}:{seconds:00}.{millis}";
    }
}
