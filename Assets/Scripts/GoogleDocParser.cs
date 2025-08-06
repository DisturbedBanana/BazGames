using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NaughtyAttributes;

public class GoogleDocParser : MonoBehaviour
{
    private const string GamesDocUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTUEueONmnscDV3UjBhqzwlDOeUr-P_G5bTQhAjyeFSUyCuuhcmD_rKD7R3NFaRyUPsfIIr-psBqrPg/pub?gid=0&single=true&output=csv";
    private const string PledgesDocUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTUEueONmnscDV3UjBhqzwlDOeUr-P_G5bTQhAjyeFSUyCuuhcmD_rKD7R3NFaRyUPsfIIr-psBqrPg/pub?gid=1330974750&single=true&output=csv";

    [Header("Parsed Data")] 
    public List<string> sentences = new List<string>();
    public List<Pledge> sentencesWithSeverity = new List<Pledge>();

    public enum Severity { Easy, Medium, Hard }

    [Serializable]
    public struct Pledge
    {
        public string sentence;
        public Severity severity;
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
            sentences.Clear();
            string[] lines = www.downloadHandler.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                sentences.Add(line.Trim());
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
            sentencesWithSeverity.Clear();
            string[] lines = www.downloadHandler.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length < 2) continue;
                var sentence = parts[0].Trim();
                var severityStr = parts[1].Trim().ToLower();
                Severity severity = Severity.Easy;
                if (severityStr == "medium") severity = Severity.Medium;
                else if (severityStr == "hard") severity = Severity.Hard;
                sentencesWithSeverity.Add(new Pledge { sentence = sentence, severity = severity });
            }
        }
    }
}

