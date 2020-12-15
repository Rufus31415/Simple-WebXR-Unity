using Microsoft.MixedReality.Toolkit;
using Rufus31415.WebXR;
using UnityEngine;

public class OculusQuestThumstickHandler : MonoBehaviour
{
    /// <summary>
    /// Translation speed in m/s
    /// </summary>
    public const float NOMINAL_TRANSLATION_SPEED = 0.5f;

    /// <summary>
    /// Rotation speed in deg/s
    /// </summary>
    public const float NOMINAL_ROTATION_SPEED = 60f;

    private void Update()
    {
        if (!SimpleWebXR.InSession) return;

        if (SimpleWebXR.LeftInput.AxesCount < 4 || SimpleWebXR.RightInput.AxesCount < 4) return;

        var axe2 = SimpleWebXR.LeftInput.Axes[2] + SimpleWebXR.RightInput.Axes[2];
        var axe3 = SimpleWebXR.LeftInput.Axes[3] + SimpleWebXR.RightInput.Axes[3];

        if (axe3 != 0)
        {
            MixedRealityPlayspace.Transform.Translate(- Vector3.forward * Time.deltaTime * NOMINAL_TRANSLATION_SPEED * axe3, Camera.main.transform);
        }

        if (axe2 != 0)
        {
            MixedRealityPlayspace.RotateAround(Camera.main.transform.position, Vector3.up, Time.deltaTime * NOMINAL_ROTATION_SPEED * axe2);
        }
    }
}
