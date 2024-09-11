using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Radio button used in BDI-II questionnaire
public class RadioButtonSystem : MonoBehaviour
{
    private ToggleGroup toggleGroup;
    // Start is called before the first frame update
    void Start() {
        toggleGroup = GetComponent<ToggleGroup>();
    }

    public void Submit() {
        Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
        Debug.Log(toggle.name + " " + toggle.GetComponentInChildren<Text>().text);
    }
}
