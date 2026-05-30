using UnityEngine;
using System;
using System.IO;

public class SoccerSendDataAPI : MonoBehaviour
{
    public string gameThemeId = "soccer";

    public void SetupAPIDataToSend(BallShooter ballShooter)
    {
        string json = "{\n";
        json += "\"gameThemeId\":\"" + gameThemeId + "\",\n";
        json += "\"sentAtUnixTime\":" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ",\n";
        json += "\"trials\":[";

        for (int i = 0; i < ballShooter.results.Count; i++)
        {
            var trial = ballShooter.results[i];
            json += "{";
            json += "\"gameThemeId\":\"" + gameThemeId + "\",";
            json += "\"trialNumber\":" + trial.trialNumber + ",";
            json += "\"ballAppearedAt\":" + trial.ballAppearedAt + ",";
            json += "\"playerTappedAt\":" + trial.playerTappedAt + ",";
            json += "\"reactionTimeMs\":" + trial.reactionTimeMs + ",";
            json += "\"scored\":" + trial.scored.ToString().ToLower();
            json += "}";

            if (i < ballShooter.results.Count - 1)
                json += ",";
        }

        json += "]\n}";

        string fileName = "soccer_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ".json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(filePath, json);
        Debug.Log("JSON saved to: " + filePath);
        Debug.Log("=== SOCCER DATA ===");
        Debug.Log(json);
    }
}