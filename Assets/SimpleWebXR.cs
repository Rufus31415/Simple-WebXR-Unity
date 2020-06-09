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
    // [0] -> [15] : projection matrix
    // [16], [17], [18] : X, Y, Z position in m
    // [19], [20], [21], [22] : RX, RY, RZ, RW rotation (quaternion)
    // [23] : if 0.0f, no data available in this array
    private readonly float[] _dataArray = new float[24];


#if UNITY_WEBGL && !UNITY_EDITOR // If executed in browser
    [DllImport("__Internal")]
    private static extern void StartSession();

    [DllImport("__Internal")]
    private static extern void InitWebXR(float[] dataArray);

    [DllImport("__Internal")]
    private static extern bool IsArSupported();

#else // if executed with Unity editor
    private static void StartSession() { }

    private static void InitWebXR(float[] dataArray) { }

    private static bool IsArSupported()
    {
        return true; // always display "Enter AR" button for debug purpose
    }
#endif

    // Share _dataArray and init WebXR
    void Start()
    {
        InitWebXR(_dataArray);
    }

    // Display "Enter AR" button if WebXR immersive AR is supported
    private void OnGUI()
    {
        if (_dataArray[23] == 0f && IsArSupported())
        {
            var width = 120;
            var height = 60;
            if (GUI.Button(new Rect((Screen.width - width) / 2, Screen.height - height, width, height), "Enter AR"))
            {
                StartSession();
            }
        }
    }

    // A wrapper for external calls
    public void StartSessionWrapper()
    {
        StartSession();
    }

    // Update camera position, rotation an projection
    void LateUpdate()
    {
        if (_dataArray[23] == 0f) return; // wait for data available

        // Get and transpose projection matrix
        var pm = new Matrix4x4();
        pm.m00 = _dataArray[0];
        pm.m01 = _dataArray[4];
        pm.m02 = _dataArray[8];
        pm.m03 = _dataArray[12];
        pm.m10 = _dataArray[1];
        pm.m11 = _dataArray[5];
        pm.m12 = _dataArray[9];
        pm.m13 = _dataArray[13];
        pm.m20 = _dataArray[2];
        pm.m21 = _dataArray[6];
        pm.m22 = _dataArray[10];
        pm.m23 = _dataArray[14];
        pm.m30 = _dataArray[3];
        pm.m31 = _dataArray[7];
        pm.m32 = _dataArray[11];
        pm.m33 = _dataArray[15];

        Camera.main.projectionMatrix = pm;

        // Get position and rotation Z, RX and RY are inverted
        Camera.main.transform.position = new Vector3(_dataArray[16], _dataArray[17], -_dataArray[18]);

        var quat = new Quaternion(_dataArray[19], _dataArray[20], _dataArray[21], _dataArray[22]);
        var euler = quat.eulerAngles;
        Camera.main.transform.rotation = Quaternion.Euler(-euler.x, -euler.y, euler.z);
    }
}
