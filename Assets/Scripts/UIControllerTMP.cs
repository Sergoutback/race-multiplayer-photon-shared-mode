using Fusion;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;

public class UIControllerTMP : MonoBehaviour
{
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI PositionText;

    private float raceTime = 0f;
    private PlayerMovement player;

    void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    IEnumerator WaitForLocalPlayer()
    {
        while (player == null)
        {
            player = FindObjectsOfType<PlayerMovement>()
                .FirstOrDefault(p => p.HasStateAuthority);

            if (player == null)
                yield return null; // wait one frame
        }
    }

    void Update()
    {
        if (player != null)
        {
            float speedKmh = player.CurrentSpeed * 3.6f;
            SpeedText.text = $"Speed: {speedKmh:F0} km/h";
            
            if (RaceManager.Instance != null)
            {
                int position = RaceManager.Instance.GetPlayerPosition(player);
                int total = RaceManager.Instance.GetTotalPlayers();
                PositionText.text = $"Position: {position} / {total}";
            }
        }

        raceTime += Time.deltaTime;
        TimerText.text = $"Time: {FormatTime(raceTime)}";
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int millis = Mathf.FloorToInt((time * 10f) % 10f);
        return $"{minutes:00}:{seconds:00}.{millis}";
    }
}