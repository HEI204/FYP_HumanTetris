using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;

public class RegressionModelHandler : MonoBehaviour
{
    private string apiURI = "https://fypmodelwebapp.azurewebsites.net/depression_prediction";
    private bool canUpload;
    [SerializeField]
    private List<double>[] landMarkDistances;
    private double predictedScore = -1;
    private string depressionSeverity = "";

    // Start is called before the first frame update
    void Start()
    {
        GameManager.EndGame += HandleEndGame;
        StartCoroutine(CallRegressionModel());
    }

    public double PredictedScore { get { return predictedScore; }}
    public string DepressionSeverity { get { return depressionSeverity; }}

    private void HandleEndGame() {
        landMarkDistances = GameObject.Find("PoseAnalyzer").GetComponent<PoseAnalyzer>().LandMarkDistances;
        canUpload = true;
    }

    private Dictionary<string, double> CalculateOverallEntropyFeatures()
    {
        List<int> landmarkList = Enumerable.Range(12, 18).Where(i => !new[] { 17, 18, 19, 20, 21, 22 }.Contains(i)).ToList();
        Dictionary<string, double> entropyFeatures = new Dictionary<string, double>();

        List<double> entropies = new List<double>();
        foreach (var landmarkIndex in landmarkList)
        {
            double entropy = CalculateMotionEntropy(landMarkDistances[landmarkIndex]);
            if (!double.IsNaN(entropy))
            {
                entropies.Add(entropy);
                entropyFeatures[$"entropy_Landmark{landmarkIndex}"] = entropy;
            }
        }

        if (entropies.Count > 0)
        {
            double meanEntropy = entropies.Average();
            entropyFeatures["mean_entropy"] = meanEntropy;

            List<double> aggregatedMotionAmplitudes = landmarkList.SelectMany(index => landMarkDistances[index]).ToList();
            double aggregatedEntropy = CalculateMotionEntropy(aggregatedMotionAmplitudes);
            entropyFeatures["aggregated_entropy"] = aggregatedEntropy;
        }

        return entropyFeatures;
    }

    private double CalculateMotionEntropy(List<double> motionAmplitudes)
    {
        double Dz = motionAmplitudes.Sum();
        if (Dz == 0) return double.NaN;

        double entropy = -motionAmplitudes.Where(D => D > 0).Sum(D => (D / Dz) * Math.Log(D / Dz, 2));
        return entropy;
    }

    public static event Action PredictedResultObtained;

    IEnumerator CallRegressionModel() {
        while(!canUpload) 
            yield return null;
            
        Dictionary<string, double> data = CalculateOverallEntropyFeatures();
        // Dictionary<string, double> data = new Dictionary<string, double> {
        //     { "mean_entropy", 9.433458 },
        //     { "entropy_Landmark12", 9.556545 },
        //     { "entropy_Landmark13", 9.592022 },
        //     { "entropy_Landmark14", 9.374832 },
        //     { "entropy_Landmark15", 9.491546 },
        //     { "entropy_Landmark16", 9.354431 },
        //     { "entropy_Landmark23", 9.368112 },
        //     { "entropy_Landmark24", 9.337321 },
        //     { "entropy_Landmark25", 9.335870 },
        //     { "entropy_Landmark26", 9.209862 },
        //     { "entropy_Landmark27", 9.257638 },
        //     { "entropy_Landmark28", 9.667530 },
        //     { "entropy_Landmark29", 9.655788 },
        //     { "aggregated_entropy", 9.694899 }
        // };

        string jsonData = JsonConvert.SerializeObject(data);

        // Create a UnityWebRequest
        using (UnityWebRequest webRequest = new UnityWebRequest(apiURI, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                // Get the response text
                string jsonResponse = webRequest.downloadHandler.text;
            
                try {
                    Dictionary<string, List<double>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<double>>>(jsonResponse);
                    
                    if (Math.Round(dict["result"][0]) > 13)
                        predictedScore = 63;
                    else if (Math.Round(dict["result"][0]) < -1)
                        predictedScore = 0;
                    else
                        predictedScore = Math.Round(dict["result"][0]);

                    if (predictedScore <= 13)
                        depressionSeverity = "Normal (正常)";
                    else if (predictedScore <= 19)
                        depressionSeverity = "Mild (輕度抑鬱)";
                    else if (predictedScore <= 28)
                        depressionSeverity = "Moderate (中度抑鬱)";
                    else
                        depressionSeverity = "Severe (重度抑鬱)";

                    PredictedResultObtained.Invoke();
                }
                catch (JsonSerializationException ex) {
                    Debug.LogError("Error deserializing response: " + ex.Message);
                    predictedScore = -2;
                }
            }
        }
    }
}