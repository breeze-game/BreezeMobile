using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressRingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    // variables for long press functionality
    [SerializeField] float holdSeconds = 0.75f; // how long to hold in seconds
    [SerializeField] Image ringFill; // the radial ring fill image
    [SerializeField] UnityEvent onHoldComplete; // event for when the hold is completed

    void Awake()
    {
        EnsureHoldEvent();
    }

    void EnsureHoldEvent()
    {
        if (onHoldComplete == null)
            onHoldComplete = new UnityEvent();
    }

    // wire ring from code (BreedingUIBuilder)
    public void SetRingImage(Image img)
    {
        ringFill = img;
    }

    // add listener from code
    public void AddCompleteListener(UnityAction action)
    {
        EnsureHoldEvent();
        if (action != null)
            onHoldComplete.AddListener(action);
    }

    // is finger down
    bool holding;
    // time held this press
    float holdT;
    // already invoked callback for this press
    bool fired;

    // start tracking hold
    public void OnPointerDown(PointerEventData eventData)
    {
        holding = true;
        holdT = 0f;
        fired = false;
        if (ringFill != null)
            ringFill.fillAmount = 0f;
    }

    // cancel hold if finger lifted
    public void OnPointerUp(PointerEventData eventData)
    {
        ResetPress();
    }

    // cancel if finger slides off button
    public void OnPointerExit(PointerEventData eventData)
    {
        ResetPress();
    }

    void Update()
    {
        // nothing to do if not holding or already fired
        if (!holding || fired) return;
        // accumulate hold time (unscaled so pause does not break)
        holdT += Time.unscaledDeltaTime;
        // drive ring visual
        if (ringFill != null)
            ringFill.fillAmount = Mathf.Clamp01(holdT / holdSeconds);
        // threshold reached
        if (holdT >= holdSeconds)
        {
            fired = true;
            EnsureHoldEvent();
            onHoldComplete.Invoke();
            ResetPress();
        }
    }

    // clear hold state and ring
    void ResetPress()
    {
        holding = false;
        holdT = 0f;
        if (ringFill != null)
            ringFill.fillAmount = 0f;
    }
}
