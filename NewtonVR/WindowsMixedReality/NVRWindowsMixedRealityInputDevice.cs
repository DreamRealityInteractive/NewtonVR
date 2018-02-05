using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;
using UnityEngine;

#if NVR_WMR
namespace NewtonVR
{
    public class NVRWindowsMixedRealityInputDevice : NVRInputDevice
    {
        private GameObject RenderModel;

        private XRNode Controller;

        private bool IsLeftHand;

        //private OVRInput.Controller Controller;

        private Dictionary<NVRButtons, string> ButtonMapping = new Dictionary<NVRButtons, string>(new NVRButtonsComparer());
        private Dictionary<NVRButtons, string> AxisMapping = new Dictionary<NVRButtons, string>(new NVRButtonsComparer());

        public override void Initialize(NVRHand hand)
        {
            base.Initialize(hand);

            SetupButtonMapping();

            if (hand == Hand.Player.LeftHand)
            {
                IsLeftHand = true;
                Controller = XRNode.LeftHand;
            }
            else
            {
                IsLeftHand = false;
                Controller = XRNode.RightHand;
            }
        }

        protected virtual void SetupButtonMapping()
        {
            ButtonMapping.Add(NVRButtons.Touchpad, IsLeftHand ? "16" : "17");
            ButtonMapping.Add(NVRButtons.Stick, IsLeftHand ? "8" : "9");
            ButtonMapping.Add(NVRButtons.Trigger, IsLeftHand ? "14" : "15");
            ButtonMapping.Add(NVRButtons.Grip, IsLeftHand ? "4" : "5");
            ButtonMapping.Add(NVRButtons.System, IsLeftHand ?  "6": "7");
            ButtonMapping.Add(NVRButtons.ApplicationMenu, IsLeftHand ? "6" : "7");
            ButtonMapping.Add(NVRButtons.Y, IsLeftHand ? "6" : "7");
            ButtonMapping.Add(NVRButtons.B, IsLeftHand ? "6" : "7");
        }

        private string GetButtonMap(NVRButtons button)
        {
            if (ButtonMapping.ContainsKey(button) == false)
            {
                return null;
            }
            return ButtonMapping[button];
        }

        private string GetAxisMap(NVRButtons button)
        {
            if (AxisMapping.ContainsKey(button) == false)
            {
                return null;
            }
            return AxisMapping[button];
        }

        public override float GetAxis1D(NVRButtons button)
        {
            string axisID = GetAxisMap(button);
            if (axisID != null)
            {
                return Input.GetAxis("joystick button " + axisID);
            }
            else
            {
                return 0;
            }
        }

        public override bool GetPressDown(NVRButtons button)
        {
            return Input.GetKeyDown("joystick button " + GetButtonMap(button));
        }

        public override bool GetPressUp(NVRButtons button)
        {
            return Input.GetKeyUp("joystick button " + GetButtonMap(button));
        }

        public override bool GetPress(NVRButtons button)
        {
            return Input.GetKey("joystick button " + GetButtonMap(button));
        }

        public override bool GetTouchDown(NVRButtons button)
        {
            return false;
        }

        public override bool GetTouchUp(NVRButtons button)
        {
            return false;
        }

        public override bool GetTouch(NVRButtons button)
        {
            return false;
        }

        public override bool GetNearTouchDown(NVRButtons button)
        {
            return false;
        }

        public override bool GetNearTouchUp(NVRButtons button)
        {
            return false;
        }

        public override bool GetNearTouch(NVRButtons button)
        {
            return false;
        }

        public override void TriggerHapticPulse(ushort durationMicroSec = 500, NVRButtons button = NVRButtons.Touchpad)
        {
            //StartCoroutine(DoHapticPulse(durationMicroSec));
        }

        public override bool IsCurrentlyTracked
        {
            get
            {
                List<XRNodeState> nodeStates = new List<XRNodeState>();
                InputTracking.GetNodeStates(nodeStates);
                foreach (XRNodeState ns in nodeStates)
                {
                    if(ns.nodeType == Controller)
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public override GameObject SetupDefaultRenderModel()
        {
            if (Hand.IsLeft == true)
            {
                RenderModel = GameObject.Instantiate(Resources.Load<GameObject>("TouchControllers/oculusTouchLeft"));
            }
            else
            {
                RenderModel = GameObject.Instantiate(Resources.Load<GameObject>("TouchControllers/oculusTouchRight"));
            }

            RenderModel.name = "Render Model for " + Hand.gameObject.name;
            RenderModel.transform.parent = Hand.transform;
            RenderModel.transform.localPosition = Vector3.zero;
            RenderModel.transform.localRotation = Quaternion.identity;
            RenderModel.transform.localScale = Vector3.one;

            RenderModel.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            return RenderModel;
        }

        public override bool ReadyToInitialize()
        {
            return true;
        }

        public override string GetDeviceName()
        {
            if (Hand.HasCustomModel == true)
            {
                return "Custom";
            }
            else
            {
                return "WMR Device";
            }
        }

        public override Collider[] SetupDefaultPhysicalColliders(Transform ModelParent)
        {
            Collider[] Colliders = null;

            string name = "oculusTouch";
            if (Hand.IsLeft == true)
            {
                name += "Left";
            }
            else
            {
                name += "Right";
            }
            name += "Colliders";

            Transform touchColliders = ModelParent.transform.Find(name);
            if (touchColliders == null)
            {
                touchColliders = GameObject.Instantiate(Resources.Load<GameObject>("TouchControllers/" + name)).transform;
                touchColliders.parent = ModelParent.transform;
                touchColliders.localPosition = Vector3.zero;
                touchColliders.localRotation = Quaternion.identity;
                touchColliders.localScale = Vector3.one;
            }

            Colliders = touchColliders.GetComponentsInChildren<Collider>();

            return Colliders;
        }

        public override Collider[] SetupDefaultColliders()
        {
            Collider[] Colliders = null;
            
            SphereCollider OculusCollider = RenderModel.AddComponent<SphereCollider>();
            OculusCollider.isTrigger = true;
            OculusCollider.radius = 0.05f;

            Colliders = new Collider[] { OculusCollider };

            return Colliders;
        }

        public override Vector2 GetAxis2D(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            this.transform.localPosition = InputTracking.GetLocalPosition(IsLeftHand?XRNode.LeftHand: XRNode.RightHand);
            this.transform.localRotation = InputTracking.GetLocalRotation(IsLeftHand ? XRNode.LeftHand : XRNode.RightHand);
        }
    }
}
#else
namespace NewtonVR
{
    public class NVROculusInputDevice : NVRInputDevice
    {
        public override bool IsCurrentlyTracked
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override float GetAxis1D(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override Vector2 GetAxis2D(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override string GetDeviceName()
        {
            throw new NotImplementedException();
        }

        public override bool GetNearTouch(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetNearTouchDown(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetNearTouchUp(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetPress(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetPressDown(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetPressUp(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetTouch(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetTouchDown(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool GetTouchUp(NVRButtons button)
        {
            throw new NotImplementedException();
        }

        public override bool ReadyToInitialize()
        {
            throw new NotImplementedException();
        }

        public override Collider[] SetupDefaultColliders()
        {
            throw new NotImplementedException();
        }

        public override Collider[] SetupDefaultPhysicalColliders(Transform ModelParent)
        {
            throw new NotImplementedException();
        }

        public override GameObject SetupDefaultRenderModel()
        {
            throw new NotImplementedException();
        }

        public override void TriggerHapticPulse(ushort durationMicroSec = 500, NVRButtons button = NVRButtons.Touchpad)
        {
            throw new NotImplementedException();
        }
    }
}
#endif

