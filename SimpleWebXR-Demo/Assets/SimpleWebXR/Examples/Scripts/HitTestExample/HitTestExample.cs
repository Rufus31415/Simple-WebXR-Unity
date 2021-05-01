using Rufus31415.WebXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitTestExample : MonoBehaviour
{
    public GameObject Reticle;

    public Text Text;

    // Start is called before the first frame update
    void Start()
    {
        SimpleWebXR.SessionStart.AddListener(SimpleWebXR.HitTestStart);
    }

    // Update is called once per frame
    void Update()
    {
        if (!SimpleWebXR.InSession || !Reticle) return;

        if (!SimpleWebXR.HitTestInProgress)
        {
            Text.text = "Hit test failed\r\n\r\n" + (SimpleWebXR.HitTestSupported ? "Your device supports hit test" : "Your device doesn't support hit test");
        }
        else
        {
            Text.text = "Hit test in progress at :\r\n x : " + SimpleWebXR.HitTestPosition.x + "\r\n y : " + SimpleWebXR.HitTestPosition.y + "\r\n z : " + SimpleWebXR.HitTestPosition.z;


            Reticle.transform.position = SimpleWebXR.HitTestPosition;
            Reticle.transform.rotation = SimpleWebXR.HitTestRotation;
        }
    }
}
