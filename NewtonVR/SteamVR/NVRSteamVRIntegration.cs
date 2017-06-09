using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

#if NVR_SteamVR
using Valve.VR;

namespace NewtonVR
{
    public class NVRSteamVRIntegration : NVRIntegration
    {
        public override void Initialize(NVRPlayer player)
        {
            Player = player;

#if UNITY_5_6_OR_NEWER
            Player.Head.gameObject.AddComponent<SteamVR_UpdatePoses>();
#endif

            Player.gameObject.SetActive(false);


            SteamVR_ControllerManager controllerManager = Player.gameObject.AddComponent<SteamVR_ControllerManager>();
            controllerManager.left = Player.LeftHand.gameObject;
            controllerManager.right = Player.RightHand.gameObject;

            //Player.gameObject.AddComponent<SteamVR_PlayArea>();

            for (int index = 0; index < Player.Hands.Length; index++)
            {
                Player.Hands[index].gameObject.AddComponent<SteamVR_TrackedObject>();
            }


            SteamVR_Camera steamVrCamera = Player.Head.gameObject.AddComponent<SteamVR_Camera>();
            Player.Head.gameObject.AddComponent<SteamVR_Ears>();
            NVRHelpers.SetField(steamVrCamera, "_head", Player.Head.transform, false);
            NVRHelpers.SetField(steamVrCamera, "_ears", Player.Head.transform, false);
            
            //// Start Integration for Holo with NVR //////

            // Integration for two camera setup needed for Microsoft Holo video with NewtonVr
            Player.Head.gameObject.name = "Head_Left";		// rename original NVR head to avoid confusion in editor

            // Create game object and position it to match NVR existing camera
            GameObject headRight = new GameObject ("Head_Right");
            headRight.transform.position = Player.Head.gameObject.transform.position;
            headRight.transform.rotation = Player.Head.transform.rotation;
            headRight.transform.parent = Player.Head.transform.parent;

            // Add camera to new game object and set for right VR eye
            Camera camRight = headRight.AddComponent<Camera> ();
            camRight.stereoTargetEye = StereoTargetEyeMask.Right;

            // Add steam camera component
            SteamVR_Camera steamVrCameraRight = headRight.gameObject.AddComponent<SteamVR_Camera>();

            // If original NVR head has a camera component, set the camera for left VR eye
            Camera camLeft = Player.Head.GetComponent<Camera> ();
            if (camLeft) {
	            camLeft.stereoTargetEye = StereoTargetEyeMask.Left;
            }

			//// End Integration for Holo with NVR //////
            Player.Head.gameObject.AddComponent<SteamVR_TrackedObject>();

            Player.gameObject.SetActive(true);

            SteamVR_Render[] steamvr_objects = GameObject.FindObjectsOfType<SteamVR_Render>();
            for (int objectIndex = 0; objectIndex < steamvr_objects.Length; objectIndex++)
            {
                steamvr_objects[objectIndex].lockPhysicsUpdateRateToRenderFrequency = false; //this generally seems to break things :) Just make sure your Time -> Physics Timestep is set to 0.011
            }
        }

        private Vector3 PlayspaceBounds = Vector3.zero;
        public override Vector3 GetPlayspaceBounds()
        {
            bool initOpenVR = (!SteamVR.active && !SteamVR.usingNativeSupport);
            if (initOpenVR)
            {
                EVRInitError error = EVRInitError.None;
                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);
            }

            CVRChaperone chaperone = OpenVR.Chaperone;
            if (chaperone != null)
            {
                chaperone.GetPlayAreaSize(ref PlayspaceBounds.x, ref PlayspaceBounds.z);
                PlayspaceBounds.y = 1;
            }

            if (initOpenVR)
                OpenVR.Shutdown();

            return PlayspaceBounds;
        }

        public override bool IsHmdPresent()
        {
            bool initOpenVR = (!SteamVR.active && !SteamVR.usingNativeSupport);
            if (initOpenVR)
            {
                EVRInitError error = EVRInitError.None;
                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Other);

                if (error != EVRInitError.None)
                {
                    return false;
                }
            }

            return OpenVR.IsHmdPresent();
        }
    }
}
#else
namespace NewtonVR
{
    public class NVRSteamVRIntegration : NVRIntegration
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