using UnityEngine;

namespace Rufus31415.WebXR.Editor {
    public class SimpleWebXRSimulator : MonoBehaviour {

        public bool SimulationIsArSupported = true;
        public bool SimulationIsVrSupported = false;
        public bool SimulationRender2Eyes = false;

        public WebXRTargetRayModes SimulationMode = WebXRTargetRayModes.TrackedPointer;

        public bool SimulationLeftSelect = false;
        public bool SimulationRightSelect = false;


        public float SimulationUserHeight = 1.8f;

#if UNITY_EDITOR
        private static GameObject _simulateLeft;
        private static GameObject _simulateRight;
        private static GameObject _simulateHead;

        private static bool _sessionStarted;

        private void Start()
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



        private void Update()
        {
            var dataArray = SimpleWebXR.SimulatedDataArray;
            var byteArray = SimpleWebXR.SimulatedByteArray;

            // [0] : number of views (0 : session is stopped)
            byteArray[0] = (byte)(_sessionStarted ? (SimulationRender2Eyes ? 3 : 1) : 0);


            if (_sessionStarted)
            {
                // [0] -> [15] : projection matrix of view 1
                var pm = Camera.main.projectionMatrix;
                dataArray[0] = pm.m00;
                dataArray[4] = pm.m01;
                dataArray[8] = pm.m02;
                dataArray[12] = pm.m03;
                dataArray[1] = pm.m10;
                dataArray[5] = pm.m11;
                dataArray[9] = pm.m12;
                dataArray[13] = pm.m13;
                dataArray[2] = pm.m20;
                dataArray[6] = pm.m21;
                dataArray[10] = pm.m22;
                dataArray[14] = pm.m23;
                dataArray[3] = pm.m30;
                dataArray[7] = pm.m31;
                dataArray[11] = pm.m32;
                dataArray[15] = pm.m33;

                // [16], [17], [18] : X, Y, Z position in m  of view 1
                dataArray[16] = _simulateHead.transform.position.x;
                dataArray[17] = _simulateHead.transform.position.y;
                dataArray[18] = -_simulateHead.transform.position.z;

                // [19], [20], [21], [22] : RX, RY, RZ, RW rotation (quaternion)  of view 1
                var rotation = SimpleWebXR.SimulatedToUnityRotation(_simulateHead.transform.rotation);
                dataArray[19] = rotation.x;
                dataArray[20] = rotation.y;
                dataArray[21] = rotation.z;
                dataArray[22] = rotation.w;

                // [23] -> [26] : Viewport X, Y, width, height  of view 1
                dataArray[23] = 0;
                dataArray[24] = 0;
                dataArray[25] = 1;
                dataArray[26] = 1;

                if (SimulationRender2Eyes)
                {
                    dataArray[25] = 0.5f;

                    // [27] -> [42] : projection matrix of view 2
                    // [43], [44], [45] : X, Y, Z position in m  of view 2
                    // [46], [47], [48], [49] : RX, RY, RZ, RW rotation (quaternion)  of view 2
                    for (int i = 0; i <= 26; i++) dataArray[27 + i] = dataArray[i];

                    // [50] -> [53] : Viewport X, Y, width, height  of view 2
                    dataArray[50] = 0.5f;
                }



                // [54] -> [60] : Left input x, y, z, rx, ry, rz, rw
                rotation = SimpleWebXR.SimulatedToUnityRotation(_simulateLeft.transform.rotation);
                dataArray[54] = _simulateLeft.transform.position.x;
                dataArray[55] = _simulateLeft.transform.position.y;
                dataArray[56] = -_simulateLeft.transform.position.z;
                dataArray[57] = rotation.x;
                dataArray[58] = rotation.y;
                dataArray[59] = rotation.z;
                dataArray[60] = rotation.w;

                // [77] -> [83] : right input x, y, z, rx, ry, rz, rw
                rotation = SimpleWebXR.SimulatedToUnityRotation(_simulateRight.transform.rotation);
                dataArray[77] = _simulateRight.transform.position.x;
                dataArray[78] = _simulateRight.transform.position.y;
                dataArray[79] = -_simulateRight.transform.position.z;
                dataArray[80] = rotation.x;
                dataArray[81] = rotation.y;
                dataArray[82] = rotation.z;
                dataArray[83] = rotation.w;

                // [100] : user height
                dataArray[100] = SimulationUserHeight;

                // [4] : left input has position info
                byteArray[4] = 1;

                // [24] : right input has position info
                byteArray[24] = 1;

                // [44] : Left controller active
                byteArray[44] = 1;

                // [45] : Right controller active
                byteArray[45] = 1;

                // [1] : left controller events
                if (SimulationLeftSelect && !SimpleWebXR.LeftInput.Selected) byteArray[1] = 8;
                else if (!SimulationLeftSelect && SimpleWebXR.LeftInput.Selected) byteArray[1] = 32;
                else byteArray[1] = 0;

                // [2] : right controller events
                if (SimulationRightSelect && !SimpleWebXR.RightInput.Selected) byteArray[2] = 8;
                else if (!SimulationRightSelect && SimpleWebXR.RightInput.Selected) byteArray[2] = 32;
                else byteArray[2] = 0;

                // [5] : left input target ray mode
                byteArray[5] = (byte)SimulationMode;

                // [25] : right input target ray mode
                byteArray[25] = byteArray[5];

            }
        }

        private void OnApplicationQuit()
        {
            DestroyImmediate(gameObject);
        }


        [UnityEditor.MenuItem("SimpleWebXR/Simulation/Start")]
        public static void StartSimulation()
        {
            if (!Application.isPlaying) return;

            if (FindObjectOfType<SimpleWebXRSimulator>()) return;
            var go = new GameObject("WebXR Simulation");
            go.AddComponent<SimpleWebXRSimulator>();
        }

        [UnityEditor.MenuItem("SimpleWebXR/Simulation/Set camera none")]
        public static void SetCameraNone()
        {
            SimpleWebXR.SimulatedDataArray[0] = 0;
        }

        [UnityEditor.MenuItem("SimpleWebXR/Simulation/Set camera left")]
        public static void SetCameraLeft()
        {
            SimpleWebXR.SimulatedDataArray[0] = 1;
        }

        [UnityEditor.MenuItem("SimpleWebXR/Simulation/Set camera right")]
        public static void SetCameraRight()
        {
            SimpleWebXR.SimulatedDataArray[0] = 2;
        }

        [UnityEditor.MenuItem("SimpleWebXR/Simulation/Set camera both")]
        public static void SetCameraBoth()
        {
            SimpleWebXR.SimulatedDataArray[0] = 3;
        }
#endif

    }
}
