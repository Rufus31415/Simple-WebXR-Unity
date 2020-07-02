using System;
using System.Runtime.InteropServices; // for DllImport
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Exposes a simple behavior that allows any WebGL application built with Unity to implement WebXR.
/// This behavior will display an "Enter AR" GUI button that will launch the WebXR session (see also SimpleWebXR.jslib and SimpleWebXR.jspre). 
/// The data (projection matrix, position and rotation) are shared via a buffer.
/// When the WebXR session is started, this behavior modifies the position and characteristics of the main camera to allow the augmented reality experience.
/// </summary>
public class SimpleWebXR : MonoBehaviour
{
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
    // [61] -> [67] : right input x, y, z, rx, ry, rz, rw
    private readonly float[] _dataArray = new float[68];

    // Shared float array with javascript.
    // [0] : number of views (0 : session is stopped)
    // [1] : left controller events
    // [2] : left controller events
    // [3] : input change event
    // [4] : left input has position info
    // [5] : left input gamepad axes count
    // [6] : left input gamepad buttons count
    // [7] : right input has position info
    // [8] : right input gamepad axes count
    // [9] : right input gamepad buttons count
    private readonly byte[] _byteArray = new byte[10];

    // Number of views (i.e. cameras)
    private WebXRViewEyes ViewEye => (WebXRViewEyes)_byteArray[0];

    // A session is running
    public bool InSession => _byteArray[0] != 0;

    public readonly WebXRInput LeftInput = new WebXRInput(WebXRHandedness.Left);
    public readonly WebXRInput RightInput = new WebXRInput(WebXRHandedness.Right);

    public Camera LeftEye => _cameras[0];
    public Camera RightEye => _cameras[1];

    // Cameras created for each eyes ([0]:left, [1]:right)
    private readonly Camera[] _cameras = new Camera[2];

    public WebXRInput GetInput(WebXRHandedness handedness)
    {
        return handedness == WebXRHandedness.Left ? LeftInput : RightInput;
    }


#if UNITY_WEBGL && !UNITY_EDITOR // If executed in browser
    [DllImport("__Internal")]
    private static extern void InternalStartSession();

    [DllImport("__Internal")]
    private static extern void InternalEndSession();

    [DllImport("__Internal")]
    private static extern void InitWebXR(float[] dataArray, int length, byte[] _byteArray, int _byteArrayLength);

    [DllImport("__Internal")]
    private static extern bool IsArSupported();


    [DllImport("__Internal")]
    private static extern bool IsVrSupported();

#else // if executed with Unity editor
    private static void InternalStartSession() { }

    private static void InternalEndSession() { }


    private static void InitWebXR(float[] dataArray, int length, byte[] _byteArray, int _byteArrayLength) { }

    private static bool IsArSupported()
    {
        return true; // always display "Enter AR" button for debug purpose
    }

    private static bool IsVrSupported()
    {
        return true; // always display "Enter VR" button for debug purpose
    }
#endif

    // Share _dataArray and init WebXR
    void Start()
    {
        InitWebXR(_dataArray, _dataArray.Length, _byteArray, _byteArray.Length);
    }

    // Display "Enter AR" button if WebXR immersive AR is supported
    private void OnGUI()
    {
        if (ViewEye == WebXRViewEyes.None && (IsArSupported() || IsVrSupported()))
        {
            var width = 120;
            var height = 60;
            if (GUI.Button(new Rect((Screen.width - width) / 2, Screen.height - height, width, height), "Enter " + (IsArSupported() ? "AR" : "VR")))
            {
                StartSession();
            }
        }

        var style = GUI.skin.label;
        style.fontSize = 40;

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "\r\n C:" + Camera.main.transform.position.ToString() + "\r\n L:" + LeftInput.Position.ToString() + "\r\n R:" + RightInput.Position.ToString(), style);
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
        if (!InSession) return;

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
        var floatStartId = (int)inputSource.Handedness * 7 + 54;
        var byteStartId = (int)inputSource.Handedness * 3 + 4;

        inputSource.Position = ToUnityPosition(_dataArray[floatStartId + 0], _dataArray[floatStartId + 1], _dataArray[floatStartId + 2]);
        inputSource.Rotation = ToUnityRotation(_dataArray[floatStartId + 3], _dataArray[floatStartId + 4], _dataArray[floatStartId + 5], _dataArray[floatStartId + 6]);
        inputSource.IsPositionValid = (_byteArray[byteStartId + 0] != 0);

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
    }

    private void RaiseInputSourceEvent(byte mask, WebXRInputSourceEventTypes type, WebXRInputEvent webxrInputEvent, UnityEvent unityEvent, WebXRInput inputSource)
    {
        if (((WebXRInputSourceEventTypes)mask & type) == type)
        {
            unityEvent.Invoke();
            webxrInputEvent.Invoke(inputSource);
        }
    }

    private string infoStrLeft = "L";
    private string infoStrRight = "R";

    private static Vector3 ToUnityPosition(float x, float y, float z)
    {
        return new Vector3(x, y, -z);
    }
    private static Quaternion ToUnityRotation(float x, float y, float z, float w)
    {
        var quat = new Quaternion(x, y, z, w);
        var euler = quat.eulerAngles;
        return Quaternion.Euler(-euler.x, -euler.y, euler.z);
    }

    public readonly UnityEvent InputSourcesChange = new UnityEvent();

    public readonly WebXRInputEvent InputSourceSelect = new WebXRInputEvent();
    public readonly WebXRInputEvent InputSourceSelectStart = new WebXRInputEvent();
    public readonly WebXRInputEvent InputSourceSelectEnd = new WebXRInputEvent();
    public readonly WebXRInputEvent InputSourceSqueeze = new WebXRInputEvent();
    public readonly WebXRInputEvent InputSourceSqueezeStart = new WebXRInputEvent();
    public readonly WebXRInputEvent InputSourceSqueezeEnd = new WebXRInputEvent();

    // Update camera position, rotation and projection
    void LateUpdate()
    {
        UpdateCamera(WebXRViewEyes.Left);
        UpdateCamera(WebXRViewEyes.Right);

        UpdateInput(LeftInput);
        UpdateInput(RightInput);

        if (_byteArray[3] != 0)
        {
            InputSourcesChange.Invoke();
            _byteArray[3] = 0;
        }
    }


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
}

public enum WebXRHandedness { Left, Right }

public class WebXRInput
{
    public WebXRInput(WebXRHandedness handedness) { Handedness = handedness; }

    public readonly WebXRHandedness Handedness;
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsPositionValid;
    public float[] Axes;

    public readonly UnityEvent Select = new UnityEvent();
    public readonly UnityEvent SelectStart = new UnityEvent();
    public readonly UnityEvent SelectEnd = new UnityEvent();
    public readonly UnityEvent Squeeze = new UnityEvent();
    public readonly UnityEvent SqueezeStart = new UnityEvent();
    public readonly UnityEvent SqueezeEnd = new UnityEvent();

}

public class WebXRInputEvent : UnityEvent<WebXRInput> { }

