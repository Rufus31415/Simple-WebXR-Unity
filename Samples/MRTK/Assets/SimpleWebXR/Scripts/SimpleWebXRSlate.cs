using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWebXRSlate : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Description;
    public TMPro.TextMeshProUGUI SelectLeft;
    public TMPro.TextMeshProUGUI SelectRight;
    public TMPro.TextMeshProUGUI SqueezeLeft;
    public TMPro.TextMeshProUGUI SqueezeRight;

    private SimpleWebXR _session;
    void Start()
    {
        _session = SimpleWebXR.GetInstance();

        if (!_session) return;

        _session.LeftInput.SelectStart.AddListener(() => SelectLeft.text = "LEFT");
        _session.LeftInput.SelectEnd.AddListener(() => SelectLeft.text = "---");
        _session.LeftInput.SqueezeStart.AddListener(() => SqueezeLeft.text = "LEFT");
        _session.LeftInput.SqueezeEnd.AddListener(() => SqueezeLeft.text = "---");

        _session.RightInput.SelectStart.AddListener(() => SelectRight.text = "RIGHT");
        _session.RightInput.SelectEnd.AddListener(() => SelectRight.text = "---");
        _session.RightInput.SqueezeStart.AddListener(() => SqueezeRight.text = "RIGHT");
        _session.RightInput.SqueezeEnd.AddListener(() => SqueezeRight.text = "---");
    }

    void Update()
    {
        if (!Description) return;

        Description.text = _session?.ToString();
    }
}
