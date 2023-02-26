using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a GameObject on screen.
/// Note that this ONLY works for an orthographic Main Camera at [0,0,0].
/// </summary>
public class BoundsCheck : MonoBehaviour {

    [System.Flags]
    public enum eScreenLocs
    {
        onScreen = 0,
        offRight = 1,
        offLeft  = 2,
        offUp    = 4,
        offDown  = 8
    }

    public enum eType { center, inset, outset};

    [Header("Inscribed")]
    public eType boundsType = eType.center;
    public float radius = 1f;
    public bool keepOnScreen = true;


    [Header("Dynamic")]
    public eScreenLocs screenLocs = eScreenLocs.onScreen;

    public float camWidth;
    public float camHeight;
    [HideInInspector]
    public bool offRight, offLeft, offUp, offDown;

	void Awake()
    {
        camHeight = Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
    }

    void LateUpdate()
    {

        float checkRadius = 0;
        if (boundsType == eType.inset) checkRadius = -radius;
        if (boundsType == eType.outset) checkRadius = radius;

        Vector3 pos = transform.position;
        screenLocs = eScreenLocs.onScreen;
       

        if (pos.x > camWidth + checkRadius)
        {
            pos.x = camWidth - checkRadius;
            screenLocs |= eScreenLocs.offRight; 
        }

        if (pos.x < -camWidth + checkRadius)
        {
            pos.x = -camWidth + checkRadius;
            screenLocs |= eScreenLocs.offLeft;
        }

        if (pos.y > camHeight + checkRadius)
        {
            pos.y = camHeight - checkRadius;
            screenLocs |= eScreenLocs.offUp;
        }

        if (pos.y < -camHeight - checkRadius)
        {
            pos.y = -camHeight + checkRadius;
            screenLocs |= eScreenLocs.offDown;
        }

        if (keepOnScreen && !isOnScreen)
        {
            transform.position = pos;
            screenLocs = eScreenLocs.onScreen;
        }


    }


    public bool isOnScreen
    {
        get { return (screenLocs == eScreenLocs.onScreen); }
    }


    public bool LocIs(eScreenLocs checkLoc)
    {
        if (checkLoc == eScreenLocs.onScreen) return isOnScreen;
        return ((screenLocs & checkLoc) == checkLoc);
    }


    // Draw the bounds in the Scene pane using OnDrawGizmos()
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Vector3 boundSize = new Vector3(camWidth * 2, camHeight * 2, 0.1f);
        Gizmos.DrawWireCube(Vector3.zero, boundSize);
    }
}
