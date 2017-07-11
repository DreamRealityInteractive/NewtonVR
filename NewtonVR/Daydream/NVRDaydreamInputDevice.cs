using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

#if UNITY_HAS_GOOGLEVR
namespace NewtonVR
{
    public class NVRDaydreamInputDevice : NVRInputDevice
    {
        private GameObject RenderModel;

		private GvrController Controller;

        public override void Initialize(NVRHand hand)
        {
            base.Initialize(hand);

            SetupButtonMapping(); 
        }

        protected virtual void SetupButtonMapping()
		{
        }

        public override float GetAxis1D(NVRButtons button)
        {
                return 0.0f;
        }

        public override Vector2 GetAxis2D(NVRButtons button)
        {
				return (button == NVRButtons.Touchpad) ? GvrController.TouchPos : Vector2.zero;
        }

        public override bool GetPressDown(NVRButtons button)
        {
			switch(button)
			{
			case NVRButtons.ApplicationMenu:
				return GvrController.AppButtonDown;
			case NVRButtons.System:
				return GvrController.HomeButtonDown;
			case NVRButtons.Touchpad:
				return GvrController.ClickButtonDown;
			}

			return false; // OVRInput.GetDown(GetButtonMap(button), Controller);
        }

        public override bool GetPressUp(NVRButtons button)
        {
			switch(button)
			{
			case NVRButtons.ApplicationMenu:
				return GvrController.AppButtonUp;
			case NVRButtons.Touchpad:
				return GvrController.ClickButtonUp;
			}
				
			return false; //OVRInput.GetUp(GetButtonMap(button), Controller);
        }

        public override bool GetPress(NVRButtons button)
        {
			switch(button)
			{
			case NVRButtons.ApplicationMenu:
				return GvrController.AppButton;
			case NVRButtons.Touchpad:
				return GvrController.ClickButton;
			}
				
			return false; // OVRInput.Get(GetButtonMap(button), Controller);
        }

		public override bool GetTouchDown(NVRButtons button)
        {
			switch(button)
			{
			case NVRButtons.Touchpad:
				return GvrController.TouchDown;
			}

			return false;
        }

		public override bool GetTouchUp(NVRButtons button)
        {
			switch(button)
			{
			case NVRButtons.Touchpad:
				return GvrController.TouchUp;
			}

			return false;
        }

		public override bool GetTouch(NVRButtons button)
        {
				return (button == NVRButtons.Touchpad) ? GvrController.IsTouching : false;
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
            return "DaydreamTouchPad";
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

			SphereCollider collider = RenderModel.AddComponent<SphereCollider>();
			collider.isTrigger = true;
			collider.radius = 0.15f;

			Colliders = new Collider[] { collider };

			return Colliders;
        }
        
    }
}
#else
namespace NewtonVR
{
	public class NVRDaydreamInputDevice : NVRInputDevice
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

