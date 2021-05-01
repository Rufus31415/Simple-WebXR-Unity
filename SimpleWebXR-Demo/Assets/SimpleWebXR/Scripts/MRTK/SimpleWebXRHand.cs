using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Rufus31415.WebXR;
using System.Collections.Generic;
using UnityEngine;

namespace Rufus31415.MixedReality.Toolkit.WebXR.Input
{

    [MixedRealityController(SupportedControllerType.ArticulatedHand, new[] { Handedness.Left, Handedness.Right })]
    public class SimpleWebXRHand : BaseHand
    {
        public SimpleWebXRHand(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {

        }

        #region IMixedRealityHand Implementation

        protected readonly Dictionary<TrackedHandJoint, MixedRealityPose> jointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
        /// <inheritdoc/>
        public override bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose)
        {
            return jointPoses.TryGetValue(joint, out pose);
        }

        #endregion IMixedRealityHand Implementation

        private ArticulatedHandDefinition handDefinition;
        private ArticulatedHandDefinition HandDefinition => handDefinition ?? (handDefinition = Definition as ArticulatedHandDefinition);



        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, new MixedRealityInputAction(4, "Pointer Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, new MixedRealityInputAction(3, "Grip Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, new MixedRealityInputAction(1, "Select", AxisType.Digital)),
            new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, new MixedRealityInputAction(7, "Grip Press", AxisType.SingleAxis)),
            new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger,  new MixedRealityInputAction(13, "Index Finger Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(5, "Teleport Pose", AxisType.SixDof, DeviceInputType.Thumb,  new MixedRealityInputAction(14, "Teleport Pose", AxisType.SixDof)),
    };

        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        public override void SetupDefaultInteractions()
        {
            AssignControllerMappings(DefaultInteractions);
        }

        public override bool IsInPointingPose => true;

        private MixedRealityPose GetJointMixedRealityPose(WebXRJoint joint)
        {

            var position = MixedRealityPlayspace.TransformPoint(joint.Position);
            var rotation = MixedRealityPlayspace.Rotation * joint.Rotation;

            return new MixedRealityPose(position, rotation);
        }

        public void UpdateController(WebXRInputSource controller)
        {
            if (!Enabled) return;

            IsPositionAvailable = IsRotationAvailable = controller.Hand.Available;


            jointPoses[TrackedHandJoint.Wrist] = GetJointMixedRealityPose(controller.Hand.Joints[WebXRHand.WRIST]);


            for (int i = WebXRHand.THUMB_METACARPAL; i < WebXRHand.JOINT_COUNT; i++)
            {
                var joint = controller.Hand.Joints[i];

                jointPoses[(TrackedHandJoint)(i + 2)] = GetJointMixedRealityPose(joint);
            }

            jointPoses[TrackedHandJoint.Palm] = new MixedRealityPose((jointPoses[TrackedHandJoint.MiddleMetacarpal].Position + jointPoses[TrackedHandJoint.MiddleMetacarpal].Position)/2, jointPoses[TrackedHandJoint.MiddleMetacarpal].Rotation) ;

            var indexPose = jointPoses[TrackedHandJoint.IndexTip];

            bool isSelecting;
            MixedRealityPose spatialPointerPose;


            if (controller.IsPositionTracked)
            {
                isSelecting = controller.Selected;
                spatialPointerPose = new MixedRealityPose(MixedRealityPlayspace.TransformPoint(controller.Position), MixedRealityPlayspace.Rotation * controller.Rotation);
            }
            else
            {
                // Is selecting if thumb tip and index tip are close
                isSelecting = Vector3.Distance(controller.Hand.Joints[WebXRHand.THUMB_PHALANX_TIP].Position, controller.Hand.Joints[WebXRHand.INDEX_PHALANX_TIP].Position) < 0.04;

                // The hand ray starts from the middle of thumb tip and index tip
                HandRay.Update((controller.Hand.Joints[WebXRHand.THUMB_PHALANX_TIP].Position + controller.Hand.Joints[WebXRHand.INDEX_PHALANX_TIP].Position) / 2, new Vector3(0.3f, -0.4f, 0.9f), CameraCache.Main.transform, ControllerHandedness);

                Ray ray = HandRay.Ray;

                spatialPointerPose = new MixedRealityPose(ray.origin, Quaternion.LookRotation(ray.direction));
            }

            CoreServices.InputSystem?.RaiseSourcePoseChanged(InputSource, this, spatialPointerPose);

            CoreServices.InputSystem?.RaiseHandJointsUpdated(InputSource, ControllerHandedness, jointPoses);

            UpdateVelocity();

            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        Interactions[i].PoseData = spatialPointerPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, Interactions[i].PoseData);
                        }
                        break;
                    case DeviceInputType.SpatialGrip:
                        Interactions[i].PoseData = indexPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, Interactions[i].PoseData);
                        }
                        break;
                    case DeviceInputType.Select:
                        Interactions[i].BoolData = isSelecting || controller.TargetRayMode == WebXRTargetRayModes.Screen;

                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                    case DeviceInputType.TriggerPress:
                        Interactions[i].BoolData = isSelecting;

                        if (Interactions[i].Changed)
                        {
                            if (Interactions[i].BoolData)
                            {
                                CoreServices.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                            else
                            {
                                CoreServices.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction);
                            }
                        }
                        break;
                    case DeviceInputType.IndexFinger:
                        Interactions[i].PoseData = indexPose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, Interactions[i].PoseData);
                        }
                        break;
                    case DeviceInputType.ThumbStick:
                        HandDefinition?.UpdateCurrentTeleportPose(Interactions[i]);
                        break;
                }
            }
        }
    }
}