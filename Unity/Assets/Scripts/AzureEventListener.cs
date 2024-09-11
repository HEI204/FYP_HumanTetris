using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Check whether the event for upload data to Azure Table and 
// get predicted score from Azure Web Service is finish
public class AzureEventListener : MonoBehaviour
{
    [SerializeField]
    private bool dataUploaded;
    [SerializeField]
    private bool predictedResultObtained;
    void Start()
    {
        AzureUploader.DataUploaded += HandleDataUploaded;
        RegressionModelHandler.PredictedResultObtained +=HandlePredictedResultObtained;
    }

    void Update()
    {
        checkAllEventFinish();
    }

    private void HandleDataUploaded() {
        dataUploaded = true;
    }

    private void HandlePredictedResultObtained() {
        predictedResultObtained = true;
    }

    private void checkAllEventFinish() {
        if (dataUploaded && predictedResultObtained)
            GameObject.Find("Canvas").GetComponent<InGameUI>().OpenGameEndPanel();
    }
}
