using UnityEngine;

public class PanelToggle : MonoBehaviour
{
    // the panel to toggle
    [SerializeField] GameObject panel;

    // function that toggles the control panel
    public void TogglePanel()
    {
        // set the panel active state to the opposite of its current state
        panel.SetActive(!panel.activeSelf);
    }
}