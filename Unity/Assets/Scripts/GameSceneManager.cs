using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Handle changing scene during game play
public class GameSceneManager : MonoBehaviour {
    public void ToStart() {
        SceneManager.LoadScene("Start");
    }
    
    public void ToGameScene() {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    public void ToSettingScene() {
        SceneManager.LoadScene("Setting");
    }

    public void QuitGame() {
        Application.Quit();
    }
}