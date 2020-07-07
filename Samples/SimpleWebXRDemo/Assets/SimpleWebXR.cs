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
    // [61] -> [68] : left input axes
    // [69] -> [76] : left gamepad value
    // [77] -> [83] : right input x, y, z, rx, ry, rz, rw
    // [84] -> [91] : right input axes
    // [92] -> [99] : right gamepad value
    private readonly float[] _dataArray = new float[100];

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
    private readonly byte[] _byteArray = new byte[46];

    // Number of views (i.e. cameras)
    private WebXRViewEyes ViewEye => (WebXRViewEyes)_byteArray[0];

    // A session is running
    private bool InternalInSession => _byteArray[0] != 0;

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

    public bool InSession { get; private set; }

    public bool HideStartButton;

    public static SimpleWebXR GetInstance()
    {
        return FindObjectOfType<SimpleWebXR>();
    }


#if UNITY_WEBGL && !UNITY_EDITOR // If executed in browser
    [DllImport("__Internal")]
    private static extern void InternalStartSession();

    [DllImport("__Internal")]
    private static extern void InternalEndSession();

    [DllImport("__Internal")]
    private static extern void InitWebXR(float[] dataArray, int length, byte[] _byteArray, int _byteArrayLength);

    [DllImport("__Internal")]
    public static extern bool IsArSupported();

    [DllImport("__Internal")]
    public static extern bool IsVrSupported();

    [DllImport("__Internal")]
    public static extern void InternalGetDeviceOrientation(float[] orientationArray, byte[] orientationInfo);

#else // if executed with Unity editor
    private static void InternalStartSession() { }

    private static void InternalEndSession() { }


    private static void InitWebXR(float[] dataArray, int length, byte[] _byteArray, int _byteArrayLength) { }

    public static bool IsArSupported()
    {
        return true; // always display "Enter AR" button for debug purpose
    }

    public static bool IsVrSupported()
    {
        return true; // always display "Enter VR" button for debug purpose
    }

    public static  void InternalGetDeviceOrientation(float[] orientationArray, byte[] orientationInfo)
    {

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
        if (HideStartButton) return;

        if (!InternalInSession && (IsArSupported() || IsVrSupported()))
        {
            var width = 120;
            var height = 60;
            if (GUI.Button(new Rect((Screen.width - width) / 2, Screen.height - height, width, height), "Enter " + (IsArSupported() ? "AR" : "VR")))
            {
                StartSession();
            }
        }

        /*
        var style = GUI.skin.label;
        style.fontSize = 40;

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), 
            $"C:{Camera.main.transform.position}\r\n" +
            $"L:{LeftInput.Position} - {LeftInput.Rotation.eulerAngles}\r\n" +
            $"R:{RightInput.Position} - {RightInput.Rotation.eulerAngles}\r\n" +
            $"L:{LeftInput.AxesCount} - {LeftInput.ButtonsCount} : {LeftInput.Buttons[0].Pressed} {LeftInput.Buttons[0].Pressed} {LeftInput.Buttons[0].Value} /  {LeftInput.Buttons[1].Pressed} {LeftInput.Buttons[1].Pressed} {LeftInput.Buttons[1].Value}\r\n" +
            $"R:{RightInput.AxesCount} - {RightInput.ButtonsCount} : {RightInput.Buttons[0].Pressed} {RightInput.Buttons[0].Pressed} {RightInput.Buttons[0].Value} / {RightInput.Buttons[1].Pressed} {RightInput.Buttons[1].Pressed} {RightInput.Buttons[1].Value}\r\n"
            , style);*/
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

        inputSource.TargetRayMode = (WebXRTargetRayModes)_byteArray[byteStartId + 1];

        inputSource.AxesCount = _byteArray[byteStartId + 2];
        for(int i=0;i< WebXRInput.AXES_BUTTON_COUNT;i++)
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
    }

    private void RaiseInputSourceEvent(byte mask, WebXRInputSourceEventTypes type, WebXRInputEvent webxrInputEvent, UnityEvent unityEvent, WebXRInput inputSource)
    {
        if (((WebXRInputSourceEventTypes)mask & type) == type)
        {
            unityEvent.Invoke();
            webxrInputEvent.Invoke(inputSource);

            Debug.Log($"{inputSource.Handedness} : {type}");
        }
    }


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
            sb.Append("  FOV :");
            sb.AppendLine(camera.fieldOfView.ToString("0.0"));
            
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
        if (!_orientationDeviceStarted) InternalGetDeviceOrientation(_orientationArray, _orientationInfo);

        alpha = _orientationArray[0];
        beta = _orientationArray[1];
        gamma = _orientationArray[2];

        return _orientationInfo[0] != 0;
    }

    #endregion

}

public enum WebXRHandedness { Left = 0, Right = 1 }

public class WebXRInput
{
    public const int AXES_BUTTON_COUNT = 8;

    public WebXRInput(WebXRHandedness handedness)
    {
        Handedness = handedness;
        for (int i = 0; i < AXES_BUTTON_COUNT; i++) Buttons[i] = new WebXRGamepadButton();
    }

    public bool Available;

    public readonly WebXRHandedness Handedness;
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsPositionValid = false;

    public int AxesCount = 0;
    public readonly float[] Axes = new float[AXES_BUTTON_COUNT];

    public int ButtonsCount = 0;
    public readonly WebXRGamepadButton[] Buttons = new WebXRGamepadButton[AXES_BUTTON_COUNT];

    public WebXRTargetRayModes TargetRayMode = WebXRTargetRayModes.None;

    public readonly UnityEvent Select = new UnityEvent();
    public readonly UnityEvent SelectStart = new UnityEvent();
    public readonly UnityEvent SelectEnd = new UnityEvent();
    public readonly UnityEvent Squeeze = new UnityEvent();
    public readonly UnityEvent SqueezeStart = new UnityEvent();
    public readonly UnityEvent SqueezeEnd = new UnityEvent();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Handedness);
        sb.AppendLine("controller");

        sb.Append("  Available : ");
        sb.AppendLine(Available ? "Yes" : "No");

        sb.Append("  Mode : ");
        sb.AppendLine(TargetRayMode.ToString());

        if (IsPositionValid)
        {
            sb.Append("  Position : ");
            sb.AppendLine(Position.ToString());

            sb.Append("  Rotation : ");
            sb.AppendLine(Rotation.eulerAngles.ToString());
        }
        else sb.AppendLine("  No position info");

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

public class WebXRInputEvent : UnityEvent<WebXRInput> { }

