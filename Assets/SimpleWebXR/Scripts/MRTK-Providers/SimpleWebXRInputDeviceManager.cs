using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Teleport;
using Microsoft.MixedReality.Toolkit.Utilities;
using Rufus31415.WebXR;
using System.Collections.Generic;
using System.Linq;

namespace Rufus31415.MixedReality.Toolkit.WebXR.Input
{
    /// <summary>
    /// Manages WebXR Hand Inputs
    /// </summary>
    [MixedRealityDataProvider(typeof(IMixedRealityInputSystem), SupportedPlatforms.Web | SupportedPlatforms.WindowsEditor, "WebXR Input Manager")]
    public class SimpleWebXRInputDeviceManager : BaseInputDeviceManager, IMixedRealityCapabilityCheck
    {
        private readonly Dictionary<Handedness, SimpleWebXRHand> trackedHands = new Dictionary<Handedness, SimpleWebXRHand>();
        private readonly Dictionary<Handedness, SimpleWebXRController> trackedControllers = new Dictionary<Handedness, SimpleWebXRController>();

        private readonly Dictionary<Handedness, SimpleWebXRController> inactiveControllerCache = new Dictionary<Handedness, SimpleWebXRController>();

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

            // create SimpleWebXR component to display "Start AR" or "Start VR" button and call SimpleWebXR.UpdateWebXR() at each frame
            SimpleWebXR.EnsureInstance();
        }

        public override void Disable()
        {
            base.Disable();

            SimpleWebXR.EndSession();

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

            UpdateController(SimpleWebXR.LeftInput, Handedness.Left);
            UpdateController(SimpleWebXR.RightInput, Handedness.Right);
        }

        #region Controller Management

        protected void UpdateController(WebXRInputSource controller, Handedness handedness)
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

            for (int i = 0; i < controller.InputSource?.Pointers?.Length; i++)
            {
                controller.InputSource.Pointers[i].Controller = controller;
            }

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

            var pointers = RequestPointers(SupportedControllerType.ArticulatedHand, handedness);
            var inputSourceType = InputSourceType.Hand;

            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;
            var inputSource = inputSystem?.RequestNewGenericInputSource($"WebXR {handedness} Hand", pointers, inputSourceType);

            var handController = new SimpleWebXRHand(TrackingState.Tracked, handedness, inputSource);

            for (int i = 0; i < handController.InputSource?.Pointers?.Length; i++)
            {
                handController.InputSource.Pointers[i].Controller = handController;
            }

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

            CoreServices.InputSystem?.RaiseSourceLost(hand.InputSource, hand);
            trackedHands.Remove(hand.ControllerHandedness);

            RecyclePointers(hand.InputSource);
        }
        #endregion
    }
}