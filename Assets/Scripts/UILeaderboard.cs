using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
[System.Serializable]
public class LeaderboardEntry
{
    public string Name;
    public int Position;
    public float DistanceToFinish;
    public float Time;
    public bool IsLocal;
}
public class UILeaderboard : NetworkBehaviour
{
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI PositionText;
    public TextMeshProUGUI LeaderboardText;

    private PlayerMovement localPlayer;

    public override void Spawned()
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
        if (!Runner || SceneManager.GetActiveScene().name != "RaceScene") return;

        if (localPlayer != null)
        {
            float speedKmh = localPlayer.CurrentSpeed * 3.6f;
            SpeedText.text = $"Speed: {speedKmh:F0} km/h";

            if (RaceManager.Instance != null)
            {
                int pos = RaceManager.Instance.GetPlayerPosition(localPlayer);
                int total = RaceManager.Instance.GetTotalPlayers();
                PositionText.text = $"Position: {pos} / {total}";
            }
        }

        if (RaceManager.Instance?.Checkpoints != null)
        {
            var cps = RaceManager.Instance.Checkpoints;
            Vector3 finishPos = cps.Length > 0 ? cps[cps.Length - 1].position : Vector3.zero;
            UpdateLeaderboard(RaceManager.Instance.GetPlayers(), cps, finishPos);
        }
    }

    private void UpdateLeaderboard(List<PlayerMovement> players, Transform[] checkpoints, Vector3 finishPos)
    {
        var sorted = players
            .OrderBy(p => CalculateDistanceToFinish(p, checkpoints))
            .Select((p, index) =>
            {
                return new LeaderboardEntry
                {
                    Name = p.PlayerName,
                    Position = index + 1,
                    DistanceToFinish = CalculateDistanceToFinish(p, checkpoints),
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


    private float CalculateDistanceToFinish(PlayerMovement player, Transform[] checkpoints)
    {
        float distance = 0f;
        int currentIndex = Mathf.Clamp(player.CurrentCheckpointIndex + 1, 0, checkpoints.Length - 1);

        // distance from the player's position to the next checkpoint
        if (checkpoints.Length > 0 && currentIndex < checkpoints.Length)
        {
            distance += Vector3.Distance(player.transform.position, checkpoints[currentIndex].position);
        }

        // distance from all remaining checkpoints to the finish
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
