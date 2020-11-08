using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using NativeWebSocket;
using TMPro;
using System;
using System.Threading.Tasks;

namespace Rufus31415.WebXR.Demo
{
    public class SpectatorViewWebXRClient : MonoBehaviour
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

        WebSocket websocket;

        private async Task InitWebSocket()
        {
            if (websocket != null)
            {
                await websocket.Close();
            }

            websocket = new WebSocket($"ws://{Host}:8090");

            websocket.OnOpen += () =>
            {
                Debug.Log("Connection opened !");
            };

            websocket.OnError += (e) =>
            {
                Debug.Log("Error! " + e);
            };

            websocket.OnClose += (e) =>
            {
                Debug.Log("Connection closed!");
            };

            websocket.OnMessage += (bytes) =>
            {
                try
                {
                    if (bytes.Length == 7 && bytes[0] == 0x12 && bytes[1] == 0x06 && bytes[2] == 0x92)
                    {
                        var imageLength = bytes[3] + (bytes[4] << 8) + (bytes[5] << 16) + (bytes[6] << 24);

                    //  Debug.Log("image Length:" + imageLength);
                    _tempPng = new byte[imageLength];
                        _tempPngId = 0;
                    }
                    else
                    {
                        Array.Copy(bytes, 0, _tempPng, _tempPngId, bytes.Length);
                        _tempPngId += bytes.Length;

                        if (_tempPngId >= _tempPng.Length)
                        {
                            _png = _tempPng;
                            _tmpPngAvailable = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            };

            // waiting for messages
            await websocket.Connect();
        }


        private async void Start()
        {
            Host = PlayerPrefs.GetString("ip", "localhost");

            _lowQuality = PlayerPrefs.GetInt("Low quality", 1) != 0;


            StartCoroutine(PollImage());

            await InitWebSocket();
        }

        private bool _tmpPngAvailable;
        private int _tempPngId;
        private byte[] _tempPng;

        private Texture2D _texture;

        private bool _followMode = true;

        private bool _lowQuality;

        private byte[] _png;

        private float _fpsStartT;

        private async void OnGUI()
        {
            var w = Screen.width;
            var h = Screen.height;

            int buttonW = w / 8;
            int buttonH = buttonW / 2;

            if (_png != null && _tmpPngAvailable)
            {
                try
                {
                    if (!_texture || _texture.width != w || _texture.height != h)
                    {
                        if (_texture) Destroy(_texture);

                        _texture = new Texture2D(w, h);
                    }

                    _texture.LoadImage(_png);

                    _tmpPngAvailable = false;

                    _deltaT[_iDeltaT++] = Time.time - _fpsStartT;
                    if (_iDeltaT >= _deltaT.Length) _iDeltaT = 0;
                    _fpsStartT = Time.time;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if(websocket?.State != WebSocketState.Open)
            {
                var style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, 0, w, h), "Please click \"Set headset IP or name\" and enter IP or name of a Hololens 2 that runs an app with SpectatorViewHololens2Server MonoBehavior.", style);
            }

            if (_texture) GUI.DrawTexture(new Rect(0, 0, w, h), _texture);

            if (GUI.Toggle(new Rect(3 * buttonW, 0, buttonW, buttonH), _lowQuality, "Low quality") != _lowQuality)
            {
                _lowQuality = !_lowQuality;
                PlayerPrefs.SetInt("lowQuality", _lowQuality ? 1 : 0);
                PlayerPrefs.Save();
            }

            if (SimpleWebXR.InSession) return;

            var deltaTAvg = _deltaT.Average();
            var fps = deltaTAvg == 0 ? 0 : 1 / deltaTAvg;

            GUI.TextArea(new Rect(0, 0, buttonW, buttonH), $"{(int)fps} FPS\r\n{Host ?? ""}");

            if (GUI.Button(new Rect(2 * buttonW, 0, buttonW, buttonH), _followMode ? "[FOLLOWING]" : "Start follow...\r\n[)-)"))
            {
                _followMode = true;
            }

            if (GUI.Button(new Rect(buttonW, 0, buttonW, buttonH), "Set headset\r\nIP or name"))
            {
                var value = Prompt("Enter Headset IP or name", Host);
                PlayerPrefs.SetString("ip", value);
                PlayerPrefs.Save();
                Host = value;

                await InitWebSocket();
            }

        }

        private int _iDeltaT = 0;
        private readonly float[] _deltaT = new float[4];

        IEnumerator PollImage()
        {
            while (true)
            {
                if (string.IsNullOrEmpty(Host))
                {
                    yield return null;
                }
                else
                {
                    var data = new RequestImageJSon();

                    if (_lowQuality)
                    {
                        data.w = Screen.width / 4;
                        data.h = Screen.height / 4;
                    }
                    else
                    {
                        data.w = Screen.width;
                        data.h = Screen.height;
                    }

                    data.p = Camera.main.transform.position;
                    data.r = Camera.main.transform.eulerAngles;
                    data.m = Camera.main.projectionMatrix;
                    data.f = !SimpleWebXR.InSession && _followMode;

#if UNITY_EDITOR
                    data.t = SimulateTouch;
#else
                    data.t = SimpleWebXR.InSession && Input.touchCount > 0;
#endif

                    var json = JsonUtility.ToJson(data);

                    if (websocket != null && websocket.State == WebSocketState.Open)
                    {
                        websocket.SendText(json);
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private Vector2? _touchStart;

        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket?.DispatchMessageQueue();
#endif


            if (SimpleWebXR.InSession) return;

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
                Rotate(Input.GetAxis("Mouse Y") * dt * 100, Input.GetAxis("Mouse X") * dt * 100);
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
            if (x != 0 || y != 0 || z != 0)
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
        public Vector3 p, r;
        public Matrix4x4 m;
        public bool f;
        public bool t;
    }
}