using System;
using System.Runtime.InteropServices; // for DllImport
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Rufus31415.WebXR
{
    /// <summary>
    /// Exposes a simple behavior that allows any WebGL application built with Unity to implement WebXR.
    /// This behavior will display an "Enter AR" GUI button that will launch the WebXR session (see also SimpleWebXR.jslib and SimpleWebXR.jspre). 
    /// The data (projection matrix, position and rotation) are shared via a buffer.
    /// When the WebXR session is started, this behavior modifies the position and characteristics of the main camera to allow the augmented reality experience.
    /// </summary>
    public class SimpleWebXR : MonoBehaviour
    {
        #region Configuration
        /// <summary>
        /// Do not show the GUI button "Start AR" or "Start VR".
        /// </summary>
        public bool HideStartButton;

        #endregion

        #region Public XR state
        /// <summary>
        /// 
        /// </summary>
        public WebXRInput LeftInput = new WebXRInput(WebXRHandedness.Left);

        /// <summary>
        /// 
        /// </summary>
        public WebXRInput RightInput = new WebXRInput(WebXRHandedness.Right);


        public WebXRInputEvent InputSourceSelect = new WebXRInputEvent();
        public WebXRInputEvent InputSourceSqueeze = new WebXRInputEvent();
        public WebXRInputEvent InputSourceSelectStart = new WebXRInputEvent();
        public WebXRInputEvent InputSourceSelectEnd = new WebXRInputEvent();
        public WebXRInputEvent InputSourceSqueezeStart = new WebXRInputEvent();
        public WebXRInputEvent InputSourceSqueezeEnd = new WebXRInputEvent();
        public UnityEvent InputSourcesChange = new UnityEvent();


        /// <summary>
        /// Indicates if a XR session is running
        /// </summary>
        public bool InSession { get; private set; }

        /// <summary>
        /// User height in meter (startup distance from floor to device)
        /// </summary>
        public float UserHeight => _dataArray[100];

        /// <summary>
        /// Camera that renders Left eye
        /// </summary>
        public Camera LeftEye => _cameras[0];

        /// <summary>
        /// Camera that renders Right eye
        /// </summary>
        public Camera RightEye => _cameras[1];

        #endregion

        // Shared float array with javascript.
        // [0] -> [15] : projection matrix of view 1
        // [16], [17], [18] : X, Y, Z position in m  of view 1
        // [19], [20], [21], [22] : RX, RY, RZ, RW rotation (quaternion)  of view 1
        // [23] -> [26] : Viewport X, Y, width, height  of view 1
        // [27] -> [42] : projection matrix of view 2
        // [43], [44], [45] : X, Y, Z position in m  of view 2
        // [46], [47], [48], [49] : RX, RY, RZ, RW rotation (quaternion)  of view 2
        // [50] -> [53] : Viewport X, Y, width, height  of view 2
        // [54] -> [60] : Left input x, y, z, rx, ry, rz, rw
        // [61] -> [68] : left input axes
        // [69] -> [76] : left gamepad value
        // [77] -> [83] : right input x, y, z, rx, ry, rz, rw
        // [84] -> [91] : right input axes
        // [92] -> [99] : right gamepad value
        // [100] : user height
        private readonly float[] _dataArray = new float[101];

        // Shared float array with javascript.
        // [0] : number of views (0 : session is stopped)
        // [1] : left controller events
        // [2] : right controller events
        // [3] : input change event
        // [4] : left input has position info
        // [5] : left input target ray mode
        // [6] : left input gamepad axes count
        // [7] : left input gamepad buttons count
        // [8] -> [15] : left inputs touched
        // [16] -> [23] : left inputs pressed
        // [24] : right input has position info
        // [25] : right input target ray mode
        // [26] : right input gamepad axes count
        // [27] : right input gamepad buttons count
        // [28] -> [35] : right inputs touched
        // [36] -> [43] : right inputs pressed
        // [44] : Left controller active
        // [45] : Right controller active
        // [46] : Left hand active
        // [47] : Right hand active
        private readonly byte[] _byteArray = new byte[48];

        // Hand data
        // [0] -> [7] : Left Wrist x, y, z, rx, ry, rz, zw, radius
        // [8] -> [199] : Left other fingers ...
        // [200] -> [399] : right wrist and fingers
        private readonly float[] _handData = new float[8 * 25 * 2];

        // Number of views (i.e. cameras)
        private WebXRViewEyes ViewEye => (WebXRViewEyes)_byteArray[0];

        // A session is running
        private bool InternalInSession => _byteArray[0] != 0;

        // Cameras created for each eyes ([0]:left, [1]:right)
        private readonly Camera[] _cameras = new Camera[2];


        public WebXRInput GetInput(WebXRHandedness handedness)
        {
            return handedness == WebXRHandedness.Left ? LeftInput : RightInput;
        }


        public static SimpleWebXR GetInstance()
        {
            return FindObjectOfType<SimpleWebXR>();
        }

        public static SimpleWebXR EnsureInstance()
        {
            var xr = GetInstance();

            if (xr) return xr;

            var newGameObject = new GameObject("WebXR");
            return newGameObject.AddComponent<SimpleWebXR>();
        }

#if UNITY_WEBGL && !UNITY_EDITOR // If executed in browser
    [DllImport("__Internal")]
    private static extern void InternalStartSession();

    [DllImport("__Internal")]
    private static extern void InternalEndSession();

    [DllImport("__Internal")]
    private static extern void InitWebXR(float[] dataArray, int length, byte[] byteArray, int _byteArrayLength, float[] handDataArray, int handDataArrayLength);

    [DllImport("__Internal")]
    public static extern bool IsArSupported();

    [DllImport("__Internal")]
    public static extern bool IsVrSupported();

    [DllImport("__Internal")]
    public static extern void InternalGetDeviceOrientation(float[] orientationArray, byte[] orientationInfo);

#else // if executed with Unity editor

        public bool SimulationIsArSupported = true;
        public bool SimulationIsVrSupported = false;
        public bool SimulationRender2Eyes = false;

        public WebXRTargetRayModes SimulationMode = WebXRTargetRayModes.TrackedPointer;

        public bool SimulationLeftSelect = false;
        public bool SimulationRightSelect = false;


        public float SimulationUserHeight = 1.8f;

        private GameObject _simulateLeft;
        private GameObject _simulateRight;
        private GameObject _simulateHead;

        private bool _sessionStarted;

        private void InternalStartSession()
        {
            _simulateLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _simulateLeft.name = "Simulation Left";
            _simulateLeft.transform.parent = gameObject.transform;
            _simulateLeft.transform.rotation = Camera.main.transform.rotation;
            _simulateLeft.transform.position = Camera.main.transform.position + new Vector3(-0.5f, -0.5f, 1);
            _simulateLeft.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f);

            _simulateRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _simulateRight.name = "Simulation Right";
            _simulateRight.transform.parent = gameObject.transform;
            _simulateRight.transform.rotation = Camera.main.transform.rotation;
            _simulateRight.transform.position = Camera.main.transform.position + new Vector3(0.5f, -0.5f, 1);
            _simulateRight.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f);

            _simulateHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _simulateHead.name = "Simulation Head";
            _simulateHead.transform.parent = gameObject.transform;
            _simulateHead.transform.rotation = Camera.main.transform.rotation;
            _simulateHead.transform.position = Camera.main.transform.position;
            _simulateHead.transform.localScale = new Vector3(0.1f, 0.1f, 0.2f);

            _sessionStarted = true;
        }

        private void InternalEndSession() { _sessionStarted = false; }


        private void InitWebXR(float[] dataArray, int length, byte[] _byteArray, int _byteArrayLength, float[] handDataArray, int handDataArrayLength) { }

        public bool IsArSupported()
        {
            return SimulationIsArSupported;
        }

        public bool IsVrSupported()
        {
            return SimulationIsVrSupported;
        }

        public static void InternalGetDeviceOrientation(float[] orientationArray, byte[] orientationInfo)  { }

        private void Update()
        {
            // [0] : number of views (0 : session is stopped)
            _byteArray[0] = (byte)(_sessionStarted ? (SimulationRender2Eyes ? WebXRViewEyes.Both : WebXRViewEyes.Left) : WebXRViewEyes.None);


            if (_sessionStarted)
            {
                // [0] -> [15] : projection matrix of view 1
                var pm = Camera.main.projectionMatrix;
                _dataArray[0] = pm.m00;
                _dataArray[4] = pm.m01;
                _dataArray[8] = pm.m02;
                _dataArray[12] = pm.m03;
                _dataArray[1] = pm.m10;
                _dataArray[5] = pm.m11;
                _dataArray[9] = pm.m12;
                _dataArray[13] = pm.m13;
                _dataArray[2] = pm.m20;
                _dataArray[6] = pm.m21;
                _dataArray[10] = pm.m22;
                _dataArray[14] = pm.m23;
                _dataArray[3] = pm.m30;
                _dataArray[7] = pm.m31;
                _dataArray[11] = pm.m32;
                _dataArray[15] = pm.m33;

                // [16], [17], [18] : X, Y, Z position in m  of view 1
                _dataArray[16] = _simulateHead.transform.position.x;
                _dataArray[17] = _simulateHead.transform.position.y;
                _dataArray[18] = -_simulateHead.transform.position.z;

                // [19], [20], [21], [22] : RX, RY, RZ, RW rotation (quaternion)  of view 1
                var rotation = ToUnityRotation(_simulateHead.transform.rotation);
                _dataArray[19] = rotation.x;
                _dataArray[20] = rotation.y;
                _dataArray[21] = rotation.z;
                _dataArray[22] = rotation.w;

                // [23] -> [26] : Viewport X, Y, width, height  of view 1
                _dataArray[23] = 0;
                _dataArray[24] = 0;
                _dataArray[25] = 1;
                _dataArray[26] = 1;

                if (SimulationRender2Eyes)
                {
                    _dataArray[25] = 0.5f;

                    // [27] -> [42] : projection matrix of view 2
                    // [43], [44], [45] : X, Y, Z position in m  of view 2
                    // [46], [47], [48], [49] : RX, RY, RZ, RW rotation (quaternion)  of view 2
                    for (int i = 0; i <= 26; i++) _dataArray[27 + i] = _dataArray[i];

                    // [50] -> [53] : Viewport X, Y, width, height  of view 2
                    _dataArray[50] = 0.5f;
                }



                // [54] -> [60] : Left input x, y, z, rx, ry, rz, rw
                rotation = ToUnityRotation(_simulateLeft.transform.rotation);
                _dataArray[54] = _simulateLeft.transform.position.x;
                _dataArray[55] = _simulateLeft.transform.position.y;
                _dataArray[56] = -_simulateLeft.transform.position.z;
                _dataArray[57] = rotation.x;
                _dataArray[58] = rotation.y;
                _dataArray[59] = rotation.z;
                _dataArray[60] = rotation.w;

                // [77] -> [83] : right input x, y, z, rx, ry, rz, rw
                rotation = ToUnityRotation(_simulateRight.transform.rotation);
                _dataArray[77] = _simulateRight.transform.position.x;
                _dataArray[78] = _simulateRight.transform.position.y;
                _dataArray[79] = -_simulateRight.transform.position.z;
                _dataArray[80] = rotation.x;
                _dataArray[81] = rotation.y;
                _dataArray[82] = rotation.z;
                _dataArray[83] = rotation.w;

                // [100] : user height
                _dataArray[100] = SimulationUserHeight;

                // [4] : left input has position info
                _byteArray[4] = 1;

                // [24] : right input has position info
                _byteArray[24] = 1;

                // [44] : Left controller active
                _byteArray[44] = 1;

                // [45] : Right controller active
                _byteArray[45] = 1;

                // [1] : left controller events
                if (SimulationLeftSelect && !LeftInput.Selected) _byteArray[1] = (byte)WebXRInputSourceEventTypes.SelectStart;
                else if (!SimulationLeftSelect && LeftInput.Selected) _byteArray[1] = (byte)WebXRInputSourceEventTypes.SelectEnd;
                else _byteArray[1] = 0;

                // [2] : right controller events
                if (SimulationRightSelect && !RightInput.Selected) _byteArray[2] = (byte)WebXRInputSourceEventTypes.SelectStart;
                else if (!SimulationRightSelect && RightInput.Selected) _byteArray[2] = (byte)WebXRInputSourceEventTypes.SelectEnd;
                else _byteArray[2] = 0;

                // [5] : left input target ray mode
                _byteArray[5] = (byte)SimulationMode;

                // [25] : right input target ray mode
                _byteArray[25] = _byteArray[5];

            }

        }
#endif

        // Share _dataArray and init WebXR
        void Start()
        {
            InitWebXR(_dataArray, _dataArray.Length, _byteArray, _byteArray.Length, _handData, _handData.Length);
        }

        // Display "Enter AR" button if WebXR immersive AR is supported
        private void OnGUI()
        {
            if (HideStartButton) return;

            if (!InternalInSession)
            {
                var width = 120;
                var height = 60;

                if ((IsArSupported() || IsVrSupported()))
                {
                    if (GUI.Button(new Rect((Screen.width - width) / 2, Screen.height - height, width, height), "Enter " + (IsArSupported() ? "AR" : "VR")))
                    {
                        StartSession();
                    }
                }
                else
                {
                    GUI.Button(new Rect((Screen.width - width) / 2, Screen.height - height, width, height), "WebXR\r\nNot supported");
                }
            }
        }

        // Starts a new session
        public void StartSession()
        {
            if (!IsArSupported() && !IsVrSupported()) return;

            InternalStartSession();
        }

        // Ends the session
        public void EndSession()
        {
            if (!InternalInSession) return;

            InternalEndSession();
        }


        private void UpdateCamera(WebXRViewEyes eye)
        {
            var id = (int)eye - 1;

            // If the camera for this id should not exist
            if ((ViewEye & eye) != eye)
            {
                if (_cameras[id])
                {
                    Destroy(_cameras[id].gameObject);
                    _cameras[id] = null;
                }

                return;
            }

            // Create camera
            if (!_cameras[id])
            {
                if (id > 0)
                {
                    var camGameObject = new GameObject("WebXRCamera_" + id);
                    camGameObject.transform.parent = gameObject.transform;
                    _cameras[id] = camGameObject.AddComponent<Camera>();
                    _cameras[id].depth = Camera.main.depth - 1;
                }
                else
                {
                    _cameras[0] = Camera.main;
                }
                _cameras[id].clearFlags = CameraClearFlags.SolidColor;
                _cameras[id].backgroundColor = new Color(0, 0, 0, 0);
                _cameras[id].nearClipPlane = 0.1f;
                _cameras[id].farClipPlane = 1000;
            }

            var floatStartId = id * 27;

            var rect = new Rect(_dataArray[floatStartId + 23], _dataArray[floatStartId + 24], _dataArray[floatStartId + 25], _dataArray[floatStartId + 26]);

            if (id > 0)
            {
                if (_cameras[0] && _cameras[0].rect == rect)
                {
                    _cameras[id].gameObject.SetActive(false);
                    return;
                }
                else
                {
                    _cameras[id].gameObject.SetActive(true);
                }
            }

            _cameras[id].rect = rect;

            // Get and transpose projection matrix
            var pm = new Matrix4x4();
            pm.m00 = _dataArray[floatStartId + 0];
            pm.m01 = _dataArray[floatStartId + 4];
            pm.m02 = _dataArray[floatStartId + 8];
            pm.m03 = _dataArray[floatStartId + 12];
            pm.m10 = _dataArray[floatStartId + 1];
            pm.m11 = _dataArray[floatStartId + 5];
            pm.m12 = _dataArray[floatStartId + 9];
            pm.m13 = _dataArray[floatStartId + 13];
            pm.m20 = _dataArray[floatStartId + 2];
            pm.m21 = _dataArray[floatStartId + 6];
            pm.m22 = _dataArray[floatStartId + 10];
            pm.m23 = _dataArray[floatStartId + 14];
            pm.m30 = _dataArray[floatStartId + 3];
            pm.m31 = _dataArray[floatStartId + 7];
            pm.m32 = _dataArray[floatStartId + 11];
            pm.m33 = _dataArray[floatStartId + 15];

            _cameras[id].projectionMatrix = pm;

            // Get position and rotation Z, RX and RY are inverted
            _cameras[id].transform.position = ToUnityPosition(_dataArray[floatStartId + 16], _dataArray[floatStartId + 17], _dataArray[floatStartId + 18]);
            _cameras[id].transform.rotation = ToUnityRotation(_dataArray[floatStartId + 19], _dataArray[floatStartId + 20], _dataArray[floatStartId + 21], _dataArray[floatStartId + 22]);
        }

        private void UpdateInput(WebXRInput inputSource)
        {
            var floatStartId = (int)inputSource.Handedness * 23 + 54;
            var byteStartId = (int)inputSource.Handedness * 20 + 4;

            inputSource.Position = ToUnityPosition(_dataArray[floatStartId + 0], _dataArray[floatStartId + 1], _dataArray[floatStartId + 2]);
            inputSource.Rotation = ToUnityRotation(_dataArray[floatStartId + 3], _dataArray[floatStartId + 4], _dataArray[floatStartId + 5], _dataArray[floatStartId + 6]);
            inputSource.IsPositionTracked = (_byteArray[byteStartId + 0] != 0);

            var eventId = (int)inputSource.Handedness + 1;
            var mask = _byteArray[eventId];
            if (mask != 0)
            {
                RaiseInputSourceEvent(mask, WebXRInputSourceEventTypes.Select, InputSourceSelect, inputSource.Select, inputSource);
                RaiseInputSourceEvent(mask, WebXRInputSourceEventTypes.SelectEnd, InputSourceSelectEnd, inputSource.SelectEnd, inputSource);
                RaiseInputSourceEvent(mask, WebXRInputSourceEventTypes.SelectStart, InputSourceSelectStart, inputSource.SelectStart, inputSource);
                RaiseInputSourceEvent(mask, WebXRInputSourceEventTypes.Squeeze, InputSourceSqueeze, inputSource.Squeeze, inputSource);
                RaiseInputSourceEvent(mask, WebXRInputSourceEventTypes.SqueezeEnd, InputSourceSqueezeEnd, inputSource.SqueezeEnd, inputSource);
                RaiseInputSourceEvent(mask, WebXRInputSourceEventTypes.SqueezeStart, InputSourceSqueezeStart, inputSource.SqueezeStart, inputSource);

                _byteArray[eventId] = 0;
            }

            inputSource.TargetRayMode = (WebXRTargetRayModes)_byteArray[byteStartId + 1];

            inputSource.AxesCount = _byteArray[byteStartId + 2];
            for (int i = 0; i < WebXRInput.AXES_BUTTON_COUNT; i++)
            {
                inputSource.Axes[i] = _dataArray[floatStartId + 7 + i];
            }

            inputSource.ButtonsCount = _byteArray[byteStartId + 3];
            for (int i = 0; i < WebXRInput.AXES_BUTTON_COUNT; i++)
            {
                var button = inputSource.Buttons[i];
                button.Value = _dataArray[floatStartId + 15 + i];
                button.Touched = _byteArray[byteStartId + 4 + i] != 0;
                button.Pressed = _byteArray[byteStartId + 12 + i] != 0;
            }

            var handAvailable = 0 != _byteArray[46 + (int)inputSource.Handedness];

            inputSource.Hand.Available = handAvailable;
            if (handAvailable)
            {
                for (int j = 0; j < 25; j++)
                {
                    var i = (int)inputSource.Handedness * 200 + j * 8;
                    var joint = inputSource.Hand.Joints[j];
                    joint.Position = ToUnityPosition(_handData[i], _handData[i + 1], _handData[i + 2]);
                    joint.Rotation = ToUnityRotation(_handData[i + 3], _handData[i + 4], _handData[i + 5], _handData[i + 6]);
                    joint.Radius = _handData[i + 7];
                }
            }

        }

        private void RaiseInputSourceEvent(byte mask, WebXRInputSourceEventTypes type, WebXRInputEvent webxrInputEvent, UnityEvent unityEvent, WebXRInput inputSource)
        {
            if (((WebXRInputSourceEventTypes)mask & type) == type)
            {
                switch (type)
                {
                    case WebXRInputSourceEventTypes.SelectStart:
                        inputSource.Selected = true;
                        break;
                    case WebXRInputSourceEventTypes.SelectEnd:
                        inputSource.Selected = false;
                        break;
                    case WebXRInputSourceEventTypes.SqueezeStart:
                        inputSource.Squeezed = true;
                        break;
                    case WebXRInputSourceEventTypes.SqueezeEnd:
                        inputSource.Squeezed = false;
                        break;
                }

                unityEvent.Invoke();
                webxrInputEvent.Invoke(inputSource);

                Debug.Log($"{inputSource.Handedness} : {type}");
            }
        }

        private static Vector3 ToUnityPosition(float x, float y, float z)
        {
            return new Vector3(x, y, -z);
        }
        private static Vector3 ToUnityPosition(Vector3 position)
        {
            return ToUnityPosition(position.x, position.y, position.z);
        }

        private static Quaternion ToUnityRotation(float x, float y, float z, float w)
        {
            return new Quaternion(-x, -y, z, w);
        }

        private static Quaternion ToUnityRotation(Quaternion rotation)
        {
            return ToUnityRotation(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        // Update camera position, rotation and projection
        void LateUpdate()
        {
            UpdateCamera(WebXRViewEyes.Left);
            UpdateCamera(WebXRViewEyes.Right);

            UpdateInput(LeftInput);
            UpdateInput(RightInput);

            LeftInput.Available = _byteArray[44] != 0;
            RightInput.Available = _byteArray[45] != 0;

            // Input source change event
            if (_byteArray[3] != 0)
            {
                InputSourcesChange.Invoke();
                _byteArray[3] = 0;
            }

            // Session state changed invoked when all gamepads and cameras are updated
            if (InternalInSession && !InSession) //New session detected
            {
                InSession = true;
                SessionStart.Invoke();
            }
            else if (InSession && !InternalInSession) // End of session detected
            {
                InSession = false;
                SessionEnd.Invoke();
            }
        }

        public readonly UnityEvent SessionStart = new UnityEvent();
        public readonly UnityEvent SessionEnd = new UnityEvent();


#if UNITY_EDITOR
        [UnityEditor.MenuItem("WebXR/Set camera none")]
        public static void SetCameraNone()
        {
            UnityEngine.Object.FindObjectOfType<SimpleWebXR>()._dataArray[0] = 0;
        }

        [UnityEditor.MenuItem("WebXR/Set camera left")]
        public static void SetCameraLeft()
        {
            UnityEngine.Object.FindObjectOfType<SimpleWebXR>()._dataArray[0] = 1;
        }

        [UnityEditor.MenuItem("WebXR/Set camera right")]
        public static void SetCameraRight()
        {
            UnityEngine.Object.FindObjectOfType<SimpleWebXR>()._dataArray[0] = 2;
        }

        [UnityEditor.MenuItem("WebXR/Set camera both")]
        public static void SetCameraBoth()
        {
            UnityEngine.Object.FindObjectOfType<SimpleWebXR>()._dataArray[0] = 3;
        }
#endif

        [System.Flags]
        private enum WebXRViewEyes
        {
            None = 0,
            Left = 1,
            Right = 2,
            Both = Left | Right
        }


        [System.Flags]
        private enum WebXRInputSourceEventTypes
        {
            None = 0,
            SqueezeStart = 1,
            Squeeze = 2,
            SqueezeEnd = 4,
            SelectStart = 8,
            Select = 16,
            SelectEnd = 32
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("In session : ");
            sb.AppendLine(InSession ? "Yes" : "No");

            sb.Append("AR supported : ");
            sb.AppendLine(IsArSupported() ? "Yes" : "No");

            sb.Append("VR supported : ");
            sb.AppendLine(IsVrSupported() ? "Yes" : "No");

            sb.Append("User height : ");
            sb.AppendLine(UserHeight.ToString("0.0"));

            sb.AppendLine(StringifyEye(LeftEye, "left"));
            sb.AppendLine(StringifyEye(RightEye, "right"));

            sb.Append(LeftInput.ToString());
            sb.Append(RightInput.ToString());

            return sb.ToString();
        }

        private string StringifyEye(Camera camera, string name)
        {
            if (camera)
            {
                var sb = new StringBuilder();
                sb.Append(name);
                sb.AppendLine("eye : ");
                sb.Append("  Position :");
                sb.AppendLine(camera.transform.position.ToString());
                sb.Append("  Rotation :");
                sb.AppendLine(camera.transform.rotation.eulerAngles.ToString());
                return sb.ToString();
            }
            else return $"No {name} eye";
        }


#region Device orientation sensor
        // Orientation info
        // 
        private readonly float[] _orientationArray = new float[3];
        private readonly byte[] _orientationInfo = new byte[1];
        private bool _orientationDeviceStarted = false;

        // Returns device orientation
        // see : https://developer.mozilla.org/en-US/docs/Web/Guide/Events/Orientation_and_motion_data_explained#About_rotation
        public bool GetDeviceOrientation(out float alpha, out float beta, out float gamma)
        {
            if (!_orientationDeviceStarted)
            {
                InternalGetDeviceOrientation(_orientationArray, _orientationInfo);
                _orientationDeviceStarted = true;
            }

            alpha = _orientationArray[0];
            beta = _orientationArray[1];
            gamma = _orientationArray[2];

            return _orientationInfo[0] != 0;
        }
#endregion
    }

    public enum WebXRHandedness { Left = 0, Right = 1 }

    [Serializable]
    public class WebXRInput
    {
        public UnityEvent Select = new UnityEvent();
        public UnityEvent SelectStart = new UnityEvent();
        public UnityEvent SelectEnd = new UnityEvent();
        public UnityEvent Squeeze = new UnityEvent();
        public UnityEvent SqueezeStart = new UnityEvent();
        public UnityEvent SqueezeEnd = new UnityEvent();

        public WebXRInput(WebXRHandedness handedness)
        {
            Handedness = handedness;
            for (int i = 0; i < AXES_BUTTON_COUNT; i++) Buttons[i] = new WebXRGamepadButton();
        }

        [HideInInspector()]
        public bool Available;

        public readonly WebXRHandedness Handedness;

        [HideInInspector()]
        public Vector3 Position;
        [HideInInspector()]
        public Quaternion Rotation;
        [HideInInspector()]
        public bool IsPositionTracked = false;

        public const int AXES_BUTTON_COUNT = 8;

        [HideInInspector()]
        public int AxesCount = 0;
        public readonly float[] Axes = new float[AXES_BUTTON_COUNT];

        [HideInInspector()]
        public int ButtonsCount = 0;
        public readonly WebXRGamepadButton[] Buttons = new WebXRGamepadButton[AXES_BUTTON_COUNT];

        [HideInInspector()]
        public WebXRTargetRayModes TargetRayMode = WebXRTargetRayModes.None;

        [HideInInspector()]
        public bool Selected;
        [HideInInspector()]
        public bool Squeezed;

        public readonly WebXRHand Hand = new WebXRHand();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Handedness);
            sb.AppendLine("controller");

            sb.Append("  Available : ");
            sb.AppendLine(Available ? "Yes" : "No");

            sb.Append("  Mode : ");
            sb.AppendLine(TargetRayMode.ToString());

            if (IsPositionTracked)
            {
                sb.Append("  Position : ");
                sb.AppendLine(Position.ToString());

                sb.Append("  Rotation : ");
                sb.AppendLine(Rotation.eulerAngles.ToString());
            }
            else sb.AppendLine("  No position info");

            sb.Append("  Hand : ");
            sb.AppendLine(Hand.Available ? "Yes" : "No");

            sb.AppendLine("  Axes :");
            if (AxesCount > 0)
            {
                for (int i = 0; i < Math.Min(AxesCount, Axes.Length); i++)
                {
                    sb.Append("    [");
                    sb.Append(i);
                    sb.Append("] : ");
                    sb.Append(100 * (int)Axes[i]);
                    sb.AppendLine("%");
                }
            }
            else sb.AppendLine("    None");

            sb.AppendLine("  Buttons :");
            if (ButtonsCount > 0)
            {
                for (int i = 0; i < Math.Min(ButtonsCount, Buttons.Length); i++)
                {
                    sb.Append("    [");
                    sb.Append(i);
                    sb.Append("] : ");
                    sb.AppendLine(Buttons[i].ToString());
                }
            }
            else sb.AppendLine("    None");

            return sb.ToString();
        }

    }

    public class WebXRGamepadButton
    {
        public float Value;
        public bool Touched;
        public bool Pressed;

        public override string ToString()
        {
            return $"{(Touched ? (Pressed ? "Touched and pressed" : "Touched") : (Pressed ? "Pressed" : "Released"))} - {(int)(100 * Value)}%";
        }
    }

    public enum WebXRTargetRayModes
    {
        None = 0,
        TrackedPointer = 1,
        Screen = 2,
        Gaze = 3,
    }

    [Serializable]
    public class WebXRInputEvent : UnityEvent<WebXRInput> { }

    public class WebXRJoint
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Radius = float.NaN;
    }

    /// <summary>
    /// 
    /// </summary>
    public class WebXRHand
    {
        public const int JOINT_COUNT = 25;

        public WebXRHand()
        {
            for (int i = 0; i < JOINT_COUNT; i++) Joints[i] = new WebXRJoint();
        }

        public bool Available;
        public readonly WebXRJoint[] Joints = new WebXRJoint[JOINT_COUNT];


        public const int WRIST = 0;

        public const int THUMB_METACARPAL = 1;
        public const int THUMB_PHALANX_PROXIMAL = 2;
        public const int THUMB_PHALANX_DISTAL = 3;
        public const int THUMB_PHALANX_TIP = 4;

        public const int INDEX_METACARPAL = 5;
        public const int INDEX_PHALANX_PROXIMAL = 6;
        public const int INDEX_PHALANX_INTERMEDIATE = 7;
        public const int INDEX_PHALANX_DISTAL = 8;
        public const int INDEX_PHALANX_TIP = 9;

        public const int MIDDLE_METACARPAL = 10;
        public const int MIDDLE_PHALANX_PROXIMAL = 11;
        public const int MIDDLE_PHALANX_INTERMEDIATE = 12;
        public const int MIDDLE_PHALANX_DISTAL = 13;
        public const int MIDDLE_PHALANX_TIP = 14;

        public const int RING_METACARPAL = 15;
        public const int RING_PHALANX_PROXIMAL = 16;
        public const int RING_PHALANX_INTERMEDIATE = 17;
        public const int RING_PHALANX_DISTAL = 18;
        public const int RING_PHALANX_TIP = 19;

        public const int LITTLE_METACARPAL = 20;
        public const int LITTLE_PHALANX_PROXIMAL = 21;
        public const int LITTLE_PHALANX_INTERMEDIATE = 22;
        public const int LITTLE_PHALANX_DISTAL = 23;
        public const int LITTLE_PHALANX_TIP = 24;
    }
}