using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// Handle the game UI open / close
public class InGameUI : MonoBehaviour
{   
    [SerializeField]
    private GameObject gameOverPanel;
    [SerializeField]
    private GameObject settingPanel;
    [SerializeField]
    private GameObject gameEndPanel;
    [SerializeField]
    private GameObject questionPanel;
    [SerializeField]
    private GameObject introPanel;
    private bool isPause;

    void Start() {
        Time.timeScale = 0;
        
        introPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        settingPanel.SetActive(false);
        gameEndPanel.SetActive(false);

        questionPanel.transform.Find("Left Button").GetComponent<Button>().onClick.AddListener(StartQuestionAsking);
        questionPanel.transform.Find("Right Button").GetComponent<Button>().onClick.AddListener(CloseQuestionPanel);
        settingPanel.transform.Find("Exit Button").GetComponent<Button>().onClick.AddListener(CloseSettingPanel);
        introPanel.transform.Find("OK Button").GetComponent<Button>().onClick.AddListener(CloseIntroPanel);
    }

    void Update() {
        if (Input.GetKeyDown("escape") && IsAllPanelClose()) {
            if (!isPause) {
                isPause = true;
                settingPanel.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    private bool IsAllPanelClose() {
        return !questionPanel.activeSelf && !introPanel.activeSelf && !gameOverPanel.activeSelf && !gameEndPanel.activeSelf;
    }
    public void OpenGameOverPanel() {
        gameOverPanel.SetActive(true);
    }

    public void OpenWaitingPanel() {
        Time.timeScale = 0;
        gameEndPanel.SetActive(true);
    }

    public void OpenGameEndPanel() {
        double predictedScore = GameObject.Find("RegressionModel").GetComponent<RegressionModelHandler>().PredictedScore;

        gameEndPanel.transform.Find("Restart Button").GetComponent<Button>().gameObject.SetActive(true);
        gameEndPanel.transform.Find("Quit Button").GetComponent<Button>().gameObject.SetActive(true);
        gameEndPanel.transform.Find("Text (TMP)").GetComponent<RectTransform>().anchoredPosition.Set(0, 35);

        if (predictedScore == -2)
            gameEndPanel.transform.Find("Text (TMP)").GetComponent<TMP_Text>().SetText("Sorry for the inconvenience\nThere may be an error during predicting your depression score\nPlease try again if you want");
        else {
            string depressionSeverity = GameObject.Find("RegressionModel").GetComponent<RegressionModelHandler>().DepressionSeverity;
            gameEndPanel.transform.Find("Text (TMP)").GetComponent<TMP_Text>().SetText("Predicted derepssion score: " + predictedScore + "/63\n" + "Depression Severity: " + depressionSeverity);
        }
    }

    public void StartQuestionAsking() {
        int age;
        bool successfullyParsed  = int.TryParse(questionPanel.transform.Find("Age").GetComponent<TMP_InputField>().text, out age);
        if (successfullyParsed && age >= 8 && age <= 99) {
            // Hide deactivate unnesscary UI component and show Question UI
            questionPanel.transform.Find("Notice").GetComponent<TMP_Text>().gameObject.SetActive(false);
            questionPanel.transform.Find("Reason").GetComponent<TMP_Text>().gameObject.SetActive(false);
            questionPanel.transform.Find("Age").GetComponent<TMP_InputField>().gameObject.SetActive(false);
            questionPanel.transform.Find("AgeWarning").GetComponent<TMP_Text>().gameObject.SetActive(false);
            questionPanel.transform.Find("Question").GetComponent<TMP_Text>().gameObject.SetActive(true);

            // Get selected language question and display
            TMP_Dropdown questionLanguage = questionPanel.transform.Find("Language").transform.Find("language").GetComponent<TMP_Dropdown>();
            string selectedLanguage = questionLanguage.options[questionLanguage.value].text == "English"? "en" : "ch";
            questionLanguage.transform.parent.gameObject.SetActive(false);
            questionPanel.GetComponent<QuestionnaireManager>().SelectLanguage(selectedLanguage, age);
        }
        // Age input restriction reminder
        else 
            questionPanel.transform.Find("AgeWarning").GetComponent<TMP_Text>().gameObject.SetActive(true);
    }

    public void CloseQuestionPanel() {
        questionPanel.SetActive(false);
        OpenIntroPanel();
    }

    public void OpenIntroPanel() {
        introPanel.SetActive(true);
        string originalIntroText = introPanel.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text;

        if (questionPanel.GetComponent<QuestionnaireManager>().Score != -1)
            introPanel.transform.Find("Text (TMP)").GetComponent<TMP_Text>().SetText("Thank you for taking the time to complete the survey!\n\n"+originalIntroText);
    }

    public void CloseSettingPanel() {
        isPause = false;
        settingPanel.SetActive(false);
        Time.timeScale = 1;
    }

    public void CloseIntroPanel() {
        introPanel.SetActive(false);
        GameObject.Find("PoseLists").GetComponent<WallManager>().FinishIntro();
        Time.timeScale = 1;
    }
}
