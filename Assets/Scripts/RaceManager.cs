using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    private List<PlayerMovement> players = new List<PlayerMovement>();
    public UILeaderboard UI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlayer(PlayerMovement player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
        }
    }

    public int GetPlayerPosition(PlayerMovement localPlayer)
    {
        var sorted = players.OrderByDescending(p => p.transform.position.z).ToList();
        return sorted.IndexOf(localPlayer) + 1;
    }

    public int GetTotalPlayers()
    {
        return players.Count;
    }

    private void Update()
    {
        // Update the UI every second
        if (UI != null)
        {
            var sorted = players
                .OrderByDescending(p => p.transform.position.z)
                .Select((p, index) => new LeaderboardEntry
                {
                    Name = $"Player {index + 1}",
                    Position = index + 1,
                    Speed = p.CurrentSpeed
                }).ToList();

            UI.UpdateLeaderboard(sorted);
        }
    }
}