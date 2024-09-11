using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Disable the blocker object created by the dropdown menu in unity
public class BlockerDisabler : MonoBehaviour {
    void Update() {
        UnityEngine.UI.Dropdown.Destroy(GameObject.Find("Blocker"));
    }
}