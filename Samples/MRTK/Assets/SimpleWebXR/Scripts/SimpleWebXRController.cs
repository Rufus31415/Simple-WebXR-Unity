//------------------------------------------------------------------------------ -
//SimpleWebXR
//------------------------------------------------------------------------------ -
//
//MIT License
//
//Copyright(c) 2020 GIRAUD Florent
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files(the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions :
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//------------------------------------------------------------------------------ -

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;
using TeleportPointer = Microsoft.MixedReality.Toolkit.Teleport.TeleportPointer;

    [MixedRealityController(SupportedControllerType.ArticulatedHand, new[] { Handedness.Left, Handedness.Right })]
    public class SimpleWebXRController : BaseController, IMixedRealityHand
    {
        private MixedRealityPose currentPointerPose = MixedRealityPose.ZeroIdentity;

        private MixedRealityPose currentIndexPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose currentGripPose = MixedRealityPose.ZeroIdentity;

        /// <summary>
        /// Teleport pointer reference. Needs custom pointer because MRTK does not support teleporting with articulated hands.
        /// </summary>
        public TeleportPointer TeleportPointer { get; set; }
    /*
        private List<Renderer> handRenderers = new List<Renderer>();
        private Material handMaterial = null;
    */
        private bool isInPointingPose = true;
        public override bool IsInPointingPose => isInPointingPose;

        #region AvatarHandReferences
    /*
        private GameObject handRoot = null;
        private GameObject handGrip = null;

        private GameObject handIndex1 = null;
        private GameObject handIndex2 = null;
        private GameObject handIndex3 = null;
        private GameObject handIndexTip = null;

        private GameObject handMiddle1 = null;
        private GameObject handMiddle2 = null;
        private GameObject handMiddle3 = null;
        private GameObject handMiddleTip = null;

        private GameObject handRing1 = null;
        private GameObject handRing2 = null;
        private GameObject handRing3 = null;
        private GameObject handRingTip = null;

        private GameObject handPinky0 = null;
        private GameObject handPinky1 = null;
        private GameObject handPinky2 = null;
        private GameObject handPinky3 = null;
        private GameObject handPinkyTip = null;

        private GameObject handThumb1 = null;
        private GameObject handThumb2 = null;
        private GameObject handThumb3 = null;
        private GameObject handThumbTip = null;*/
        #endregion

        protected readonly Dictionary<TrackedHandJoint, MixedRealityPose> jointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
    /*
        private const float cTriggerDeadZone = 0.1f;
        private int pinchStrengthProp;
    */

        public SimpleWebXRController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {

        }

        /// <summary>
        /// The Windows Mixed Reality Controller default interactions.
        /// </summary>
        /// <remarks>A single interaction mapping works for both left and right controllers.</remarks>
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, new MixedRealityInputAction(4, "Pointer Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, new MixedRealityInputAction(3, "Grip Pose", AxisType.SixDof)),
            new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, new MixedRealityInputAction(1, "Select", AxisType.Digital)),
            new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, new MixedRealityInputAction(7, "Grip Press", AxisType.SingleAxis)),
        };

        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        [System.Obsolete]
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }

        /// <summary>
        /// Update the controller data from the provided platform state
        /// </summary>
        /// <param name="interactionSourceState">The InteractionSourceState retrieved from the platform</param>
        public void UpdateController(WebXRInput controller)
        {
            if (!Enabled)
            {
                return;
            }


        var pose = new MixedRealityPose(controller.Position, controller.Rotation);


            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        Interactions[i].PoseData = pose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, pose);
                        }
                        break;
                    case DeviceInputType.SpatialGrip:
                        Interactions[i].PoseData = pose;
                        if (Interactions[i].Changed)
                        {
                            CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, Interactions[i].MixedRealityInputAction, pose);
                        }
                        break;
                    case DeviceInputType.Select:
                        Interactions[i].BoolData = controller.Selected;

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
                        Interactions[i].BoolData = controller.Squeezed;

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
                }
            }
        }

    public bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose)
    {
        pose = new MixedRealityPose();
        return false;
    }

    /*
        private void UpdateTeleport(Vector2 stickInput)
        {
            if (MRTKOculusConfig.Instance.ActiveTeleportPointerMode == MRTKOculusConfig.TeleportPointerMode.None) return;

            MixedRealityInputAction teleportAction = MixedRealityInputAction.None;
            IMixedRealityTeleportPointer teleportPointer = TeleportPointer;

            // Check if we're focus locked or near something interactive to avoid teleporting unintentionally.
            bool anyPointersLockedWithHand = false;
            for (int i = 0; i < InputSource?.Pointers?.Length; i++)
            {
                if (InputSource.Pointers[i] == null) continue;
                if (InputSource.Pointers[i] is IMixedRealityNearPointer)
                {
                    var nearPointer = (IMixedRealityNearPointer)InputSource.Pointers[i];
                    anyPointersLockedWithHand |= nearPointer.IsNearObject;
                }
                anyPointersLockedWithHand |= InputSource.Pointers[i].IsFocusLocked;

                // If official teleport mode and we have a teleport pointer registered, we get the input action to trigger it.
                if (MRTKOculusConfig.Instance.ActiveTeleportPointerMode == MRTKOculusConfig.TeleportPointerMode.Official
                    && InputSource.Pointers[i] is IMixedRealityTeleportPointer)
                {
                    teleportPointer = (TeleportPointer)InputSource.Pointers[i];
                    teleportAction = ((TeleportPointer)teleportPointer).TeleportInputAction;
                }
            }

            bool isReadyForTeleport = !anyPointersLockedWithHand && stickInput != Vector2.zero;

            // If not ready for teleport, we raise a cancellation event to prevent accidental teleportation.
            if (!isReadyForTeleport && teleportPointer != null)
            {
                CoreServices.TeleportSystem?.RaiseTeleportCanceled(teleportPointer, null);
            }

            isInPointingPose = !isReadyForTeleport;

            RaiseTeleportInput(isInPointingPose ? Vector2.zero : stickInput, teleportAction, isReadyForTeleport);
        }

    private void RaiseTeleportInput(Vect*leportInput, MixedRealityInputAction teleportAction, bool isReadyForTeleport)
    {
        if (teleportAction.Equals(MixedRealityInputAction.None)) return;
        CoreServices.InputSystem?.RaisePositionInputChanged(InputSource, ControllerHandedness, teleportAction, teleportInput);
    }

        /// <summary>
        /// Updates material instance used for avatar hands.
        /// </summary>
        /// <param name="newMaterial">Material to use for hands.</param>
        public void UpdateAvatarMaterial(Material newMaterial)
        {
            if (newMaterial == null || !MRTKOculusConfig.Instance.UseCustomHandMaterial) return;
            if (handMaterial != null)
            {
                Object.Destroy(handMaterial);
            }
            handMaterial = new Material(newMaterial);

            ApplyHandMaterial();
        }

        /// <summary>
        /// Updates hand material set on hand renderers with member variable stored on controller.
        /// </summary>
        public void ApplyHandMaterial()
        {
            foreach (var handRenderer in handRenderers)
            {
                handRenderer.sharedMaterial = handMaterial;
            }
        }

        private void UpdateHandRenderers(GameObject handRoot)
        {
            if (handRoot == null) return;
            handRenderers = new List<Renderer>(handRoot.GetComponentsInChildren<Renderer>());
            ApplyHandMaterial();
        }

        private void UpdateAvatarJointPoses()
        {
            // Reference
            /*
            { BoneId.Hand_Thumb1, TrackedHandJoint.ThumbMetacarpalJoint },
            { BoneId.Hand_Thumb2, TrackedHandJoint.ThumbProximalJoint },
            { BoneId.Hand_Thumb3, TrackedHandJoint.ThumbDistalJoint },
            { BoneId.Hand_ThumbTip, TrackedHandJoint.ThumbTip },
            { BoneId.Hand_Index1, TrackedHandJoint.IndexKnuckle },
            { BoneId.Hand_Index2, TrackedHandJoint.IndexMiddleJoint },
            { BoneId.Hand_Index3, TrackedHandJoint.IndexDistalJoint },
            { BoneId.Hand_IndexTip, TrackedHandJoint.IndexTip },
            { BoneId.Hand_Middle1, TrackedHandJoint.MiddleKnuckle },
            { BoneId.Hand_Middle2, TrackedHandJoint.MiddleMiddleJoint },
            { BoneId.Hand_Middle3, TrackedHandJoint.MiddleDistalJoint },
            { BoneId.Hand_MiddleTip, TrackedHandJoint.MiddleTip },
            { BoneId.Hand_Ring1, TrackedHandJoint.RingKnuckle },
            { BoneId.Hand_Ring2, TrackedHandJoint.RingMiddleJoint },
            { BoneId.Hand_Ring3, TrackedHandJoint.RingDistalJoint },
            { BoneId.Hand_RingTip, TrackedHandJoint.RingTip },
            { BoneId.Hand_Pinky1, TrackedHandJoint.PinkyKnuckle },
            { BoneId.Hand_Pinky2, TrackedHandJoint.PinkyMiddleJoint },
            { BoneId.Hand_Pinky3, TrackedHandJoint.PinkyDistalJoint },
            { BoneId.Hand_PinkyTip, TrackedHandJoint.PinkyTip },
            { BoneId.Hand_WristRoot, TrackedHandJoint.Wrist },
            

            // Thumb
            UpdateAvatarJointPose(TrackedHandJoint.ThumbMetacarpalJoint, handThumb1.transform.position, handThumb1.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.ThumbProximalJoint, handThumb2.transform.position, handThumb2.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.ThumbDistalJoint, handThumb3.transform.position, handThumb3.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.ThumbTip, handThumbTip.transform.position, handThumbTip.transform.rotation);

            // Index
            UpdateAvatarJointPose(TrackedHandJoint.IndexKnuckle, handIndex1.transform.position, handIndex1.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.IndexMiddleJoint, handIndex2.transform.position, handIndex2.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.IndexDistalJoint, handIndex3.transform.position, handIndex3.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.IndexTip, handIndexTip.transform.position, handIndexTip.transform.rotation);

            // Middle
            UpdateAvatarJointPose(TrackedHandJoint.MiddleKnuckle, handMiddle1.transform.position, handMiddle1.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.MiddleMiddleJoint, handMiddle2.transform.position, handMiddle2.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.MiddleDistalJoint, handMiddle3.transform.position, handMiddle3.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.MiddleTip, handMiddleTip.transform.position, handMiddleTip.transform.rotation);

            // Ring
            UpdateAvatarJointPose(TrackedHandJoint.RingKnuckle, handRing1.transform.position, handRing1.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.RingMiddleJoint, handRing2.transform.position, handRing2.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.RingDistalJoint, handRing3.transform.position, handRing3.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.RingTip, handRingTip.transform.position, handRingTip.transform.rotation);

            // Pinky
            UpdateAvatarJointPose(TrackedHandJoint.PinkyKnuckle, handPinky1.transform.position, handPinky1.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.PinkyMiddleJoint, handPinky2.transform.position, handPinky2.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.PinkyDistalJoint, handPinky3.transform.position, handPinky3.transform.rotation);
            UpdateAvatarJointPose(TrackedHandJoint.PinkyTip, handPinkyTip.transform.position, handPinkyTip.transform.rotation);

            // Wrist
            // Wrist rotation works very differently from the other joints, so we correct it separately
            UpdateJointPose(TrackedHandJoint.Wrist, handPinky0.transform.position, handGrip.transform.rotation);

            UpdatePalm();
        }

        protected void UpdatePalm()
        {
            bool hasMiddleKnuckle = TryGetJoint(TrackedHandJoint.MiddleKnuckle, out var middleKnucklePose);
            bool hasWrist = TryGetJoint(TrackedHandJoint.Wrist, out var wristPose);

            if (hasMiddleKnuckle && hasWrist)
            {
                Vector3 wristRootPosition = wristPose.Position;
                Vector3 middle3Position = middleKnucklePose.Position;

                Vector3 palmPosition = Vector3.Lerp(wristRootPosition, middle3Position, 0.5f);
                Quaternion palmRotation = CorrectPalmRotation(wristPose.Rotation);

                UpdateJointPose(TrackedHandJoint.Palm, palmPosition, palmRotation);
            }
        }

        private Quaternion CorrectPalmRotation(Quaternion palmRotation)
        {
            Quaternion correctedPalmRotation = palmRotation;

            // WARNING THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK - This fix is a hack to fix broken and inconsistent rotations for hands
            if (ControllerHandedness == Handedness.Right)
            {
                correctedPalmRotation *= Quaternion.Euler(90f, 90f, 0f);
            }
            else
            {
                correctedPalmRotation *= Quaternion.Euler(90f, 0, 90f);
            }
            return correctedPalmRotation;
        }

        private void UpdateFakeHandJointPoses()
        {
            // While we can get pretty much everything done with just the grip pose, we simulate hand sizes for bounds calculations

            // Index
            Vector3 fingerTipPos = currentGripPose.Position + currentGripPose.Rotation * Vector3.forward * 0.1f;
            UpdateJointPose(TrackedHandJoint.IndexTip, fingerTipPos, currentGripPose.Rotation);

            // Handed directional offsets
            Vector3 inWardVector;
            if (ControllerHandedness == Handedness.Left)
            {
                inWardVector = currentGripPose.Rotation * Vector3.right;
            }
            else
            {
                inWardVector = currentGripPose.Rotation * -Vector3.right;
            }

            // Thumb
            Vector3 thumbPose = currentGripPose.Position + inWardVector * 0.04f;
            UpdateJointPose(TrackedHandJoint.ThumbTip, thumbPose, currentGripPose.Rotation);
            UpdateJointPose(TrackedHandJoint.ThumbMetacarpalJoint, thumbPose, currentGripPose.Rotation);
            UpdateJointPose(TrackedHandJoint.ThumbDistalJoint, thumbPose, currentGripPose.Rotation);

            // Pinky
            Vector3 pinkyPose = currentGripPose.Position - inWardVector * 0.03f;
            UpdateJointPose(TrackedHandJoint.PinkyKnuckle, pinkyPose, currentGripPose.Rotation);

            // Palm
            UpdateJointPose(TrackedHandJoint.Palm, currentGripPose.Position, currentGripPose.Rotation);

            // Wrist
            Vector3 wristPose = currentGripPose.Position - currentGripPose.Rotation * Vector3.forward * 0.05f;
            UpdateJointPose(TrackedHandJoint.Palm, wristPose, currentGripPose.Rotation);
        }

        protected void UpdateAvatarJointPose(TrackedHandJoint joint, Vector3 position, Quaternion rotation)
        {
            Quaternion correctedRotation = rotation;

            // WARNING THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK - This fix is a hack to fix broken and inconsistent rotations for hands
            if (ControllerHandedness == Handedness.Left)
            {
                // Rotate palm 180 on X to flip up
                // Rotate palm 90 degrees on y to align x with right
                correctedRotation *= Quaternion.Euler(180f, 90f, 0f);
            }
            else
            {
                // Rotate palm 90 degrees on y to align x with right
                correctedRotation *= Quaternion.Euler(0f, 90f, 0f);
            }

            UpdateJointPose(joint, position, correctedRotation);
        }

        protected void UpdateJointPose(TrackedHandJoint joint, Vector3 position, Quaternion rotation)
        {
            MixedRealityPose pose = new MixedRealityPose(position, rotation);
            if (!jointPoses.ContainsKey(joint))
            {
                jointPoses.Add(joint, pose);
            }
            else
            {
                jointPoses[joint] = pose;
            }

            CoreServices.InputSystem?.RaiseHandJointsUpdated(InputSource, ControllerHandedness, jointPoses);
        }

        private void UpdateIndexFingerData(MixedRealityInteractionMapping interactionMapping)
        {
            if (jointPoses.TryGetValue(TrackedHandJoint.IndexTip, out var pose))
            {
                currentIndexPose.Rotation = pose.Rotation;
                currentIndexPose.Position = pose.Position;
            }

            interactionMapping.PoseData = currentIndexPose;

            // If our value changed raise it.
            if (interactionMapping.Changed)
            {
                // Raise input system Event if it enabled
                CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, currentIndexPose);
            }
        }

        public bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose)
        {
            if (jointPoses.TryGetValue(joint, out pose))
            {
                return true;
            }
            pose = currentGripPose;
            return true;
        }*/
}
