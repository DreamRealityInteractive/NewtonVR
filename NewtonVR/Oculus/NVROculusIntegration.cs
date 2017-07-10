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
    public class NVROculusIntegration : NVRIntegration
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


        public override void Initialize(NVRPlayer player)
        {
            Player = player;
            Player.gameObject.SetActive(false);

            OVRManager manager = Player.gameObject.AddComponent<OVRManager>();
            manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;

            // User OVR rig for two camera, per eye setup
            OVRCameraRig rig = Player.gameObject.AddComponent<OVRCameraRig>();
            rig.usePerEyeCameras = true;

            NVRHelpers.SetProperty(rig, "trackingSpace", Player.transform, true);
            NVRHelpers.SetProperty(rig, "leftHandAnchor", Player.LeftHand.transform, true);
            NVRHelpers.SetProperty(rig, "rightHandAnchor", Player.RightHand.transform, true);
            NVRHelpers.SetProperty(rig, "centerEyeAnchor", Player.Head.transform, true);

            Player.gameObject.SetActive(true);

            // Initialize Left and Right eye cameras with components and values matching original head camera
            // This allows image effects, scripts, etc. set up and tested on the main camera to copy
            // over maintaining values to two camera setup

            // Reference to the NVR Head game object with desired camera components and values
            GameObject originalHead = Player.Head.gameObject;

            // Reference to left and right eye cameras created by OVR rig setup 
            Camera _leftEyeCamera = rig.leftEyeAnchor.GetComponent<Camera>();
            Camera _rightEyeCamera = rig.rightEyeAnchor.GetComponent<Camera>();

            if (originalHead != null)
            {
                ///////////////////// LEFT EYE CAMERA SETUP //////////////////////////////

                // Adjust clipping and target eye for stereo camera
                _leftEyeCamera.stereoTargetEye = StereoTargetEyeMask.Left;
                _leftEyeCamera.farClipPlane = 200f;
                _leftEyeCamera.nearClipPlane = 0.01f;

                // Set all layers to 1, then compare with & to change "RightEye" layer to 0 while keeping all others 1
                // 1111 add 1101 get 1101
                _leftEyeCamera.cullingMask = -1;
                _leftEyeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("RightEye"));

                // Copy over image effect components from the main camera tagged with "ArtSetupCamera"
                // to each target eye camera with reflected component values
                Component[] cameraComponents = originalHead.GetComponents(typeof(Component));

                // Copy over each component from the head to the right eye
                foreach (Component curComponent in cameraComponents)
                {
                    // Get component type
                    System.Type type = curComponent.GetType();

                    // Skip certain components ie left eye already has Camera 
                    // component and scene only needs one AudioListener
                    if (type != typeof(Camera) && type != typeof(Transform) &&
                        type != typeof(AudioListener))
                    {
                        // Add component to right eye game object
                        Component copy = rig.leftEyeAnchor.gameObject.AddComponent(type);
                        // Save active status of component from head
                        bool isActive = ((Behaviour)originalHead.GetComponent(type)).enabled;

                        // Reflect all component values from head to right eye
                        System.Reflection.FieldInfo[] fields = type.GetFields();
                        foreach (System.Reflection.FieldInfo field in fields)
                        {
                            field.SetValue(copy, field.GetValue(curComponent));
                        }

                        // Set active status of right eye component from status of head
                        ((Behaviour) rig.leftEyeAnchor.GetComponent(type)).enabled = isActive;
                    }
                }

                ///////////////////// RIGHT EYE CAMERA SETUP //////////////////////////////

                // Adjust clipping and target eye for stereo camera
                _rightEyeCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                _rightEyeCamera.farClipPlane = 200f;
                _rightEyeCamera.nearClipPlane = 0.01f;

                // Set all layers to 1, then compare with & to change "LeftEye" layer to 0 while keeping all others 1
                // 1111 add 1101 get 1101
                _rightEyeCamera.cullingMask = -1;
                _rightEyeCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("LeftEye"));
                _rightEyeCamera.tag = "MainCamera";

                // Copy over image effect components from the main camera tagged with "ArtSetupCamera"
                // to each target eye camera with reflected component values
               // Component[] cameraComponents = originalHead.GetComponents(typeof(Component));

                // Copy over each component from the head to the right eye
                foreach (Component curComponent in cameraComponents)
                {
                    // Get component type
                    System.Type type = curComponent.GetType();

                    // Skip certain components ie right eye already has Camera 
                    // component and scene only needs one AudioListener
                    if (type != typeof(Camera) && type != typeof(Transform) &&
                        type != typeof(AudioListener))
                    {
                        // Add component to right eye game object
                        Component copy = rig.rightEyeAnchor.gameObject.AddComponent(type);
                        // Save active status of component from head
                        bool isActive = ((Behaviour)originalHead.GetComponent(type)).enabled;

                        // Reflect all component values from head to right eye
                        System.Reflection.FieldInfo[] fields = type.GetFields();
                        foreach (System.Reflection.FieldInfo field in fields)
                        {
                            field.SetValue(copy, field.GetValue(curComponent));
                        }

                        // Set active status of right eye component from status of head
                        ((Behaviour) rig.rightEyeAnchor.GetComponent(type)).enabled = isActive;
                    }
                }
            }
        }

        private Vector3 PlayspaceBounds = Vector3.zero;
        public override Vector3 GetPlayspaceBounds()
        {
            bool configured = Boundary.GetConfigured();
            if (configured == true)
            {
                PlayspaceBounds = Boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary);
            }

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

                if (Display == null)
                {
                    return false;
                }

                if (Tracker == null)
                {
                    return false;
                }
            }

            return OVRPlugin.hmdPresent;
        }
    }
}
#else
namespace NewtonVR
{
    public class NVROculusIntegration : NVRIntegration
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