using UnityEngine;
using UnityEngine.EventSystems;

// single tap on this object closes the panel (for controls overlay)
public class TapToClosePanel : MonoBehaviour, IPointerClickHandler
{
    // panel root to hide (often this same gameobject)
    [SerializeField] GameObject panel;

    // unity calls this when user taps and event system hits this graphic
    public void OnPointerClick(PointerEventData eventData)
    {
        // only if assigned
        if (panel != null)
            panel.SetActive(false);
    }
}
