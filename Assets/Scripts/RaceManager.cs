using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    private List<PlayerMovement> players = new List<PlayerMovement>();
    
    public Transform[] Checkpoints;

    public Transform FinishLine;

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
        var sorted = players
            .OrderByDescending(p => p.CurrentCheckpointIndex)
            .ThenBy(p => Vector3.Distance(p.transform.position, Checkpoints[Mathf.Clamp(p.CurrentCheckpointIndex + 1, 0, Checkpoints.Length - 1)].position))
            .ToList();

        return sorted.IndexOf(localPlayer) + 1;
    }

    public int GetTotalPlayers() => players.Count;

    public List<PlayerMovement> GetPlayers() => players;
}