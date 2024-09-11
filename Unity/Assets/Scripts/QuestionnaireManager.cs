using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

// Handle the score counting, question asking process of the BDI-II questionnaire for player
public class QuestionnaireManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private ToggleGroup toggleGroup;
    private Button leftButton;
    private Button rightButton;
    private Dictionary<string, Dictionary<string, Dictionary<string, string>>> questionnaire;
    private string language;
    private int?[] optionRecord = new int?[21];
    private int score = -1;
    private int age;
    private bool startQuestion;
    private List<string> questionKeys;
    [SerializeField]
    private int currentQuestionIndex;


    void Start() {
        // Obtain the questionnaire data from JSON file
        TextAsset jsonFile = Resources.Load<TextAsset>("Questionnaires/BDI-II");
        questionnaire = DeserializeJsonString(jsonFile.text);
    }

    void Update() {
        if (startQuestion && !toggleGroup.AnyTogglesOn()) 
            transform.Find("Right Button").GetComponent<Button>().interactable = false;
        else
            transform.Find("Right Button").GetComponent<Button>().interactable = true;
    }

    public int Score {
        get {return score;}
    }

    private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> DeserializeJsonString(string jsonString) {
        return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(jsonString);
    }

    public void SelectLanguage(string language, int age) {
        startQuestion = true;
        this.age = age;
        this.language = language;
        if (questionnaire.ContainsKey(language)) {
            // Get all the question
            questionKeys = new List<string>(questionnaire[language].Keys);
            currentQuestionIndex = 0;

            // Setup the button
            transform.Find("Left Button").GetComponentInChildren<TMP_Text>().SetText("Back");
            transform.Find("Right Button").GetComponentInChildren<TMP_Text>().SetText("Next");

            leftButton = transform.Find("Left Button").GetComponent<Button>();
            rightButton = transform.Find("Right Button").GetComponent<Button>();

            leftButton.onClick.RemoveAllListeners();
            rightButton.onClick.RemoveAllListeners();

            leftButton.onClick.AddListener(PreviousQuestion);
            rightButton.onClick.AddListener(NextQuestion);

            leftButton.interactable = false;

            ShowQuestion();
        }
        else
            Debug.LogError("Selected language not found in questionnaire.");
    }

    private void SaveCurrentSelectedOption() {
        optionRecord[currentQuestionIndex] = int.Parse(toggleGroup.GetFirstActiveToggle().name);
    }

    private void UsePreviousSavedSelectedOption() {
        toggleGroup.GetComponentsInChildren<Toggle>()[(int)optionRecord[currentQuestionIndex]].isOn = true;
    }

    private void NextQuestion() {
        if (toggleGroup.AnyTogglesOn()) {
            SaveCurrentSelectedOption();
            // Go to next question and check whether it is last question or not
            currentQuestionIndex++;
            // If click "Next" in last question
            if (currentQuestionIndex >= questionKeys.Count) {
                FinishAllQuestion();
                GameObject.Find("Canvas").GetComponent<InGameUI>().CloseQuestionPanel();
            }
            else {
                // Last qusetion
                ShowQuestion();

                if (currentQuestionIndex == questionKeys.Count - 1)
                    transform.Find("Right Button").GetComponentInChildren<TMP_Text>().SetText("Finish");

                if (optionRecord[currentQuestionIndex].HasValue) 
                    UsePreviousSavedSelectedOption();

                transform.Find("Left Button").GetComponent<Button>().interactable = true;
            }
        }
    }

    private void PreviousQuestion() {
        if (currentQuestionIndex > 0) {
            if (toggleGroup.AnyTogglesOn())
                SaveCurrentSelectedOption();

            currentQuestionIndex--;
            if (currentQuestionIndex == 0)
                leftButton.interactable = false;

            if (transform.Find("Right Button").GetComponentInChildren<TMP_Text>().text == "Finish")
                transform.Find("Right Button").GetComponentInChildren<TMP_Text>().SetText("Next");

            ShowQuestion();
            UsePreviousSavedSelectedOption();
        }
    }

    public static event Action<int, int> FinishQuestionnaire;

    private void FinishAllQuestion() {
        score = 0;
        foreach (int s in optionRecord) 
            score += s;
            
        if (score != -1)
            FinishQuestionnaire?.Invoke(score, age);

        
    }

    private void ShowQuestion() {
        if (currentQuestionIndex < questionKeys.Count) {
            string questionKey = questionKeys[currentQuestionIndex];
            questionText.SetText(questionKey + " (Total Question: 21)");

            Dictionary<string, string> options = questionnaire[language][questionKey];
            int i = 0;
            foreach (var option in options) {
                toggleGroup.GetComponentsInChildren<Toggle>()[i].GetComponentInChildren<Text>().text = option.Value;
                i++;
            }
            toggleGroup.SetAllTogglesOff();
        }
    }
}
