using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NaughtyAttributes;

public enum PledgeSeverity { Easy, Medium, Hard }

[Serializable]
public struct Pledge
{
    public string text;
    public PledgeSeverity severity;
}

public class GoogleDocParser : MonoBehaviour
{
    public static GoogleDocParser Instance;
    
    const string GamesDocUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTUEueONmnscDV3UjBhqzwlDOeUr-P_G5bTQhAjyeFSUyCuuhcmD_rKD7R3NFaRyUPsfIIr-psBqrPg/pub?gid=0&single=true&output=csv";
    const string PledgesDocUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTUEueONmnscDV3UjBhqzwlDOeUr-P_G5bTQhAjyeFSUyCuuhcmD_rKD7R3NFaRyUPsfIIr-psBqrPg/pub?gid=1330974750&single=true&output=csv";

    [Header("Parsed Data")] 
    public List<string> gamesList = new List<string>();
    public List<Pledge> pledgesList = new List<Pledge>();

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

    [Button("Refresh Data")]
    public void RefreshAll()
    {
        StartCoroutine(DownloadGamesStrings());
        StartCoroutine(DownloadPledges());
    }

    IEnumerator DownloadGamesStrings()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(GamesDocUrl))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch sentences doc: " + www.error);
                yield break;
            }
            gamesList.Clear();
            string[] lines = www.downloadHandler.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                gamesList.Add(line.Trim());
            }
        }
    }

    IEnumerator DownloadPledges()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(PledgesDocUrl))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch severity doc: " + www.error);
                yield break;
            }
            pledgesList.Clear();
            string[] lines = www.downloadHandler.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 2) continue;
                var sentence = parts[0].Trim();
                var severityStr = parts[1].Trim().ToLower();
                PledgeSeverity pledgeSeverity = PledgeSeverity.Easy;
                if (severityStr == "medium") pledgeSeverity = PledgeSeverity.Medium;
                else if (severityStr == "hard") pledgeSeverity = PledgeSeverity.Hard;
                pledgesList.Add(new Pledge { text = sentence, severity = pledgeSeverity });
            }
        }
    }
    
    public string GetRandomGame()
    {
        if (gamesList.Count == 0) return "No games available";
        int randomIndex = UnityEngine.Random.Range(0, gamesList.Count);
        return gamesList[randomIndex];
    }
    
    public Pledge GetRandomPledge()
    {
        if (pledgesList.Count == 0) return new Pledge { text = "No pledges available", severity = PledgeSeverity.Easy };
        int randomIndex = UnityEngine.Random.Range(0, pledgesList.Count);
        return pledgesList[randomIndex];
    }
}
