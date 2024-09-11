using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Handle the basic game setting like screen resolution & background music
public class SettingManager : MonoBehaviour {
    private bool enableBackgroundMusic;
    private string resolution;

    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullScreenToggle;

    public string Resolution {
        get { return resolution; }
    }

    void Start() {
        fullScreenToggle.onValueChanged.AddListener((newValue) => {
            Screen.fullScreen = fullScreenToggle.isOn;
        });
        
        resolutionDropdown.onValueChanged.AddListener((selectedIdx) => { 
            resolution = resolutionDropdown.options[selectedIdx].text;
            Screen.SetResolution(
                int.Parse(resolution.Split("x")[0]), 
                int.Parse(resolution.Split("x")[1]), 
                true
            );
        });

        // Ensure the system setting and the toggle value is match initially
        if (fullScreenToggle.isOn != Screen.fullScreen) {
            if (Screen.fullScreen) {
                Screen.SetResolution(1920, 1080, true);
                fullScreenToggle.isOn = true;
            }
            else {
                Screen.SetResolution(1600, 900, true);
                fullScreenToggle.isOn = false;
            }
        }
    }

    void Update() {
        // Ensure the system setting and the toggle value is match
        if (fullScreenToggle.isOn != Screen.fullScreen) {
            Screen.fullScreen = fullScreenToggle.isOn;
        }
    }
}