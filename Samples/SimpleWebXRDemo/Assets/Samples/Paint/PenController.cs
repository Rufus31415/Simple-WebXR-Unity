using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenController : MonoBehaviour
{
    private SimpleWebXR _session;
    private WebXRInput _input;
    private MeshRenderer _renderer;

    public LineRenderer LinePrefab;

    public bool IsLeft;

    void Start()
    {
        _session = SimpleWebXR.GetInstance();

        if (!_session) return;

        _input = IsLeft ? _session.LeftInput : _session.RightInput;

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
        if (!_renderer || !_session) return;

        _renderer.enabled = _session.InSession && _input.Available && _input.IsPositionValid;

        transform.position = _input.Position;
        transform.rotation = _input.Rotation;
    }
}
