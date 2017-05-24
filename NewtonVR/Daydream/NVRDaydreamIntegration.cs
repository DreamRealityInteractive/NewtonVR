using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.VR;

#if UNITY_HAS_GOOGLEVR
namespace NewtonVR
{
    public class NVRDaydreamIntegration : NVRIntegration
    {
        public override void Initialize(NVRPlayer player)
        {
            Player = player;
            Player.gameObject.SetActive(true);
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
				if (VRSettings.enabled == false) 
				{
					VRSettings.enabled = true;
				}
            }
	
			return true; // OVRPlugin.hmdPresent;
        }
    }
}
#else
namespace NewtonVR
{
	public class NVRDaydreamIntegration : NVRIntegration
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