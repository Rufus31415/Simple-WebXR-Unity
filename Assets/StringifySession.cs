using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringifySession : MonoBehaviour
{
    public TextMesh descriptionText;

    private SimpleWebXR _session;
    void Start()
    {
        _session = SimpleWebXR.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        if (!descriptionText) return;

        descriptionText.text = _session?.ToString();
    }
}
