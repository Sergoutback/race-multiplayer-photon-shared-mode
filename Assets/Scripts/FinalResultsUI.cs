using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalResultsUI : MonoBehaviour
{
    public TextMeshProUGUI resultsText;
    public Button backToLobbyButton;

    private void Start()
    {
        ShowResults();

        backToLobbyButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("RaceScene");
        });
    }

    private void ShowResults()
    {
        resultsText.text = "";

        foreach (var entry in RaceResults.Players)
        {
            resultsText.text += $"{entry.Position}. {entry.Name,-10} — {FormatTime(entry.Time)}\n";
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