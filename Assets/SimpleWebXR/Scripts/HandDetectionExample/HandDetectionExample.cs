using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Rufus31415.WebXR.Demo
{
    public class HandDetectionExample : MonoBehaviour
    {
        private GameObject _left;
        private GameObject _right;

        void Start()
        {
            // Create spheres
            _left = CreateHand("Left");
            _right = CreateHand("Right");
        }

        private GameObject CreateHand(string name)
        {
            var hand = new GameObject(name);
            hand.transform.SetParent(this.transform);
            for (int iJoint = 0; iJoint < SimpleWebXR.LeftInput.Hand.Joints.Length; iJoint++)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider collider = sphere.GetComponent<SphereCollider>(); // reference to SphereCollider for IL2CPP
                sphere.transform.SetParent(hand.transform);
                sphere.transform.localScale = Vector3.one * 0.02f;
                sphere.SetActive(false);
            }
            return hand;
        }

        void Update()
        {
            SimpleWebXR.UpdateWebXR();

            UpdateHand(_left, SimpleWebXR.LeftInput.Hand);
            UpdateHand(_right, SimpleWebXR.RightInput.Hand);
        }

        private void UpdateHand(GameObject go, WebXRHand hand)
        {
            for (int i = 0; i < WebXRHand.JOINT_COUNT; i++)
            {
                var sphere = go.transform.GetChild(i);

                // Show the sphere if hand is available
                sphere.gameObject.SetActive(hand.Available);

                // Move the sphere to joint position
                sphere.position = hand.Joints[i].Position;

                // Set radius if supported
                var radius = hand.Joints[i].Radius;
                if (!float.IsNaN(radius)) sphere.transform.localScale = new Vector3(radius, radius, radius);
            }
        }

        private void OnGUI()
        {
            if (SimpleWebXR.InSession) return;

            var style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Once the WebXR session is started, you can view your hands. On some browsers, flags must be enabled to activate hand detection.", style);
        }
    }
}