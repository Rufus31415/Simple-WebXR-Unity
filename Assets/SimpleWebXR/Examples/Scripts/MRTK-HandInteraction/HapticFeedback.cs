using Rufus31415.WebXR;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos
{
    public class HapticFeedback : MonoBehaviour
    {
        public void Pulse(float duration)
        {
            SimpleWebXR.HapticPulse(SimpleWebXR.LeftInput.Selected ? WebXRHandedness.Left : WebXRHandedness.Right, 1, duration);
        }
    }


}