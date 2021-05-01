// SimpleWebXR - Unity
//                                                                                           .*#%@@@@@@&.                 
//           ..                           ,                                        .,(%&@@@@@@@@@@@@@@@@#                 
//           //////,                ./(((((/                                    %@@@@@@@@@@@@@@@@@@@@@@@@*                
//           *//////////,      .*((((((((((,                                  *@@@@@@@@&#/,.  #@@@@@@@@@@&.               
//           /////////////*,*/(((((((((((/                          .#@@@@@@@#/,.          ,&@@@@@*(@@@@@#               
//            ////////*,,,,,,,,,,,*/((((((,                        /@@@@@@@&.              (@@@@@%   %@@@@@*              
//            //*,,,,,,,,,,,,,,,,,,,,,,,//                      .%@@@@@@@(               ,&@@@@@*    ,@@@@@&.             
//          ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,                 (@@@@@@@%.                (@@@@@%.      /@@@@@%             
//       .,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,            ,&@@@@@@@/                 .&@@@@@/         #@@@@&.            
//       ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.         #@@@@@@@@#((((((((((((((((((%@@@@@%.          .&@@(              
//        ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,       ,&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@/             *&.               
//         ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,          ,%@@@@@@@@&%%%%%%%%%%%%%%%%%%&@@@@@#            &@@*              
//          ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,              /@@@@@@@&,                 *@@@@@@,         (@@@@%.            
//           ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,.                 .%@@@@@@@#                 %@@@@@#       *@@@@@%             
//            ,,,,,,,,,,,,,,,,,,,,,,,,,,,,.                     *&@@@@@@&,               *@@@@@@,    .&@@@@@,             
//             ,,,,,,,,,,,,,,,,,,,,,,,,,,.                         #@@@@@@@#               %@@@@@#   %@@@@@/              
//              ,,,,,,,,,,.  .,,,,,,,,,,.                            ,&@@@@@@&/,.           *@@@@@@,/@@@@@%               
//               ,.,,,,,        ,,,,,,,.                                      /@@@@@@@%#/,.   %@@@@@@@@@@@.               
//                ....            .,,,.                                        .&@@@@@@@@@@@@@@@@@@@@@@@@*                
//                                                                                .*(%@@@@@@@@@@@@@@@@@@#                 
//                                                                                          ,/#&@@@@@@@&.                 
//                                                                                                   .*,                  
//
//                ███████╗██╗███╗   ███╗██████╗ ██╗     ███████╗██╗    ██╗███████╗██████╗ ██╗  ██╗██████╗ 
//                ██╔════╝██║████╗ ████║██╔══██╗██║     ██╔════╝██║    ██║██╔════╝██╔══██╗╚██╗██╔╝██╔══██╗
//                ███████╗██║██╔████╔██║██████╔╝██║     █████╗  ██║ █╗ ██║█████╗  ██████╔╝ ╚███╔╝ ██████╔╝
//                ╚════██║██║██║╚██╔╝██║██╔═══╝ ██║     ██╔══╝  ██║███╗██║██╔══╝  ██╔══██╗ ██╔██╗ ██╔══██╗
//                ███████║██║██║ ╚═╝ ██║██║     ███████╗███████╗╚███╔███╔╝███████╗██████╔╝██╔╝ ██╗██║  ██║
//                ╚══════╝╚═╝╚═╝     ╚═╝╚═╝     ╚══════╝╚══════╝ ╚══╝╚══╝ ╚══════╝╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
//
// 
// -----------------------------------------------------------------------------
//
// https://github.com/Rufus31415/Simple-WebXR-Unity
//
// -----------------------------------------------------------------------------
//
// MIT License
//
// Copyright(c) 2020 Florent GIRAUD (Rufus31415)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----------------------------------------------------------------------------

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Rufus31415.WebXR
{
    /// <summary>
    /// This class can be used as a Behavior to make your application instantly compatible with WebXR.
    /// It will display a "Enter AR" or "Enter VR" GUI button that will launch the WebXR session and the main camera pose is automatically updated.
    /// However, for more advanced applications, this class contains static functions to expose the WebXR javascript API in the Unity C# code.
    /// </summary>
    /// <remarks>
    /// The data (projection matrix, position and rotation) are shared with javascript via arrays.
    /// See also files SimpleWebXR.jslib and SimpleWebXR.jspre
    /// </remarks>
    public class SimpleWebXR : MonoBehaviour
    {
        #region MonoBehaviour

        /// <summary>
        /// Hide the GUI button "Start AR" or "Start VR".
        /// </summary>
        public bool HideStartButton;

        /// <summary>
        /// XR Space in which positions are returned
        /// </summary>
        public WebXRReferenceSpaces ReferenceSpace = WebXRReferenceSpaces.Viewer;

        /// <summary>
        /// Y axis offset to apply when local floor XR space is not supported
        /// </summary>
        public float FallbackUserHeight = 1.8f;

        /// Update cameras poses and trigger events
        private void LateUpdate()
        {
            _referenceSpace = ReferenceSpace;
            _fallbackUserHeight = FallbackUserHeight;
            UpdateWebXR();
        }

        /// <summary>
        /// A human-readable presentation of the WebXR session and capabilities
        /// </summary>
        public override string ToString() => Stringify();

#if UNITY_WEBGL
        // Display "Enter AR" or "Enter VR" button if WebXR immersive AR is supported
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
#endif

        #endregion

        #region Static WebXR library

        /// <summary>
        /// Indicates if a WebXR session is running
        /// </summary>
        public static bool InSession { get; private set; }

        /// <summary>
        /// User height in meter (startup distance from floor to device)
        /// </summary>
        public static float UserHeight => _dataArray[100];

        /// <summary>
        /// Event triggered when a session has started
        /// </summary>
        public static readonly UnityEvent SessionStart = new UnityEvent();

        /// <summary>
        /// Event triggered when a session has ended
        /// </summary>
        public static readonly UnityEvent SessionEnd = new UnityEvent();

        /// <summary>
        /// Left input controller data
        /// </summary>
        public static readonly WebXRInputSource LeftInput = new WebXRInputSource(WebXRHandedness.Left);

        /// <summary>
        /// Right input controller data
        /// </summary>
        public static readonly WebXRInputSource RightInput = new WebXRInputSource(WebXRHandedness.Right);

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source has fully completed its primary action.
        /// </summary>
        /// <remarks>
        /// On Oculus Quest : Back trigger button was pressed
        /// On Hololens 2 : A air tap has been was performed
        /// On smartphones : The screen was touched
        /// </remarks>
        public static readonly WebXRInputEvent InputSourceSelect = new WebXRInputEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectstart event, which means the input source begins its primary action.
        /// </summary>
        public static readonly WebXRInputEvent InputSourceSelectStart = new WebXRInputEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source ends its primary action.
        /// </summary>
        public static readonly WebXRInputEvent InputSourceSelectEnd = new WebXRInputEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source has fully completed its primary squeeze action.
        /// </summary>
        /// <remarks>
        /// On Oculus Quest : Side grip button was pressed
        /// </remarks>
        public static readonly WebXRInputEvent InputSourceSqueeze = new WebXRInputEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectstart event, which means the input source begins its primary squeeze action.
        /// </summary>
        public static readonly WebXRInputEvent InputSourceSqueezeStart = new WebXRInputEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source ends its primary squeeze action.
        /// </summary>
        public static readonly WebXRInputEvent InputSourceSqueezeEnd = new WebXRInputEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.inputsourceschange event, which means a input sources has been added or removed.
        /// </summary> 
        public static readonly UnityEvent InputSourcesChange = new UnityEvent();


        /// <summary>
        /// Camera that renders Left eye (Camera.main)
        /// </summary>
        public static Camera LeftEye => _cameras[0];

        /// <summary>
        /// Camera that renders Right eye
        /// </summary>
        public static Camera RightEye => _cameras[1];

        /// <summary>
        /// Indicates that a hit test is available in HitTestPosition and HitTestRotation
        /// </summary>
        public static bool HitTestInProgress => _byteArray[48] != 0;

        /// <summary>
        /// Indicates hit test is supported
        /// The immersive session should be started to estimate this capability
        /// </summary>
        public static bool HitTestSupported => _byteArray[49] != 0;

        /// <summary>
        /// Position of the hit test between head ray and the real world
        /// </summary>
        public static Vector3 HitTestPosition;

        /// <summary>
        /// Orientation of the hit test normale between head ray and the real world
        /// </summary>
        public static Quaternion HitTestRotation;

        /// <summary>
        /// Initialize the binding with the WebXR API, via shared arrays
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            InitWebXR(_dataArray, _dataArray.Length, _byteArray, _byteArray.Length, _handData, _handData.Length);
            _initialized = true;
        }

        /// <summary>
        /// Returns SimpleWebXR.LeftInput or SimpleWebXR.RightInput according to the argument
        /// </summary>
        /// <param name="handedness">The handedness of the desired input source</param>
        /// <returns>Input source controller</returns>
        public static WebXRInputSource GetInput(WebXRHandedness handedness)
        {
            return handedness == WebXRHandedness.Left ? LeftInput : RightInput;
        }

        /// <summary>
        /// Returns the instance of the gameobject SimpleWebXR in current scene.
        /// </summary>
        /// <remarks>Return null is no gameobject was found</remarks>
        public static SimpleWebXR GetInstance()
        {
            return FindObjectOfType<SimpleWebXR>();
        }

        /// <summary>
        /// Ensures that a SimpleWebXR instance is available in the current scene.
        /// If no instance was found, a "WebXR" gameobject is created as a root gameobject with a SimpleWebXR component.
        /// </summary>
        /// <returns>The existing or newly created SimpleWebXR instance</returns>
        public static SimpleWebXR EnsureInstance()
        {
            var xr = GetInstance();

            if (xr) return xr;

            var newGameObject = new GameObject("WebXR");
            return newGameObject.AddComponent<SimpleWebXR>();
        }

        /// <summary>
        /// Check if Augmented Reality (AR) is supported 
        /// </summary>
        /// <remarks>It returns the result of navigator.xr.isSessionSupported('immersive-ar')</remarks>
        /// <returns>True if AR is supported</returns>
        public static bool IsArSupported()
        {
            Initialize();
            return InternalIsArSupported();
        }

        /// <summary>
        /// Check if Virtual Reality (VR) is supported 
        /// </summary>
        /// <remarks>It returns the result of navigator.xr.isSessionSupported('immersive-vr')</remarks>
        /// <returns>True if VR is supported</returns>
        public static bool IsVrSupported()
        {
            Initialize();
            return InternalIsVrSupported();
        }

        /// <summary>
        /// Triggers the start of a WebXR immersive session 
        /// </summary>
        public static void StartSession()
        {
            Initialize();
            if (InternalInSession) return;
            InternalStartSession();
        }

        /// <summary>
        /// Ends the current WebXR immersive session
        /// </summary>
        public static void EndSession()
        {
            if (!InternalInSession) return;
            InternalHitTestCancel();
            InternalEndSession();
        }

        /// <summary>
        /// Starts hit test
        /// </summary>
        public static void HitTestStart()
        {
            Initialize();
            InternalHitTestStart();
        }

        /// <summary>
        /// Ends hit test
        /// </summary>
        public static void HitTestCancel()
        {
            Initialize();
            InternalHitTestCancel();
        }

        /// <summary>
        /// Update cameras (eyes), input sources and propagates WebXR events to Unity. This function should be called once per frame.
        /// </summary>
        public static void UpdateWebXR()
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

            UpdateHitTest();

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

        /// <summary>
        /// Applies haptic pulse feedback to a controller 
        /// </summary>
        /// <param name="hand">Controller to apply feedback</param>
        /// <param name="intensity">Feedback strength between 0 and 1</param>
        /// <param name="duration">Feedback duration in milliseconds</param>
        public static void HapticPulse(WebXRHandedness hand, float intensity, float duration)
        {
            _dataArray[101 + (int)hand] = intensity;
            _dataArray[103 + (int)hand] = duration;
        }

        /// <summary>
        /// A human-readable presentation of the WebXR session and capabilities
        /// </summary>
        public static string Stringify()
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

            sb.AppendLine(Stringify(LeftEye, "left"));
            sb.AppendLine(Stringify(RightEye, "right"));

            sb.Append(LeftInput.ToString());
            sb.Append(RightInput.ToString());

            return sb.ToString();
        }

        // Indicate that InitWebXR() has been called
        private static bool _initialized = false;

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
        // [101] : left input haptic pulse value
        // [102] : right input haptic pulse value
        // [103] : left input haptic pulse duration
        // [104] : right input haptic pulse duration
        // [105] -> [111] : hit test x, y, z, rx, ry, rz, rw
        private static readonly float[] _dataArray = new float[112];

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
        // [48] : Hit test in progress
        // [49] : Hit test supported
        private static readonly byte[] _byteArray = new byte[50];

        // Hand data
        // [0] -> [7] : Left Wrist x, y, z, rx, ry, rz, zw, radius
        // [8] -> [199] : Left other fingers ...
        // [200] -> [399] : right wrist and fingers
        private static readonly float[] _handData = new float[8 * 25 * 2];

        // static cache value of ReferenceSpace
        private static WebXRReferenceSpaces _referenceSpace;

        // static cache value of FallbackUserHeight
        private static float _fallbackUserHeight;

        // Number of views (i.e. cameras)
        private static WebXRViewEyes ViewEye => (WebXRViewEyes)_byteArray[0];

        // A session is running
        private static bool InternalInSession => _byteArray[0] != 0;

        // Cameras created for each eyes ([0]:left, [1]:right)
        private static readonly Camera[] _cameras = new Camera[2];

        // Indicates that main camera properties should be restored after sessions ends
        private static bool _shouldRestoreMainCameraProperties;

        // Remember main camera background color to restore value after sessions ends
        private static Color _mainCameraBackgroundColor;

        // Remember main camera clear flags to restore value after sessions ends
        private static CameraClearFlags _mainCameraClearFlags;

        private static Matrix4x4 _mainCameraProjectionMatrix;

        private static Rect _mainCameraRect;

        // Create and update camera pose
        private static void UpdateCamera(WebXRViewEyes eye)
        {
            var id = (eye == WebXRViewEyes.Left) ? 0 : 1;

            // If the camera for this id should not exist
            if ((ViewEye & eye) != eye)
            {
                if (_cameras[id])
                {
                    if (eye == WebXRViewEyes.Left) // don't destroy main camera
                    {
                        if (_shouldRestoreMainCameraProperties)
                        {
                            Camera.main.backgroundColor = _mainCameraBackgroundColor;
                            Camera.main.clearFlags = _mainCameraClearFlags;
                            Camera.main.projectionMatrix = _mainCameraProjectionMatrix;
                            Camera.main.rect = _mainCameraRect;
                        }
                    }
                    else
                    {
                        Destroy(_cameras[id].gameObject);
                    }

                    _cameras[id] = null;
                }

                return;
            }

            // Create camera
            if (!_cameras[id])
            {
                if (id > 0)
                {
                    // clone main camera
                    _cameras[id] = Instantiate(Camera.main, Camera.main.gameObject.transform.parent);
                    _cameras[id].name = "WebXRCamera_" + id;
                    _cameras[id].depth = Camera.main.depth - 1;
                }
                else
                {
                    _shouldRestoreMainCameraProperties = false;

                    if (Camera.main)
                    {
                        _cameras[0] = Camera.main;
                        _shouldRestoreMainCameraProperties = true;
                        _mainCameraBackgroundColor = Camera.main.backgroundColor;
                        _mainCameraClearFlags = Camera.main.clearFlags;
                        _mainCameraProjectionMatrix = Camera.main.projectionMatrix;
                        _mainCameraRect = Camera.main.rect;
                    }
                    else
                    {
                        var go = new GameObject("WebXRCamera_0");
                        _cameras[0] = go.AddComponent<Camera>();
                    }
                }

                if (IsArSupported())
                {
                    _cameras[id].clearFlags = CameraClearFlags.SolidColor;
                    _cameras[id].backgroundColor = new Color(0, 0, 0, 0);
                }
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
            _cameras[id].transform.localPosition = ToUnityPosition(_dataArray[floatStartId + 16], _dataArray[floatStartId + 17], _dataArray[floatStartId + 18]);
            _cameras[id].transform.localRotation = ToUnityRotation(_dataArray[floatStartId + 19], _dataArray[floatStartId + 20], _dataArray[floatStartId + 21], _dataArray[floatStartId + 22]);
        }

        // Update input source pose
        private static void UpdateInput(WebXRInputSource inputSource)
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
            for (int i = 0; i < WebXRInputSource.AXES_BUTTON_COUNT; i++)
            {
                inputSource.Axes[i] = _dataArray[floatStartId + 7 + i];
            }

            inputSource.ButtonsCount = _byteArray[byteStartId + 3];
            for (int i = 0; i < WebXRInputSource.AXES_BUTTON_COUNT; i++)
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

        // Raise input sources select and squeeze events
        private static void RaiseInputSourceEvent(byte mask, WebXRInputSourceEventTypes type, WebXRInputEvent webxrInputEvent, UnityEvent unityEvent, WebXRInputSource inputSource)
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


        // Update the hit test infos
        private static void UpdateHitTest()
        {
            if (HitTestInProgress)
            {
                HitTestPosition = ToUnityPosition(_dataArray[105], _dataArray[106], _dataArray[107]);
                HitTestRotation = ToUnityRotation(_dataArray[108], _dataArray[109], _dataArray[110], _dataArray[111]);
            }
        }

        // Converts a WebGL position coordinate to a Unity position coordinate
        private static Vector3 ToUnityPosition(float x, float y, float z)
        {
            float yOffset = 0;

            if(_referenceSpace == WebXRReferenceSpaces.LocalFloor)
            {
                yOffset = UserHeight <= 0 ? _fallbackUserHeight : UserHeight;
            }

            return new Vector3(x, y + yOffset, -z);
        }

        // Converts a WebGL rotation to a Unity rotation
        private static Quaternion ToUnityRotation(float x, float y, float z, float w)
        {
            return new Quaternion(-x, -y, z, w);
        }


#if UNITY_WEBGL && !UNITY_EDITOR // If executed in browser
        [DllImport("__Internal")]
        private static extern void InternalStartSession();

        [DllImport("__Internal")]
        private static extern void InternalEndSession();

        [DllImport("__Internal")]
        private static extern void InitWebXR(float[] dataArray, int length, byte[] byteArray, int _byteArrayLength, float[] handDataArray, int handDataArrayLength);

        [DllImport("__Internal")]
        private static extern bool InternalIsArSupported();

        [DllImport("__Internal")]
        private static extern bool InternalIsVrSupported();

        [DllImport("__Internal")]
        private static extern void InternalHitTestStart();

        [DllImport("__Internal")]
        private static extern void InternalHitTestCancel();


        [DllImport("__Internal")]
        private static extern void InternalGetDeviceOrientation(float[] orientationArray, byte[] orientationInfo);

#else // if executed with Unity editor
        private static void InternalEndSession() { }

        private static void InternalStartSession() { }

        private static void InitWebXR(float[] dataArray, int length, byte[] _byteArray, int _byteArrayLength, float[] handDataArray, int handDataArrayLength) { }

        private static bool InternalIsArSupported() => false;

        private static bool InternalIsVrSupported() => false;

        private static void InternalGetDeviceOrientation(float[] orientationArray, byte[] orientationInfo) { }

        private static void InternalHitTestStart() { }

        private static void InternalHitTestCancel() { }
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

        private static string Stringify(Camera camera, string name)
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
        #endregion


        #region Device orientation sensor

        // Orientation float data (shared array with javascript)
        // [0] : alpha
        // [1] : beta
        // [2] : gamma
        private static readonly float[] _orientationArray = new float[3];

        // Orientation byte data (shared array with javascript)
        // [0] : 1=valid angle values, 0=angles not available yet
        private static readonly byte[] _orientationInfo = new byte[1];

        // Indicates that InternalGetDeviceOrientation was already called
        private static bool _orientationDeviceStarted = false;

        /// <summary>
        /// Get the orientation of the device. This feature is independent of WebXR and can be used as a fallback if WebXR is not supported. 
        /// </summary>
        /// <remarks>
        /// Values come from the javascript event "deviceorientation" obtained by : window.addEventListener("deviceorientation", _onDeviceOrientation);
        /// The x axis is in the plane of the screen and is positive toward the right and negative toward the left.
        /// The y axis is in the plane of the screen and is positive toward the top and negative toward the bottom.
        /// The z axis is perpendicular to the screen or keyboard, and is positive extending outward from the screen.
        /// </remarks>
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/Guide/Events/Orientation_and_motion_data_explained#About_rotation"/>
        /// <param name="alpha">Rotation around the z axis -- that is, twisting the device -- causes the alpha rotation angle to change. The alpha angle is 0° when top of the device is pointed directly toward the Earth's north pole, and increases as the device is rotated toward the left</param>
        /// <param name="beta">Rotation around the x axis -- that is, tipping the device away from or toward the user -- causes the beta rotation angle to change. The beta angle is 0° when the device's top and bottom are the same distance from the Earth's surface; it increases toward 180° as the device is tipped forward toward the user, and it decreases toward -180° as the device is tipped backward away from the user.</param>
        /// <param name="gamma">Rotation around the y axis -- that is, tilting the device toward the left or right -- causes the gamma rotation angle to change.The gamma angle is 0° when the device's left and right sides are the same distance from the surface of the Earth, and increases toward 90° as the device is tipped toward the right, and toward -90° as the device is tipped toward the left.</param>
        /// <returns>True if valid angles are returned</returns>
        public static bool GetDeviceOrientation(out float alpha, out float beta, out float gamma)
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

        #region "Simulation"
        public static float[] SimulatedDataArray => _dataArray;
        public static byte[] SimulatedByteArray => _byteArray;

        public static Quaternion SimulatedToUnityRotation(Quaternion q)
        {
            return ToUnityRotation(q.x, q.y, q.z, q.w);
        }
        #endregion
    }

    /// <summary>
    /// Handedness of a controller
    /// </summary>
    public enum WebXRHandedness { Left = 0, Right = 1 }

    /// <summary>
    /// Contains WebXR input source controller state and events
    /// </summary>
    [Serializable]
    public class WebXRInputSource
    {
        #region Events
        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source has fully completed its primary action.
        /// </summary>
        /// <remarks>
        /// On Oculus Quest : Back trigger button was pressed
        /// On Hololens 2 : A air tap has been was performed
        /// On smartphones : The screen was touched
        /// </remarks>
        public readonly UnityEvent Select = new UnityEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectstart event, which means the input source begins its primary action.
        /// </summary>
        public readonly UnityEvent SelectStart = new UnityEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source ends its primary action.
        /// </summary>
        public readonly UnityEvent SelectEnd = new UnityEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source has fully completed its primary squeeze action.
        /// </summary>
        /// <remarks>
        /// On Oculus Quest : Side grip button was pressed
        /// </remarks>
        public readonly UnityEvent Squeeze = new UnityEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectstart event, which means the input source begins its primary squeeze action.
        /// </summary>
        public readonly UnityEvent SqueezeStart = new UnityEvent();

        /// <summary>
        /// Event triggered when the browser triggers a XRSession.selectend event, which means the input source ends its primary squeeze action.
        /// </summary>
        public readonly UnityEvent SqueezeEnd = new UnityEvent();

        #endregion

        #region Constant
        public const int AXES_BUTTON_COUNT = 8;
        #endregion

        #region State
        /// <summary>
        /// Indicates if the input source exists
        /// </summary>
        public bool Available;

        /// <summary>
        /// Handedness of the input source
        /// </summary>
        public readonly WebXRHandedness Handedness;

        /// <summary>
        /// Indicates that the input source is detected and its position is tarcked
        /// </summary>
        public bool IsPositionTracked = false;

        /// <summary>
        /// Current position of the input source if the position is tracked
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Current rotation of the input source if the position is tracked
        public Quaternion Rotation;

        /// <summary>
        /// Number of axes available for this input source
        /// </summary>
        public int AxesCount = 0;

        /// <summary>
        /// Current value of each axes
        /// </summary>
        public readonly float[] Axes = new float[AXES_BUTTON_COUNT];

        /// <summary>
        /// Number of button for this input source
        /// </summary>
        public int ButtonsCount = 0;

        /// <summary>
        /// Current state of each buttons
        /// </summary>
        public readonly WebXRGamepadButton[] Buttons = new WebXRGamepadButton[AXES_BUTTON_COUNT];

        /// <summary>
        /// Describes the method used to produce the target ray, and indicates how the application should present the target ray to the user if desired.
        /// </summary>
        public WebXRTargetRayModes TargetRayMode = WebXRTargetRayModes.None;

        /// <summary>
        /// The input source primary action is active
        /// </summary>
        /// <remarks>
        /// On Oculus Quest : Back trigger button is pressed
        /// On Hololens 2 : A air tap is performed
        /// On smartphones : The screen is touched
        /// </remarks>
        public bool Selected;

        /// <summary>
        /// The input source primary squeeze action is active
        /// </summary>
        /// <remarks>
        /// On Oculus Quest : Side grip button is pressed
        /// </remarks>
        public bool Squeezed;

        /// <summary>
        /// Constains hand joints poses, if hand tracking is enabled
        /// </summary>
        public readonly WebXRHand Hand = new WebXRHand();
        #endregion

        #region Methods

        /// <summary>
        /// Constructor that initialize the input source
        /// </summary>
        /// <param name="handedness">Handedness of the input source</param>
        public WebXRInputSource(WebXRHandedness handedness)
        {
            Handedness = handedness;
            for (int i = 0; i < AXES_BUTTON_COUNT; i++) Buttons[i] = new WebXRGamepadButton();
        }

        /// <summary>
        /// Applies haptic pulse feedback 
        /// </summary>
        /// <param name="intensity">Feedback strength between 0 and 1</param>
        /// <param name="duration">Feedback duration in milliseconds</param>
        public void HapticPulse(float intensity, float duration)
        {
            SimpleWebXR.HapticPulse(Handedness, intensity, duration);
        }


        /// <summary>
        /// Return a string that represent current input source state
        /// </summary>
        /// <returns>String that represent current input source state</returns>
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
                    sb.Append((int)(100 * Axes[i]));
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
        #endregion
    }

    /// <summary>
    /// Describes a button, trigger, thumbstick, or touchpad data 
    /// </summary>
    public class WebXRGamepadButton
    {
        /// <summary>
        /// The amount which the button has been pressed, between 0.0 and 1.0, for buttons that have an analog sensor
        /// </summary>
        public float Value;

        /// <summary>
        /// The touched state of the button
        /// </summary>
        public bool Touched;

        /// <summary>
        /// The pressed state of the button
        /// </summary>
        public bool Pressed;

        /// <summary>
        /// Stringify the button, trigger, thumbstick, or touchpad data
        /// </summary>
        /// <returns>String that describes the button, trigger, thumbstick, or touchpad data</returns>
        public override string ToString()
        {
            return $"{(Touched ? (Pressed ? "Touched and pressed" : "Touched") : (Pressed ? "Pressed" : "Released"))} - {(int)(100 * Value)}%";
        }
    }

    /// <summary>
    /// Describes the method used to produce the target ray, and indicates how the application should present the target ray to the user if desired.
    /// </summary>
    public enum WebXRTargetRayModes
    {
        /// <summary>
        /// No event has yet identified the target ray mode.
        /// </summary>
        None = 0,

        /// <summary>
        /// The target ray originates from either a handheld device or other hand-tracking mechanism and represents that the user is using their hands or the held device for pointing. The orientation of the target ray relative to the tracked object MUST follow platform-specific ergonomics guidelines when available. In the absence of platform-specific guidance, the target ray SHOULD point in the same direction as the user’s index finger if it was outstretched.
        /// </summary>
        TrackedPointer = 1,

        /// <summary>
        /// The input source was an interaction with the canvas element associated with an inline session’s output context, such as a mouse click or touch event.
        /// </summary>
        Screen = 2,

        /// <summary>
        /// The target ray will originate at the viewer and follow the direction it is facing. (This is commonly referred to as a "gaze input" device in the context of head-mounted displays.)
        /// </summary>
        Gaze = 3,
    }

    /// <summary>
    /// Describes spaces that application can use to establish a spatial relationship with the user’s physical environment
    /// </summary>
    public enum WebXRReferenceSpaces
    {
        /// <summary>
        /// Represents a tracking space with a native origin which tracks the position and orientation of the viewer. The y axis equals 0 at head level when session starts.
        /// </summary>
        Viewer,

        /// <summary>
        /// Represents a tracking space with a native origin at the floor in a safe position for the user to stand. The y axis equals 0 at floor level, with the x and z position and orientation initialized based on the conventions of the underlying platform.
        /// </summary>
        LocalFloor
    }

    /// <summary>
    /// Unity event triggered when an input source event is triggered
    /// </summary>
    [Serializable]
    public class WebXRInputEvent : UnityEvent<WebXRInputSource> { }

    /// <summary>
    /// Joint of a hand. Each hand is made up many bones, connected by joints.
    /// </summary>
    public class WebXRJoint
    {
        /// <summary>
        /// Position of the joint
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Rotatiuon of the joint
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Optional joint radius that can be used to represent the joint has a sphere.
        /// </summary>
        /// <remarks>float.NaN if not supported</remarks>
        public float Radius = float.NaN;
    }

    /// <summary>
    /// Describes the poses of hand skeleton joints
    /// </summary>
    public class WebXRHand
    {
        public WebXRHand()
        {
            for (int i = 0; i < JOINT_COUNT; i++) Joints[i] = new WebXRJoint();
        }

        /// <summary>
        /// Indicates if hand tracking is available
        /// </summary>
        public bool Available;

        /// <summary>
        /// Poses of hand skeleton joints
        /// </summary>
        public readonly WebXRJoint[] Joints = new WebXRJoint[JOINT_COUNT];

        #region Constants
        /// <summary>
        /// Number of tracked joints
        /// </summary>
        public const int JOINT_COUNT = 25;

        // INDEX OF EACH JOINT IN ARRAY :

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
        #endregion
    }
}