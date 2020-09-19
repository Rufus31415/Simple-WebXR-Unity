//------------------------------------------------------------------------------ -
// SimpleWebXR
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
using Microsoft.MixedReality.Toolkit.Teleport;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages WebXR Hand Inputs
/// </summary>
[MixedRealityDataProvider(typeof(IMixedRealityInputSystem), SupportedPlatforms.Web | SupportedPlatforms.WindowsEditor, "WebXR Input Manager")]
public class SimpleWebXRInputDeviceManager : BaseInputDeviceManager, IMixedRealityCapabilityCheck
{
    private readonly Dictionary<Handedness, SimpleWebXRHand> trackedHands = new Dictionary<Handedness, SimpleWebXRHand>();
    private readonly Dictionary<Handedness, SimpleWebXRController> trackedControllers = new Dictionary<Handedness, SimpleWebXRController>();

    //private readonly Dictionary<Handedness, SimpleWebXRHand> inactiveHandCache = new Dictionary<Handedness, SimpleWebXRHand>();
    private readonly Dictionary<Handedness, SimpleWebXRController> inactiveControllerCache = new Dictionary<Handedness, SimpleWebXRController>();
    private readonly Dictionary<Handedness, TeleportPointer> teleportPointers = new Dictionary<Handedness, TeleportPointer>();

   // private bool handsInitialized = false;

    private SimpleWebXR xr;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the data provider.</param>
    /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
    /// <param name="name">Friendly name of the service.</param>
    /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
    /// <param name="profile">The service's configuration profile.</param>
    public SimpleWebXRInputDeviceManager(
        IMixedRealityInputSystem inputSystem,
        string name = null,
        uint priority = DefaultPriority,
        BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile)
    {
    }

    public override void Enable()
    {
        base.Enable();
        SetupInput();
        xr = SimpleWebXR.GetInstance();
        
        MixedRealityHandTrackingProfile handTrackingProfile = CoreServices.InputSystem?.InputSystemProfile?.HandTrackingProfile;
        if (handTrackingProfile != null)
        {
            handTrackingProfile.EnableHandMeshVisualization = true;
            handTrackingProfile.EnableHandJointVisualization = true;
        }
    }

    private void SetupInput()
    {/*
            cameraRig = GameObject.FindObjectOfType<OVRCameraRig>();
            if (cameraRig == null)
            {
                var mainCamera = Camera.main;

                // Instantiate camera rig as a child of the MixedRealityPlayspace
                cameraRig = GameObject.Instantiate(MRTKOculusConfig.Instance.OVRCameraRigPrefab);

                // Ensure all related game objects are configured
                cameraRig.EnsureGameObjectIntegrity();

                if (mainCamera != null)
                {
                    // We already had a main camera MRTK probably started using, let's replace the CenterEyeAnchor MainCamera with it
                    GameObject prefabMainCamera = cameraRig.trackingSpace.Find("CenterEyeAnchor").gameObject;
                    prefabMainCamera.SetActive(false);
                    mainCamera.transform.SetParent(cameraRig.trackingSpace.transform);
                    mainCamera.name = prefabMainCamera.name;
                    GameObject.Destroy(prefabMainCamera);
                }
                cameraRig.transform.SetParent(MixedRealityPlayspace.Transform);
            }
            else
            {
                // Ensure all related game objects are configured
                cameraRig.EnsureGameObjectIntegrity();
            }

            bool useAvatarHands = MRTKOculusConfig.Instance.RenderAvatarHandsInsteadOfController;
            // If using Avatar hands, de-activate ovr controller rendering
            foreach (var controllerHelper in cameraRig.gameObject.GetComponentsInChildren<OVRControllerHelper>())
            {
                controllerHelper.gameObject.SetActive(!useAvatarHands);
            }

            if (useAvatarHands && !MRTKOculusConfig.Instance.AllowDevToManageAvatarPrefab)
            {
                // Initialize the local avatar controller
                GameObject.Instantiate(MRTKOculusConfig.Instance.LocalAvatarPrefab, cameraRig.trackingSpace);
            }

            var ovrHands = cameraRig.GetComponentsInChildren<OVRHand>();

            foreach (var ovrHand in ovrHands)
            {
                // Manage Hand skeleton data
                var skeltonDataProvider = ovrHand as OVRSkeleton.IOVRSkeletonDataProvider;
                var skeltonType = skeltonDataProvider.GetSkeletonType();
                var meshRenderer = ovrHand.GetComponent<OVRMeshRenderer>();

                var ovrSkelton = ovrHand.GetComponent<OVRSkeleton>();
                if (ovrSkelton == null)
                {
                    continue;
                }

                switch (skeltonType)
                {
                    case OVRSkeleton.SkeletonType.HandLeft:
                        leftHand = ovrHand;
                        leftSkeleton = ovrSkelton;
                        leftMeshRenderer = meshRenderer;
                        break;
                    case OVRSkeleton.SkeletonType.HandRight:
                        rightHand = ovrHand;
                        rightSkeleton = ovrSkelton;
                        righMeshRenderer = meshRenderer;
                        break;
                }
            }*/
    }


    public override void Disable()
    {
        base.Disable();

        RemoveAllControllerDevices();
        RemoveAllHandDevices();
    }

    public override IMixedRealityController[] GetActiveControllers()
    {
        if (trackedHands.Values.Count > 0)
        {
            return trackedHands.Values.ToArray<IMixedRealityController>();
        }
        else if (trackedControllers.Values.Count > 0)
        {
            return trackedControllers.Values.ToArray<IMixedRealityController>();
        }
        return Enumerable.Empty<IMixedRealityController>().ToArray();
    }

    /// <inheritdoc />
    public bool CheckCapability(MixedRealityCapability capability)
    {
        return (capability == MixedRealityCapability.ArticulatedHand);
    }

    public override void Update()
    {
        base.Update();

        UpdateController(xr.LeftInput, Handedness.Left);
        UpdateController(xr.RightInput, Handedness.Right);
    }

    #region Controller Management

    protected void UpdateController(WebXRInput controller, Handedness handedness)
    {
        if (controller.Available && (controller.IsPositionTracked || controller.Hand.Available))
        {
            if (controller.Hand.Available)
            {
                RemoveControllerDevice(handedness);
                GetOrAddHand(handedness)?.UpdateController(controller);
            }
            else
            {
                RemoveHandDevice(handedness);
                GetOrAddController(handedness)?.UpdateController(controller);
            }
        }
        else
        {
            RemoveControllerDevice(handedness);
            RemoveHandDevice(handedness);
        }
    }

    private SimpleWebXRController GetOrAddController(Handedness handedness)
    {
        if (trackedControllers.ContainsKey(handedness))
        {
            return trackedControllers[handedness];
        }

        var pointers = RequestPointers(SupportedControllerType.ArticulatedHand, handedness);
        var inputSourceType = InputSourceType.Hand;

        IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
        var inputSource = inputSystem?.RequestNewGenericInputSource($"WebXR {handedness} Controller", pointers, inputSourceType);

        if (!inactiveControllerCache.TryGetValue(handedness, out var controller))
        {
            controller = new SimpleWebXRController(TrackingState.Tracked, handedness, inputSource);
        }
        //inactiveHandCache.Remove(handedness);

        for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
        {
            controller.InputSource.Pointers[i].Controller = controller;
        }

        /*
        if (MRTKOculusConfig.Instance.ActiveTeleportPointerMode == MRTKOculusConfig.TeleportPointerMode.Custom && MixedRealityToolkit.IsTeleportSystemEnabled)
        {
            if (!teleportPointers.TryGetValue(handedness, out TeleportPointer pointer))
            {
                pointer = GameObject.Instantiate(MRTKOculusConfig.Instance.CustomTeleportPrefab).GetComponent<TeleportPointer>();
                pointer.gameObject.SetActive(false);
                teleportPointers.Add(handedness, pointer);
            }
            pointer.Controller = controller;
            controller.TeleportPointer = pointer;
        }*/

        inputSystem?.RaiseSourceDetected(controller.InputSource, controller);

        trackedControllers.Add(handedness, controller);

        return controller;
    }

    private void RemoveControllerDevice(Handedness handedness)
    {
        if (trackedControllers.TryGetValue(handedness, out SimpleWebXRController controller))
        {
            RemoveControllerDevice(controller);
        }
    }

    private void RemoveAllControllerDevices()
    {
        if (trackedControllers.Count == 0) return;

        // Create a new list to avoid causing an error removing items from a list currently being iterated on.
        foreach (var controller in new List<SimpleWebXRController>(trackedControllers.Values))
        {
            RemoveControllerDevice(controller);
        }
        trackedControllers.Clear();
    }

    private void RemoveControllerDevice(SimpleWebXRController controller)
    {
        if (controller == null) return;
        CoreServices.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
        trackedControllers.Remove(controller.ControllerHandedness);

        if (teleportPointers.TryGetValue(controller.ControllerHandedness, out TeleportPointer pointer))
        {
            if (pointer == null)
            {
                teleportPointers.Remove(controller.ControllerHandedness);
            }
            else
            {
                pointer.Reset();
            }
            controller.TeleportPointer = null;
        }

        RecyclePointers(controller.InputSource);
    }
    #endregion
    
        #region Hand Management


        private SimpleWebXRHand GetOrAddHand(Handedness handedness)
        {
            if (trackedHands.ContainsKey(handedness))
            {
                return trackedHands[handedness];
            }

            // Add new hand
            var pointers = RequestPointers(SupportedControllerType.ArticulatedHand, handedness);
            var inputSourceType = InputSourceType.Hand;

            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
            var inputSource = inputSystem?.RequestNewGenericInputSource($"WebXR {handedness} Hand", pointers, inputSourceType);

        //if (!inactiveHandCache.TryGetValue(handedness, out var handController))
        // {
        var handController = new SimpleWebXRHand(TrackingState.Tracked, handedness, inputSource);
            //}
            //inactiveHandCache.Remove(handedness);

            for (int i = 0; i < handController.InputSource?.Pointers?.Length; i++)
            {
                handController.InputSource.Pointers[i].Controller = handController;
            }
            /*
            if (MRTKOculusConfig.Instance.ActiveTeleportPointerMode == MRTKOculusConfig.TeleportPointerMode.Custom && MixedRealityToolkit.IsTeleportSystemEnabled)
            {
                if (!teleportPointers.TryGetValue(handedness, out TeleportPointer pointer))
                {
                    pointer = GameObject.Instantiate(MRTKOculusConfig.Instance.CustomTeleportPrefab).GetComponent<TeleportPointer>();
                    pointer.gameObject.SetActive(false);
                    teleportPointers.Add(handedness, pointer);
                }
                pointer.Controller = handController;
                handController.TeleportPointer = pointer;
            }
            */

            inputSystem?.RaiseSourceDetected(handController.InputSource, handController);

            trackedHands.Add(handedness, handController);

            return handController;
        }

        private void RemoveHandDevice(Handedness handedness)
        {
            if (trackedHands.TryGetValue(handedness, out SimpleWebXRHand hand))
            {
                RemoveHandDevice(hand);
            }
        }

        private void RemoveAllHandDevices()
        {
            if (trackedHands.Count == 0) return;

            // Create a new list to avoid causing an error removing items from a list currently being iterated on.
            foreach (var hand in new List<SimpleWebXRHand>(trackedHands.Values))
            {
                RemoveHandDevice(hand);
            }
            trackedHands.Clear();
        }

        private void RemoveHandDevice(SimpleWebXRHand hand)
        {
            if (hand == null) return;

            if (teleportPointers.TryGetValue(hand.ControllerHandedness, out TeleportPointer pointer))
            {
                if (pointer == null)
                {
                    teleportPointers.Remove(hand.ControllerHandedness);
                }
                else
                {
                    pointer.Reset();
                }
            }

            CoreServices.InputSystem?.RaiseSourceLost(hand.InputSource, hand);
            trackedHands.Remove(hand.ControllerHandedness);

            RecyclePointers(hand.InputSource);
        }
        #endregion
    
}