using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NaughtyAttributes;

public class GoogleDocParser : MonoBehaviour
{
    [Header("Google Doc CSV URLs (Published)")]
    [Tooltip("First doc: simple sentences, one per line/cell")]
    public string gamesDocUrl = "https://docs.google.com/spreadsheets/d/PLACEHOLDER/export?format=csv";
    [Tooltip("Second doc: sentence, severity (easy, medium, hard)")]
    public string pledgesDocUrl = "https://docs.google.com/spreadsheets/d/PLACEHOLDER/export?format=csv";

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

    // Call this to refresh both docs (e.g. from a button)
    [Button("Refresh Data")]
    public void RefreshAll()
    {
        StartCoroutine(DownloadGamesStrings());
        StartCoroutine(DownloadPledges());
    }

    IEnumerator DownloadGamesStrings()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(gamesDocUrl))
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
            Debug.Log($"Loaded {sentences.Count} sentences.");
        }
    }

    IEnumerator DownloadPledges()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(pledgesDocUrl))
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
            Debug.Log($"Loaded {sentencesWithSeverity.Count} sentences with severity.");
        }
    }
}

