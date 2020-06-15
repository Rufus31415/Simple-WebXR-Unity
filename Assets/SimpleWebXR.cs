using System.Runtime.InteropServices; // for DllImport
using UnityEngine;

/// <summary>
/// Exposes a simple behavior that allows any WebGL application built with Unity to implement WebXR.
/// This behavior will display an "Enter AR" GUI button that will launch the WebXR session (see also SimpleWebXR.jslib and SimpleWebXR.jspre). 
/// The data (projection matrix, position and rotation) are shared via a buffer.
/// When the WebXR session is started, this behavior modifies the position and characteristics of the main camera to allow the augmented reality experience.
/// </summary>
public class SimpleWebXR : MonoBehaviour
{
    // Shared array with javascript.
    // [1] : Number of views
    // [0] -> [15] : projection matrix of view 1
    // [16], [17], [18] : X, Y, Z position in m  of view 1
    // [19], [20], [21], [22] : RX, RY, RZ, RW rotation (quaternion)  of view 1
    // [23] -> [26] : Viewport X, Y, width, height  of view 1
    // [27] -> [42] : projection matrix of view 2
    // [43], [44], [45] : X, Y, Z position in m  of view 2
    // [46], [47], [48], [49] : RX, RY, RZ, RW rotation (quaternion)  of view 2
    // [50] -> [53] : Viewport X, Y, width, height  of view 2
    private readonly float[] _dataArray = new float[55];

    // Number of views (i.e. cameras)
    public int CameraCount => (int)_dataArray[0];

    // A session is running
    public bool InSession => _dataArray[0] != 0;


    // Cameras created for each eyes ([0]:left, [1]:right)
    private readonly Camera[] _cameras = new Camera[2];



#if UNITY_WEBGL && !UNITY_EDITOR // If executed in browser
    [DllImport("__Internal")]
    private static extern void InternalStartSession();

    [DllImport("__Internal")]
    private static extern void InternalEndSession();

    [DllImport("__Internal")]
    private static extern void InitWebXR(float[] dataArray, int length);

    [DllImport("__Internal")]
    private static extern bool IsArSupported();


    [DllImport("__Internal")]
    private static extern bool IsVrSupported();

#else // if executed with Unity editor
    private static void InternalStartSession() { }

    private static void InternalEndSession() { }


    private static void InitWebXR(float[] dataArray, int length) { }

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
        InitWebXR(_dataArray, _dataArray.Length);
    }

    // Display "Enter AR" button if WebXR immersive AR is supported
    private void OnGUI()
    {
        if (CameraCount == 0 && (IsArSupported() || IsVrSupported()))
        {
            var width = 120;
            var height = 60;
            if (GUI.Button(new Rect((Screen.width - width) / 2, Screen.height - height, width, height), "Enter " + (IsArSupported() ? "AR" : "VR")))
            {
                StartSession();
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
        if (!InSession) return;

        InternalEndSession();
    }


    private void UpdateCamera(int id)
    {
        // If the camera for this id should not exist
        if (CameraCount <= id)
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
            var camGameObject = new GameObject("WebXRCamera_" + id);
            camGameObject.transform.parent = gameObject.transform;
            _cameras[id] = camGameObject.AddComponent<Camera>();
            _cameras[id].clearFlags = CameraClearFlags.SolidColor;
            _cameras[id].backgroundColor = new Color(0, 0, 0, 0);
        }

        var startIndex = id * 27 + 1;

        // Get and transpose projection matrix
        var pm = new Matrix4x4();
        pm.m00 = _dataArray[startIndex+0];
        pm.m01 = _dataArray[startIndex + 4];
        pm.m02 = _dataArray[startIndex + 8];
        pm.m03 = _dataArray[startIndex + 12];
        pm.m10 = _dataArray[startIndex + 1];
        pm.m11 = _dataArray[startIndex + 5];
        pm.m12 = _dataArray[startIndex + 9];
        pm.m13 = _dataArray[startIndex + 13];
        pm.m20 = _dataArray[startIndex + 2];
        pm.m21 = _dataArray[startIndex + 6];
        pm.m22 = _dataArray[startIndex + 10];
        pm.m23 = _dataArray[startIndex + 14];
        pm.m30 = _dataArray[startIndex + 3];
        pm.m31 = _dataArray[startIndex + 7];
        pm.m32 = _dataArray[startIndex + 11];
        pm.m33 = _dataArray[startIndex + 15];

        _cameras[id].projectionMatrix = pm;

        // Get position and rotation Z, RX and RY are inverted
        _cameras[id].transform.position = new Vector3(_dataArray[startIndex + 16], _dataArray[startIndex + 17], -_dataArray[startIndex + 18]);

        var quat = new Quaternion(_dataArray[startIndex + 19], _dataArray[startIndex + 20], _dataArray[startIndex + 21], _dataArray[startIndex + 22]);
        var euler = quat.eulerAngles;
        _cameras[id].transform.rotation = Quaternion.Euler(-euler.x, -euler.y, euler.z);

        _cameras[id].rect = new Rect(_dataArray[startIndex + 23], _dataArray[startIndex + 24], _dataArray[startIndex + 25], _dataArray[startIndex + 26]);
    }

    // Update camera position, rotation and projection
    void LateUpdate()
    {
        UpdateCamera(0);
        UpdateCamera(1);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("WebXR/Set camera count")]
    public static void SetCamera()
    {
        Object.FindObjectOfType<SimpleWebXR>()._dataArray[0] = 2;
    }
#endif
}
