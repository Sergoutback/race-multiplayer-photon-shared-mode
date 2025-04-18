using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(
                PlayerPrefab,
                new Vector3(0, 0.3f, 0),
                Quaternion.identity,
                player,
                (runner, obj) =>
                {
                    var playerMovement = obj.GetComponent<PlayerMovement>();
                    // Add uniqueness to the name
                    playerMovement.PlayerName = $"Player {player.PlayerId}_{Random.Range(1000, 9999)}";

                    var ui = FindObjectOfType<UILeaderboard>();
                    if (ui != null)
                        playerMovement.LeaderboardUI = ui;
                });
        }
    }
}