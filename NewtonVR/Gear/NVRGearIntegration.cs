    using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.VR;

#if NVR_Oculus
namespace NewtonVR
{
    public class NVRGearIntegration : NVRIntegration
    {
        private OVRBoundary boundary;
        private OVRBoundary Boundary
        {
            get
            {
                if (boundary == null)
                {
                    boundary = new OVRBoundary();
                }
                return boundary;
            }
        }

        private OVRDisplay display;
        private OVRDisplay Display
        {
            get
            {
                if (display == null)
                {
                    display = new OVRDisplay();
                }
                return display;
            }
        }

        private OVRTracker tracker;
        private OVRTracker Tracker
        {
            get
            {
                if (tracker == null)
                {
                    tracker = new OVRTracker();
                }
                return tracker;
            }
        }

        private static bool isOculusGo;
        private static bool IsOculusGo
        {
            get { return isOculusGo; }
        }


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
	public class NVRGearIntegration : NVRIntegration
    {
        private static bool isOculusGo;
        public static bool IsOculusGo
        {
            get { return isOculusGo; }
        }

        public override void Initialize(NVRPlayer player)
        {
            //// Start Integration for Stereo with NVR //////
            NVRPlayer Player = player;
            Debug.Log("init gear");

            // Integration for two camera setup needed for Microsoft Holo video with NewtonVr
            Player.Head.gameObject.name = "Head_Left";      // rename original NVR head to avoid confusion in editor

            // Create game object and position it to match NVR existing camera
            GameObject headRight = new GameObject("Head_Right");
          //  headRight.gameObject.tag = "MainCamera";
            headRight.transform.position = Player.Head.gameObject.transform.position;
            headRight.transform.rotation = Player.Head.transform.rotation;
            headRight.transform.parent = Player.Head.transform.parent;

            // Add camera to new game object and set for right VR eye
            Camera camRight = headRight.AddComponent<Camera>();
            camRight.stereoTargetEye = StereoTargetEyeMask.Right;
            camRight.cullingMask = Player.Head.GetComponent<Camera>().cullingMask;
            camRight.cullingMask &= ~(1 << 8);
            camRight.farClipPlane = 160f;
            camRight.nearClipPlane = 0.01f;
            camRight.allowHDR = true;
            camRight.depth = -1;

            // If original NVR head has a camera component, set the camera for left VR eye
            Camera camLeft = Player.Head.GetComponent<Camera>();
            if (camLeft)
            {
                camLeft.stereoTargetEye = StereoTargetEyeMask.Left;
                // Set all layers to 1, then compare with & to change "RightEye" layer to 0 while keeping all others 1
                // 1111 add 1101 get 1101
                camLeft.cullingMask &= ~(1 << 9);
                camLeft.farClipPlane = 160f;
                camLeft.nearClipPlane = 0.01f;
                camLeft.allowHDR = true;
            }

            // Copy over image effect components from the main camera tagged with "ArtSetupCamera"
            // to each target eye camera with reflected component values
            Component[] cameraComponents = Player.Head.GetComponents(typeof(Component));

            // Copy over each component from the head to the right eye
            foreach (Component curComponent in cameraComponents)
            {
                // Get component type
                System.Type type = curComponent.GetType();

                // Skip certain components ie left eye already has Camera 
                // component and scene only needs one AudioListener
                if (type != typeof(Camera) && type != typeof(Transform) &&
                    type != typeof(AudioListener))// && type != typeof(VideoPlayTest))
                {
                    // Add component to right eye game object
                    Component copy = headRight.AddComponent(type);
                    // Save active status of component from head
                    bool isActive = ((Behaviour)Player.Head.GetComponent(type)).enabled;

                    // Reflect all component values from head to right eye
                    System.Reflection.FieldInfo[] fields = type.GetFields();
                    foreach (System.Reflection.FieldInfo field in fields)
                    {
                        field.SetValue(copy, field.GetValue(curComponent));
                    }

                    // Set active status of right eye component from status of head
                    ((Behaviour)headRight.GetComponent(type)).enabled = isActive;
                }
            }

            isOculusGo = SystemInfo.deviceModel == "Oculus Pacific";

            Debug.Log("Is Oculus go " + isOculusGo + " Device model " + SystemInfo.deviceModel);

            if (isOculusGo)
            {
                UnityEngine.VR.VRSettings.renderScale = 1.2f;
            }

            //// End Integration for Stereo with NVR //////
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