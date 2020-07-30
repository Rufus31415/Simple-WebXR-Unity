using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class SpectatorViewClient : MonoBehaviour
{
    public string Host;

#if UNITY_EDITOR
    public bool SimulateTouch;
#endif


#if !UNITY_EDITOR && UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string Prompt(string message, string defaultValue);
#else
    private static string Prompt(string message, string defaultValue) { return defaultValue; }
#endif

    private SimpleWebXR _xr;

    private void Start()
    {
        _xr = SimpleWebXR.GetInstance();

        Host = PlayerPrefs.GetString("ip", "localhost");

        StartCoroutine(PollImage());
    }

    private Texture2D _texture;

    private bool _followMode = true;

    private void OnGUI()
    {
        if (_texture)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);
        }

        if (_xr.InSession) return;
        var deltaTAvg = _deltaT.Average();
        var fps = deltaTAvg == 0 ? 0 : 1 / deltaTAvg;

        GUI.TextArea(new Rect(0, 0, 100, 40), $"{(int)fps} FPS\r\n{Host ?? ""}");

        if (GUI.Button(new Rect(100, 0, 100, 40), "Set headset\r\nIP nor name"))
        {
            var value = Prompt("Enter Headset IP or name", Host);
            PlayerPrefs.SetString("ip", value);
            PlayerPrefs.Save();
            Host = value;
        }

        if (GUI.Button(new Rect(200, 0, 100, 40), _followMode ? "[FOLLOWING]" : "Start follow...\r\n[)-)"))
        {
            _followMode = true;
        }
    }

    private int _iDeltaT = 0;
    private readonly float[] _deltaT = new float[4];

    IEnumerator PollImage()
    {
        yield return null;

        var i = 0;

        while (true)
        {
            if (string.IsNullOrEmpty(Host))
            {
                yield return null;
            }
            else
            {
                var startT = Time.time;

                var w = Screen.width;
                var h = Screen.height;

                var data = new RequestImageJSon();

                data.w = w;
                data.h = h;
                data.p = Camera.main.transform.position;
                data.r = Camera.main.transform.eulerAngles;
                data.m = Camera.main.projectionMatrix;
                data.f = !_xr.InSession && _followMode;

#if UNITY_EDITOR
                data.t = SimulateTouch;
#else
                data.t = _xr.InSession && Input.touchCount > 0;
#endif

                var json = JsonUtility.ToJson(data);

                using (var req = UnityWebRequest.Post($"http://{Host}:8090/?t={i}", json))
                {
                    req.timeout = 1;
                    yield return req.SendWebRequest();

                    if (string.IsNullOrEmpty(req.error))
                    {
                        if (req.downloadHandler.data.Length == 0)
                        {
                            Debug.LogWarning("Image with size 0 received");
                        }
                        else
                        {
                            if (!_texture || _texture.width != w || _texture.height != h)
                            {
                                if (_texture) DestroyImmediate(_texture);

                                _texture = new Texture2D(w, h);
                            }
                            _texture.LoadImage(req.downloadHandler.data);
                        }
                    }
                    else
                    {
                        Debug.LogError(req.error);
                    }

                }

                _deltaT[_iDeltaT++] = Time.time - startT;
                if (_iDeltaT >= _deltaT.Length) _iDeltaT = 0;
            }
        }
    }

    private Vector2? _touchStart;

    void Update()
    {
        if (_xr.InSession) return;

        var dt = Time.deltaTime;

        if (Input.touchSupported && Input.touchCount == 2)
        {

            // get current touch positions
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);

            // get touch position from the previous frame
            Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
            Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

            float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
            float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

            // get offset value
            float deltaDistance = oldTouchDistance - currentTouchDistance;
            Translate(0, 0, deltaDistance * 0.01f);
        }
        else if (Input.touchSupported && Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            var pose = touch.position;
            if (touch.phase == TouchPhase.Moved)
            {
                if (_touchStart == null) _touchStart = pose;

                var delta = (pose - (Vector2)_touchStart) * dt;
                _touchStart = pose;
                Rotate(delta.y, delta.x);
            }
        }
        else
        {
            _touchStart = null;
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Rotate(Input.GetAxis("Mouse Y")* dt * 100, Input.GetAxis("Mouse X")* dt * 100);
        }

        Translate(0, 0, Input.GetAxis("Mouse ScrollWheel"));

        if (Input.GetKey(KeyCode.DownArrow)) Translate(0, 0, -0.1f * dt);
        else if (Input.GetKey(KeyCode.UpArrow)) Translate(0, 0, 0.1f * dt);

        if (Input.GetKey(KeyCode.LeftArrow)) Translate(-0.1f * dt, 0, 0);
        else if (Input.GetKey(KeyCode.RightArrow)) Translate(0.1f * dt, 0, 0);

        var rotation = Camera.main.transform.rotation.eulerAngles;
        Camera.main.transform.eulerAngles = new Vector3(rotation.x, rotation.y, 0);
    }

    void Translate(float x, float y, float z)
    {
        if (x!=0 || y!=0 || z!=0)
        {
            _followMode = false;
            Camera.main.transform.Translate(x, y, z);
        }
    }

    void Rotate(float rx, float ry)
    {
        if (rx != 0 || ry != 0)
        {
            _followMode = false;
            Camera.main.transform.Rotate(new Vector3(-rx, ry, 0));
        }
    }
}

public class RequestImageJSon
{
    public int w, h;
    public Vector3 p,r;
    public Matrix4x4 m;
    public bool f;
    public bool t;
}
