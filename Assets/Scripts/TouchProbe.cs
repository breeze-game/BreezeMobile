using UnityEngine;

public class TouchProbe : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // if no touches, return
        if (Input.touchCount == 0) return;

        // get the first touch
        Touch touch = Input.GetTouch(0);

        // log the touch phase
        if (touch.phase == TouchPhase.Began) Debug.Log($"Touch began at {touch.position}");
        if (touch.phase == TouchPhase.Moved) Debug.Log($"Touch delta {touch.deltaPosition}");
        if (touch.phase == TouchPhase.Ended) Debug.Log($"Touch ended");
        
        
    }
}
