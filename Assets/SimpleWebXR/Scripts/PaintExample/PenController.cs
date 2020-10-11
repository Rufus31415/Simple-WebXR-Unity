using System.Collections;
using UnityEngine;

namespace Rufus31415.WebXR.Demo
{
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

        private void OnGUI()
        {
            if (_xr.InSession && IsLeft) return;

            var style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Once the WebXR session is started, you can draw 3D lines with your controllers, or your touch screen on mobiles.", style);
        }
    }
}