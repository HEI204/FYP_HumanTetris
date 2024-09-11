using UnityEngine;
using System;
using TMPro;


// Handle in-game score & heart management 
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text scoreText;
    private GameObject player;
    private GameObject poseLists;
    private int score;
    private bool getScore;
    private int heartCounts = 4;
    private GameObject[] hearts = new GameObject[4];
    private bool deductHeart;
    private bool isGameEnd;
    private bool isGameOver;
    private Player playerScript;
    private WallManager wallScript;
    [SerializeField]
    private AudioSource successSoundEffect;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0];
        poseLists = GameObject.Find("PoseLists");

        playerScript = player.GetComponent<Player>();
        wallScript = poseLists.GetComponent<WallManager>();

        for (int i = 0; i < heartCounts; i++)
        {
            string objectName = "Heart" + (i + 1);
            hearts[i] = GameObject.Find(objectName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Continue the game
        if (!isGameOver && !isGameEnd)
        {
            // Handle score
            scoreText.SetText("Score: " + score);
            if (!getScore && playerScript.MatchAllCollider() && !deductHeart)
            {
                getScore = true;

            }

            if (getScore && !playerScript.MatchAllCollider())
            {
                score++;
                successSoundEffect.Play();
                getScore = false;
            }

            // Notify new round start
            if (wallScript.IsNewRound)
                playerScript.NewRound();

            // Handle heart deduction
            if (!deductHeart && playerScript.IsHurt && !getScore)
                deductHeart = true;

            if (deductHeart && !playerScript.IsHurt && !getScore)
            {
                DeductHeart();
                deductHeart = false;
            }
            
            // End game when all pose already show 
            if (wallScript.PoseCount == 0) {
                isGameEnd = true;
                GameObject.Find("Canvas").GetComponent<InGameUI>().OpenWaitingPanel();
                EndGame.Invoke();
            }
        }
    }

    public static event Action EndGame;

    public void DeductHeart()
    {
        hearts[heartCounts - 1].SetActive(false);
        heartCounts -= 1;

        if (heartCounts == 0)
        {
            wallScript.StopWall();
            isGameOver = true;
            GameObject.Find("Canvas").GetComponent<InGameUI>().OpenGameOverPanel();
        }
    }
}
