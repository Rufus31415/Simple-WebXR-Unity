using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class HandDetectionExample : MonoBehaviour
{
    private SimpleWebXR _xr;

    private GameObject _left;
    private GameObject _right;

    void Start()
    {
        _xr = SimpleWebXR.GetInstance();

        if (!_xr) return;

        // Create spheres
        _left = CreateHand("Left");
        _right = CreateHand("Right");
    }

    private GameObject CreateHand(string name)
    {
        var hand = new GameObject(name);
        hand.transform.SetParent(this.transform);
        for (int iJoint = 0; iJoint < _xr.LeftInput.Hand.Joints.Length; iJoint++)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            SphereCollider collider= sphere.GetComponent<SphereCollider>(); // reference to SphereCollider for IL2CPP
            sphere.transform.SetParent(hand.transform);
            sphere.transform.localScale = Vector3.one * 0.02f;
            sphere.SetActive(false);
        }
        return hand;
    }

    void Update()
    {
        if (!_xr) return;

        UpdateHand(_left, _xr.LeftInput.Hand);
        UpdateHand(_right, _xr.RightInput.Hand);
    }

    private void UpdateHand(GameObject go, WebXRHand hand)
    {
        for (int i = 0; i < WebXRHand.JOINT_COUNT; i++) {
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

#if UNITY_EDITOR // debug purposes
    private void OnGUI()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Head : ");
        sb.Append("   Pose : ");
        sb.AppendLine(_xr.LeftEye.transform.position.ToString());

        sb.AppendLine("Left contr : ");
        sb.Append("   Pose : ");
        sb.AppendLine(_xr.LeftInput.Position.ToString());

        sb.AppendLine("Right contr : ");
        sb.Append("   Pose : ");
        sb.AppendLine(_xr.RightInput.Position.ToString());

        sb.AppendLine("Left Hand : ");
        sb.Append("   Available : ");
        sb.AppendLine(_xr.LeftInput.Hand.Available.ToString());
        sb.Append("   Pose : ");
        sb.AppendLine(_xr.LeftInput.Hand.Joints[0].Position.ToString());

        sb.AppendLine("Right Hand : ");
        sb.Append("   Available : ");
        sb.AppendLine(_xr.RightInput.Hand.Available.ToString());
        sb.Append("   Pose : ");
        sb.AppendLine(_xr.RightInput.Hand.Joints[0].Position.ToString());

        GUI.Label(new Rect(0,0,Screen.width, Screen.height), sb.ToString());
    }
#endif  
}
