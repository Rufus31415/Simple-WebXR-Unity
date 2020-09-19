using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenController : MonoBehaviour
{
    private SimpleWebXR _xr;
    private WebXRInput _input;
    private MeshRenderer _renderer;

    public LineRenderer LinePrefab;

    public bool IsLeft;

    void Start()
    {
        _xr = SimpleWebXR.GetInstance();

        if (!_xr) return;

        _input = IsLeft ? _xr.LeftInput : _xr.RightInput;

        _input.SelectStart.AddListener(() => StartCoroutine(DrawCoroutine()));
        _input.SelectEnd.AddListener(StopAllCoroutines);

        _renderer = GetComponent<MeshRenderer>();
    }

    public IEnumerator DrawCoroutine()
    {
       var line = Instantiate(LinePrefab);
        line.positionCount = 0;

        while (true)
        {
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, _input.Position);

            yield return new WaitForSeconds(.1f);
        }
    }

    void LateUpdate()
    {
        if (!_renderer || !_xr) return;

        _renderer.enabled = _xr.InSession && _input.Available && _input.IsPositionTracked;

        transform.position = _input.Position;
        transform.rotation = _input.Rotation;
    }
}
