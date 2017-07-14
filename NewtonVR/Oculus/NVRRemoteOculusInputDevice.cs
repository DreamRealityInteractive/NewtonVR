using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

#if NVR_Oculus
namespace NewtonVR
{
    public class NVRRemoteOculusInputDevice : NVRInputDevice
    {
        private GameObject RenderModel;

        public OVRInput.Controller Controller;

        private Dictionary<NVRButtons, OVRInput.Button> ButtonMapping = new Dictionary<NVRButtons, OVRInput.Button>(new NVRButtonsComparer());
      
        public override void Initialize(NVRHand hand)
        {
            base.Initialize(hand);

            SetupButtonMapping();

            Controller = OVRInput.Controller.Remote;   
        }

        protected virtual void SetupButtonMapping()
        {
            ButtonMapping.Add(NVRButtons.A, OVRInput.Button.One);
            ButtonMapping.Add(NVRButtons.B, OVRInput.Button.Two);
            ButtonMapping.Add(NVRButtons.X, OVRInput.Button.One);
            ButtonMapping.Add(NVRButtons.Y, OVRInput.Button.Two);
            ButtonMapping.Add(NVRButtons.DPad_Up, OVRInput.Button.DpadUp);
            ButtonMapping.Add(NVRButtons.DPad_Down, OVRInput.Button.DpadDown);
            ButtonMapping.Add(NVRButtons.DPad_Left, OVRInput.Button.DpadLeft);
            ButtonMapping.Add(NVRButtons.DPad_Right, OVRInput.Button.DpadRight);
            ButtonMapping.Add(NVRButtons.System, OVRInput.Button.Back);
        }

        private OVRInput.Button GetButtonMap(NVRButtons button)
        {
            if (ButtonMapping.ContainsKey(button) == false)
            {
                //Debug.LogError("No Oculus button configured for: " + button.ToString());
                return OVRInput.Button.None;
            }
            return ButtonMapping[button];
        }

        public override float GetAxis1D(NVRButtons button)
        {
                return 0.0f;
        }

        public override Vector2 GetAxis2D(NVRButtons button)
        {
            return Vector2.zero;
        }

        public override bool GetPressDown(NVRButtons button)
        {
            return OVRInput.GetDown(GetButtonMap(button), Controller);
        }

        public override bool GetPressUp(NVRButtons button)
        {
                return OVRInput.GetUp(GetButtonMap(button), Controller);
        }

        public override bool GetPress(NVRButtons button)
        {
                return OVRInput.Get(GetButtonMap(button), Controller);
        }

        public override bool GetTouchDown(NVRButtons button)
        {
            return OVRInput.GetDown(OVRInput.Button.DpadDown, Controller) || OVRInput.GetDown(OVRInput.Button.DpadUp, Controller)
                || OVRInput.GetDown(OVRInput.Button.DpadLeft, Controller) || OVRInput.GetDown(OVRInput.Button.DpadRight, Controller)
                || OVRInput.GetDown(OVRInput.Button.One, Controller);
        }

        public override bool GetTouchUp(NVRButtons button)
        {
            return OVRInput.GetUp(OVRInput.Button.DpadDown, Controller) || OVRInput.GetUp(OVRInput.Button.DpadUp, Controller)
                || OVRInput.GetUp(OVRInput.Button.DpadLeft, Controller) || OVRInput.GetUp(OVRInput.Button.DpadRight, Controller)
                || OVRInput.GetUp(OVRInput.Button.One, Controller);
        }

        public override bool GetTouch(NVRButtons button)
        {
            return OVRInput.Get(OVRInput.Button.DpadDown, Controller) || OVRInput.Get(OVRInput.Button.DpadUp, Controller)
                || OVRInput.Get(OVRInput.Button.DpadLeft, Controller) || OVRInput.Get(OVRInput.Button.DpadRight, Controller)
                || OVRInput.Get(OVRInput.Button.One, Controller);
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
                StartCoroutine(DoHapticPulse(durationMicroSec));
        }

        private IEnumerator DoHapticPulse(ushort durationMicroSec)
        {
            OVRInput.SetControllerVibration(0.2f, 0.2f, Controller);    //Should we allow setting strength
            float endTime = Time.time + ((float)durationMicroSec / 1000000);
            do
            {
                yield return null;
            } while (Time.time < endTime);
            OVRInput.SetControllerVibration(0, 0, Controller);
        }

        public override bool IsCurrentlyTracked
        {
            get
            {
                return true;
            }
        }


        public override GameObject SetupDefaultRenderModel()
        {
            RenderModel = new GameObject("Render Model for " + Hand.gameObject.name);
            RenderModel.name = "Render Model for " + Hand.gameObject.name;
            RenderModel.transform.parent = Hand.transform;
            RenderModel.transform.localPosition = Vector3.zero;
            RenderModel.transform.localRotation = Quaternion.identity;
            RenderModel.transform.localScale = Vector3.one;

            return RenderModel;
        }

        public override bool ReadyToInitialize()
        {
            return true;
        }

        public override string GetDeviceName()
        {
            return "OculusRemote";
        }

        public override Collider[] SetupDefaultPhysicalColliders(Transform ModelParent)
        {
            Collider[] colliders = null;

            SphereCollider defaultCollider = ModelParent.gameObject.AddComponent<SphereCollider>();
            defaultCollider.isTrigger = true;
            defaultCollider.radius = 0.15f;

            colliders = new Collider[] { defaultCollider };

            return colliders;
        }

        public override Collider[] SetupDefaultColliders()
        {
            Collider[] Colliders = null;

            SphereCollider OculusCollider = RenderModel.AddComponent<SphereCollider>();
            OculusCollider.isTrigger = true;
            OculusCollider.radius = 0.15f;

            Colliders = new Collider[] { OculusCollider };

            return Colliders;
        }
        
    }
}
#else
namespace NewtonVR
{
	public class NVRRemoteOculusInputDevice : NVRInputDevice
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

