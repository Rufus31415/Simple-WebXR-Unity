using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetDeviceOrientation : MonoBehaviour
{
    private SimpleWebXR _session;
    private Text _text;

    void Start()
    {
        _session = SimpleWebXR.GetInstance();

        _text = GetComponentInChildren<Text>();
    }

    void Update()
    {
        if (!_session || !_text) return;

        if (_session.GetDeviceOrientation(out float alpha, out float beta, out float gamma))
        {
            this.gameObject.transform.eulerAngles = new Vector3(-beta, alpha, -gamma);

            _text.text = $"a: {alpha.ToString("0.0")}\n\nb: {beta.ToString("0.0")}\n\ng: {gamma.ToString("0.0")}";
        }
    }
}
