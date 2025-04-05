using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    [Networked] public NetworkBool RaceFinished { get; set; }

    private List<PlayerMovement> players = new List<PlayerMovement>();

    public Transform[] Checkpoints;

    public override void Spawned()
    {
        if (Instance == null) Instance = this;
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

    public void OnPlayerFinish(PlayerMovement player)
    {
        if (RaceFinished) return;

        RaceFinished = true;

        RaceResults.Players = GetPlayers()
            .OrderByDescending(p => p.CurrentCheckpointIndex)
            .ThenBy(p => Vector3.Distance(p.transform.position, Checkpoints.Last().position))
            .Select((p, index) => new RaceResults.Entry
            {
                Name = p.PlayerName,
                Position = index + 1,
                Time = p.ElapsedTime
            }).ToList();

        if (Object.HasStateAuthority)
        {
            Runner.LoadScene("FinalResultsScene", LoadSceneMode.Single);
        }
    }
}