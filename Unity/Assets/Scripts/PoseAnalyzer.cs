using System;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.PoseTracking;

public class PoseAnalyzer : MonoBehaviour
{
    [SerializeField]
    private PoseTrackingSolution poseTrackingSolution;
    [SerializeField]
    private WallManager wM;
    private const int LANDMARK_COUNT = 33;
    private List<Vector2> previousLandmarks = new List<Vector2>();
    [SerializeField]
    private List<double>[] landmarkDistances = new List<double>[LANDMARK_COUNT];

    private void Start()
    {
        // Find the PoseTrackingSolution instance in the scene
        GameObject poseTrackingObject = GameObject.Find("Solution");
        if (poseTrackingObject != null)
        {
            // Handle action event fired by PoseTrackingSolution to get the body point
            poseTrackingSolution = poseTrackingObject.GetComponent<PoseTrackingSolution>();
            poseTrackingSolution.MediapipePredictionsReceived += HandleReceivedPoseLandMarks;
        }
        else
            Debug.LogError("PoseTrackingSolution GameObject not found in the scene.");

        wM = GameObject.Find("PoseLists").GetComponent<WallManager>();
        for (int i = 0; i < LANDMARK_COUNT; i++)
        {
            landmarkDistances[i] = new List<double>();
        }
    }

    public List<double>[] LandMarkDistances { get { return landmarkDistances; } }

    private void HandleReceivedPoseLandMarks(Mediapipe.LandmarkList landmarks)
    {
        if (wM.IsGameStart)
        {
            if (previousLandmarks.Count > 0)
            {
                CalculateLandmarksDistance(landmarks);
            }

            // Update previous landmarks
            previousLandmarks.Clear();
            for (int i = 0; i < LANDMARK_COUNT; i++)
            {
                previousLandmarks.Add(new Vector2(landmarks.Landmark[i].X, landmarks.Landmark[i].Y));
            }
        }
    }

    private void CalculateLandmarksDistance(Mediapipe.LandmarkList newLandmarks)
    {
        for (int i = 0; i < LANDMARK_COUNT; i++)
        {
            double dx = newLandmarks.Landmark[i].X - previousLandmarks[i].x;
            double dy = newLandmarks.Landmark[i].Y - previousLandmarks[i].y;
            landmarkDistances[i].Add(Math.Round(Math.Sqrt(dx * dx + dy * dy), 5));
        }
    }
}