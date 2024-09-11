using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// Upload data to Azure Table Storage via Azure Function

public class AzureUploader : MonoBehaviour
{
    private bool canUpload;
    [SerializeField]
    private int questionnaireScore;
    [SerializeField]
    private int playerAge;
    private List<double>[] landMarkDistances;

    void Start() {
        // Handle action event fired by QuestionnaireManager when player finish the questionnaire
        QuestionnaireManager.FinishQuestionnaire += HandleQuestionnaireFinish;
        GameManager.EndGame += HandleEndGame;
        StartCoroutine(Upload());
    }
    
    private void HandleQuestionnaireFinish(int score, int age) {
        questionnaireScore = score;
        playerAge = age;
    }

    private void HandleEndGame() {
        if (playerAge != 0) {
            canUpload = true;
            landMarkDistances = GameObject.Find("PoseAnalyzer").GetComponent<PoseAnalyzer>().LandMarkDistances;
        }
        else 
            DataUploaded.Invoke();
    }

    private string CreatePlayerDataJson() { 
        PlayerData data = new PlayerData() {
            user = Guid.NewGuid().ToString(),
            score = questionnaireScore,
            age = playerAge,
            Landmark1 = landMarkDistances[0],
            Landmark2 = landMarkDistances[1],
            Landmark3 = landMarkDistances[2],
            Landmark4 = landMarkDistances[3],
            Landmark5 = landMarkDistances[4],
            Landmark6 = landMarkDistances[5],
            Landmark7 = landMarkDistances[6],
            Landmark8 = landMarkDistances[7],
            Landmark9 = landMarkDistances[8],
            Landmark10 = landMarkDistances[9],
            Landmark11 = landMarkDistances[10],
            Landmark12 = landMarkDistances[11],
            Landmark13 = landMarkDistances[12],
            Landmark14 = landMarkDistances[13],
            Landmark15 = landMarkDistances[14],
            Landmark16 = landMarkDistances[15],
            Landmark17 = landMarkDistances[16],
            Landmark18 = landMarkDistances[17],
            Landmark19 = landMarkDistances[18],
            Landmark20 = landMarkDistances[19],
            Landmark21 = landMarkDistances[20],
            Landmark22 = landMarkDistances[21],
            Landmark23 = landMarkDistances[22],
            Landmark24 = landMarkDistances[23],
            Landmark25 = landMarkDistances[24],
            Landmark26 = landMarkDistances[25],
            Landmark27 = landMarkDistances[26],
            Landmark28 = landMarkDistances[27],
            Landmark29 = landMarkDistances[28],
            Landmark30 = landMarkDistances[29],
            Landmark31 = landMarkDistances[30],
            Landmark32 = landMarkDistances[31],
            Landmark33 = landMarkDistances[32],
        };
        
        return JsonConvert.SerializeObject(data);
    }

    public static event Action DataUploaded;

    IEnumerator Upload() {
        while(!canUpload) 
            yield return null;

        string playerDataJson = CreatePlayerDataJson();
        using (
            UnityWebRequest webRequest = UnityWebRequest.Post(
                "https://fyp-function-app.azurewebsites.net/api/UploadPlayerData?code=PiMaI_5rPH8WBjWJe_Fd1_BPjhsAPZ_HsbYQVy5YMIhkAzFu7_DbVQ%3D%3D", 
                playerDataJson,
                "application/json"
            )
        ){
            yield return webRequest.SendWebRequest();

            if (webRequest.responseCode != 201)
                Debug.LogError(webRequest.error);
            else {
                Debug.Log("Form upload complete!");
                DataUploaded.Invoke();
            }
        }
    }
}

public class PlayerData {
    public string user { get; set; }
    public int score { get; set; }
    public int age { get; set; }
    public List<double> Landmark1 { get; set; }
    public List<double> Landmark2 { get; set; }
    public List<double> Landmark3 { get; set; }
    public List<double> Landmark4 { get; set; }
    public List<double> Landmark5 { get; set; }
    public List<double> Landmark6 { get; set; }
    public List<double> Landmark7 { get; set; }
    public List<double> Landmark8 { get; set; }
    public List<double> Landmark9 { get; set; }
    public List<double> Landmark10 { get; set; }
    public List<double> Landmark11 { get; set; }
    public List<double> Landmark12 { get; set; }
    public List<double> Landmark13 { get; set; }
    public List<double> Landmark14 { get; set; }
    public List<double> Landmark15 { get; set; }
    public List<double> Landmark16 { get; set; }
    public List<double> Landmark17 { get; set; }
    public List<double> Landmark18 { get; set; }
    public List<double> Landmark19 { get; set; }
    public List<double> Landmark20 { get; set; }
    public List<double> Landmark21 { get; set; }
    public List<double> Landmark22 { get; set; }
    public List<double> Landmark23 { get; set; }
    public List<double> Landmark24 { get; set; }
    public List<double> Landmark25 { get; set; }
    public List<double> Landmark26 { get; set; }
    public List<double> Landmark27 { get; set; }
    public List<double> Landmark28 { get; set; }
    public List<double> Landmark29 { get; set; }
    public List<double> Landmark30 { get; set; }
    public List<double> Landmark31 { get; set; }
    public List<double> Landmark32 { get; set; }
    public List<double> Landmark33 { get; set; }
}