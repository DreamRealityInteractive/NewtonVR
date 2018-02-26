using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using HoloToolkit.Unity;

#if NVR_WMR
namespace NewtonVR
{
    public class NVRWindowsMixedRealityIntegration : NVRIntegration
    {

        public override void Initialize(NVRPlayer player)
        {
            Player = player;
            Camera leftCam = player.Head.GetComponent<Camera>();

            leftCam.cullingMask = -1;
            leftCam.farClipPlane = 160f;
            leftCam.nearClipPlane = 0.01f;
            leftCam.stereoTargetEye = StereoTargetEyeMask.Left;

            leftCam.gameObject.AddComponent<GpuTimingCamera>();

            HoloToolkit.Unity.AdaptiveQuality qualityController = player.gameObject.AddComponent<HoloToolkit.Unity.AdaptiveQuality>();
            HoloToolkit.Unity.AdaptiveViewport viewport = player.gameObject.AddComponent<HoloToolkit.Unity.AdaptiveViewport>();

            GameObject rightGo = new GameObject("StereoR");
            Camera rightCam = rightGo.AddComponent<Camera>();
            rightCam.cullingMask = -1;
            rightCam.farClipPlane = 160f;
            rightCam.nearClipPlane = 0.01f;
            rightCam.stereoTargetEye = StereoTargetEyeMask.Right;
            rightGo.transform.parent = player.Head.transform.parent;
            rightCam.gameObject.AddComponent<GpuTimingCamera>();
        }

        private Vector3 PlayspaceBounds = Vector3.zero;
        public override Vector3 GetPlayspaceBounds()
        {
            return PlayspaceBounds;
        }

        public override bool IsHmdPresent()
        {
            if (Application.isPlaying == false) //try and enable vr if we're in the editor so we can get hmd present
            {
                if (UnityEngine.XR.XRSettings.enabled == false)
                {
                    UnityEngine.XR.XRSettings.enabled = true;
                }
            }

            return XRDevice.isPresent;
        }
    }
}
#else
namespace NewtonVR
{
    public class NVRWindowsMixedRealityIntegration : NVRIntegration
    {
        public override void Initialize(NVRPlayer player)
        {
        }

        public override Vector3 GetPlayspaceBounds()
        {
            return Vector3.zero;
        }

        public override bool IsHmdPresent()
        {
            return false;
        }
    }
}
#endif
