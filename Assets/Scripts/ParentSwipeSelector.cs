using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ParentSwipeSelector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // session reference
    [SerializeField] BreedingSession session;
    // true = mares, false = stallions (ignored if isOwnedPicker)
    [SerializeField] bool isMareIcon = true;
    // true = cycle owned horses (center column)
    [SerializeField] bool isOwnedPicker = false;
    // pixels needed before we call it a swipe not a tap
    [SerializeField] float swipeThresholdPx = 60f;

    // called after a successful swipe (refresh ascii etc.)
    Action onAfterSwipe;

    // screen position when pointer went down
    Vector2 pointerDownPos;

    // wire from code (BreedingUIBuilder calls this)
    public void Configure(BreedingSession s, bool mareIcon, bool ownedPicker, Action afterSwipe)
    {
        session = s;
        isMareIcon = mareIcon;
        isOwnedPicker = ownedPicker;
        onAfterSwipe = afterSwipe;
    }

    // remember start of gesture
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPos = eventData.position;
    }

    // decide swipe vs tap and cycle the right list
    public void OnPointerUp(PointerEventData eventData)
    {
        if (session == null) return;

        // total movement while finger was down
        Vector2 delta = eventData.position - pointerDownPos;

        // too small = tap, ignore here (background handles multi-tap)
        if (Mathf.Abs(delta.x) < swipeThresholdPx)
            return;

        // swipe right = next index, left = previous
        int direction = delta.x > 0 ? 1 : -1;

        if (isOwnedPicker)
            session.CycleOwned(direction);
        else if (isMareIcon)
            session.CycleMare(direction);
        else
            session.CycleStallion(direction);

        // tell ui to refresh text
        onAfterSwipe?.Invoke();
    }
}
