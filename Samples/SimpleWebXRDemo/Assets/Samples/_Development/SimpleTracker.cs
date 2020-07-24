using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTracker : MonoBehaviour
{
    public SimpleWebXR SimpleWebXR;

    public WebXRHandedness Handedness;

    private WebXRInput _input;
    
    // Start is called before the first frame update
    void Start()
    {
        _input = SimpleWebXR.GetInput(Handedness);

        _input.SelectStart.AddListener(() => gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f));
        _input.SelectEnd.AddListener(() => gameObject.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f));
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = _input.Position;
        gameObject.transform.rotation = _input.Rotation;
    }
}
