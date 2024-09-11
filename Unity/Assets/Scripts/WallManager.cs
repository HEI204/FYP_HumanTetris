using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Handle the wall movement and pose update in each round
public class WallManager : MonoBehaviour
{
    private List<GameObject> poseLists = new List<GameObject>();
    [SerializeField]
    private Image poseImage;
    [SerializeField]
    private TMP_Text poseAppearCountDown;
    [SerializeField]
    private TMP_Text poseCount;
    private float speed = 3;
    private float nextWallTime = 3;
    private float previousWallTime;
    private bool isNewRound;
    private bool isGameStart;
    private bool isFirstPose = true;
    private bool stopWall;
    private bool finishIntro;

    void Start(){   
        GetAllChild(transform, "Pose");
        previousWallTime = Time.time;
        GameManager.EndGame += HandleEndGame;
    }

    void Update() {
        if (!stopWall && finishIntro) {
            LoopWall();
            isGameStart = true;
        }
    }

    private void GetAllChild(Transform parent, string tag) {
        foreach (Transform child in parent) {
            if (child.gameObject.CompareTag("Pose"))
                poseLists.Add(child.gameObject);
            GetAllChild(child, tag);
        }
    }
    
    private void HandleEndGame() {
        isGameStart = false;
        stopWall = true;
    }

    // Iterate the pose in the pose lists (update pose each round)
    private void LoopWall() {
        if (poseLists.Count > 0) {
            float waitingTime = Time.time - previousWallTime;
            // Check the waiting time 
            if  ((isFirstPose && waitingTime > 5) || (!isFirstPose && waitingTime > nextWallTime)) {
                poseAppearCountDown.enabled = false;

                // Load the new pose and move it towards the player
                GameObject wall = poseLists.First();
                UpdatePosePanel(wall.name);
                wall.SetActive(true);
                isNewRound = false;
                wall.transform.Translate(Vector3.up * speed * Time.deltaTime);

                // Cleanup and update necessary game object after the wall pass through the player
                if (wall.transform.position.z >= 0) {
                    if (isFirstPose)
                        isFirstPose = false;
                    RemovePreviousWall(wall);
                    HidePoseImage();
                }  
            }

            else {
                // First pose count down for 5 second
                // Longer time for pose tracking prepartion
                poseAppearCountDown.enabled = true;
                if (isFirstPose)
                    poseAppearCountDown.SetText((5 - (int)waitingTime).ToString());
                else
                    poseAppearCountDown.SetText((nextWallTime - (int)waitingTime).ToString());
            }
        }
    }

    // Update the top center part of the Game UI (pose count + pose image)
    private void UpdatePosePanel(string imageName) {
        UpdatePoseCountText();
        UpdatePoseImage(imageName);
    }

    private void UpdatePoseCountText() {
        poseCount.SetText("Pose: " + (8 - poseLists.Count + 1) + "/8");
    }
    
    private void UpdatePoseImage(string imageName) {
        poseImage.enabled = true;
        poseImage.sprite = Resources.Load<Sprite>("Texture/"+ imageName);
    }

    private void HidePoseImage() {
        poseImage.enabled = false;
    }

    private void RemovePreviousWall(GameObject wall) {
        Destroy(wall);
        isNewRound = true;
        previousWallTime = Time.time;
        poseLists.RemoveAt(0);
    }

    public bool IsNewRound {
        get {return isNewRound;}
    }

    public bool IsGameStart {
        set {isGameStart = value;}
        get {return isGameStart;}
    }

    public int PoseCount {
        get {return poseLists.Count;}
    }

    public void StopWall() {
        stopWall = true;
    }

    public void FinishIntro() {
        finishIntro = true;
    }
}
