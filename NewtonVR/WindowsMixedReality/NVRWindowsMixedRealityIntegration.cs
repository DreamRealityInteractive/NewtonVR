using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#if NVR_WMR
namespace NewtonVR
{
    public class NVRWindowsMixedRealityIntegration : NVRIntegration
    {

        public override void Initialize(NVRPlayer player)
        {
            Player = player;
            Camera cam = player.Head.GetComponent<Camera>();
            cam.cullingMask = -1;
            cam.farClipPlane = 160f;
            cam.nearClipPlane = 0.01f;
            cam.tag = "MainCamera";
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
