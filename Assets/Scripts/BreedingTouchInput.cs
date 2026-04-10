using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class BreedingTouchInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // variable for the breeding session
    [SerializeField] BreedingSession session;

    // settings for the tap timing
    [Header("Tap Input Timing")]
    [SerializeField] float multiTapWindow = 0.4f; // time window for multiple taps
    [SerializeField] float tapMoveTolerance = 30f; // tolerance for tap movement

    // optional: assign menu from scene; if empty, we find it at runtime (double-tap opens it)
    [Header("Help menu (double-tap background)")]
    [SerializeField] HelpMenu helpMenu;

    // event for double tap
    [Header("Events")]
    public UnityEvent onDoubleTap; // create a new event for double tap
    Vector2 downPos; // variable for the down position where the touch started)
    float lastTapTime = -999f; // variable for the last tap time (currently "never")
    int tapCount = 0; // variable for the tap count (initalized at reset/0)

    // need to handle the taps better so that triple tap is not a double tap
    Coroutine resolveTapRoutine;

    // function that is called when the pointer is down (meaning finger is touching the screen)
    public void OnPointerDown(PointerEventData eventData)
    {
        // set the down position to the event data position
        downPos = eventData.position;
    }

    // function that is called when the pointer is up (meaning finger is lifted from the screen)
    public void OnPointerUp(PointerEventData eventData)
    {
        // detects when finger no longer touching the screen
        Debug.Log("PointerUp hit");

        // ignore if finger moved too far (a swipe/drag is detected)
        if (Vector2.Distance(downPos, eventData.position) > tapMoveTolerance)
        {
            // do nothing
            return;
        }

        // get the current time
        float now = Time.unscaledTime;

        // check if the last tap time is within the multi tap window
        if (now - lastTapTime <= multiTapWindow)
        {
            // log the number of taps detected
            Debug.Log("TAP COUNT: " + tapCount);

            // if the last tap time is within the multi tap window, then can be a double+ tap
            // increment the tap count
            tapCount++;
        }
        else
        {
            // if the last tap time is not within the multi tap window, then it is a single tap
            // reset the tap count
            tapCount = 1;
        }

        // set the last tap time to the current time
        lastTapTime = now;

        // check if there is a pending tap resolution routine
        if (resolveTapRoutine != null)
        {
            // stop the pending tap resolution routine
            StopCoroutine(resolveTapRoutine);
        }

        // start a new tap resolution routine
        resolveTapRoutine = StartCoroutine(ResolveTapRoutine());
    }

    // helper function to resolve the tap type after a delay
    IEnumerator ResolveTapRoutine()
    {
        // wait for the multi tap window to expire
        yield return new WaitForSecondsRealtime(multiTapWindow);

        // double tap = help menu (controls, reset pools button, quit, X)
        // check if the exact tap count is 2 for a double tap
        if (tapCount == 2)
        {
            // confirm that it is a double tap
            Debug.Log("DOUBLE TAP DETECTED — help menu");

            // invoke the double tap event (trigger the Controls overlay toggle)
            //onDoubleTap?.Invoke();

            // reset after double tap action
            //tapCount = 0;

            var menu = helpMenu != null
                ? helpMenu
                : FindAnyObjectByType<HelpMenu>(FindObjectsInactive.Include);
            if (menu != null)
                menu.Open();
        }
        // check if the tap count is 3 or more for a triple tap
        // triple tap = reroll mares + stallions for this session
        else if (tapCount >= 3)
        {
            // confirm that it is a triple tap
            Debug.Log("TRIPLE TAP DETECTED — reset pools");

            // triple tap will trigger the reroll of available breeding horse pools
            // regenerate the possible parent horses
            //session.RegeneratePools();

            // reset after triple tap action
            //tapCount = 0;

            if (session != null)
                session.RegeneratePools();
        }

        // reset after tap action is resolved
        tapCount = 0;
        // reset the tap resolution routine
        resolveTapRoutine = null;
    }
}