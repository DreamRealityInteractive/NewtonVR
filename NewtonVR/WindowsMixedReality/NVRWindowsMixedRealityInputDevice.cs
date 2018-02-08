using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;
using UnityEngine;
using HoloToolkit.Unity;

#if NVR_WMR
namespace NewtonVR
{
    
    public class NVRWindowsMixedRealityInputDevice : NVRInputDevice
    {
        private struct ButtonState
        {
            public float previousState;
            public float currentState;

            public ButtonState(float previous, float current)
            {
                previousState = previous;
                currentState = current;
            }
        }

        private GameObject RenderModel;

        private string controllerSourceKey;

        private XRNode Controller;

        private bool IsLeftHand;

        private Dictionary<NVRButtons, ButtonState> ButtonStates = new Dictionary<NVRButtons, ButtonState>();

        private Vector3 lastTrackedPos;

        private Quaternion lastTrackedRot;

        private const float sensitivity = 0.1f;

        public override void Initialize(NVRHand hand)
        {
            base.Initialize(hand);

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

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
            foreach (var sourceState in InteractionManager.GetCurrentReading())
            {
                if (sourceState.source.kind == InteractionSourceKind.Controller && IsHandednessCorrect(sourceState.source.handedness))
                {
                    StartTrackingController(sourceState.source);
                }
            }

            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
#endif
            SetupButtonMapping();
        }

        private bool IsHandednessCorrect(InteractionSourceHandedness handedness)
        {
            if((IsLeftHand && handedness == InteractionSourceHandedness.Left) || (!IsLeftHand && handedness == InteractionSourceHandedness.Right))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            if (obj.state.source.kind == InteractionSourceKind.Controller && IsHandednessCorrect(obj.state.source.handedness))
            {
                StartTrackingController(obj.state.source);
            }
        }

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs obj)
        {
            InteractionSource source = obj.state.source;
            if (source.kind == InteractionSourceKind.Controller && IsHandednessCorrect(source.handedness) && controllerSourceKey == GenerateKey(source))
            {
                Debug.Log("ConnectionLost");
                ButtonStates[NVRButtons.Touchpad] = new ButtonState ( 0, 0 );
                ButtonStates[NVRButtons.Stick]= new ButtonState(0, 0);
                ButtonStates[NVRButtons.Trigger] = new ButtonState(0, 0);
                ButtonStates[NVRButtons.Grip] = new ButtonState(0, 0);
                ButtonStates[NVRButtons.System] = new ButtonState(0, 0);
                ButtonStates[NVRButtons.ApplicationMenu] = new ButtonState(0, 0);
                ButtonStates[NVRButtons.Y] = new ButtonState(0, 0);
                ButtonStates[NVRButtons.B] = new ButtonState(0, 0);
                controllerSourceKey = null;
            }
        }

        private string GenerateKey(InteractionSource source)
        {
            return source.vendorId + "/" + source.productId + "/" + source.productVersion + "/" + source.handedness;
        }

        private void StartTrackingController(InteractionSource source)
        {
            string key = GenerateKey(source);

            if (source.kind == InteractionSourceKind.Controller && IsHandednessCorrect(source.handedness))
            {
                controllerSourceKey = GenerateKey(source);
            }
        }

        private void UpdateButtonState(NVRButtons button, float value)
        {
            ButtonState buttonState = ButtonStates[button];
            buttonState.previousState = ButtonStates[button].currentState;
            buttonState.currentState = value;
            ButtonStates[button] = buttonState;
        }

        private void UpdateControllerState()
        {
            if(controllerSourceKey == null)
            {
                return;
            }
            foreach (var sourceState in InteractionManager.GetCurrentReading())
            {
                if (sourceState.source.kind == InteractionSourceKind.Controller && IsHandednessCorrect(sourceState.source.handedness) && controllerSourceKey == GenerateKey(sourceState.source))
                {
                    UpdateButtonState(NVRButtons.Trigger, sourceState.selectPressedAmount);

                    if (sourceState.source.supportsGrasp)
                    {
                        UpdateButtonState(NVRButtons.Grip, sourceState.grasped ? 1 : 0);
                    }

                    if (sourceState.source.supportsMenu)
                    {
                        UpdateButtonState(NVRButtons.ApplicationMenu, sourceState.menuPressed ? 1 : 0);
                    }

                    Vector3 newPosition;
                    if (sourceState.sourcePose.TryGetPosition(out newPosition, InteractionSourceNode.Grip) && ValidPosition(newPosition))
                    {
                        lastTrackedPos = newPosition;
                    }

                    Quaternion newRotation;
                    if (sourceState.sourcePose.TryGetRotation(out newRotation, InteractionSourceNode.Grip) && ValidRotation(newRotation))
                    {
                        lastTrackedRot = newRotation;
                    }
                }
            }
        }
#endif

        private bool ValidRotation(Quaternion newRotation)
        {
            return !float.IsNaN(newRotation.x) && !float.IsNaN(newRotation.y) && !float.IsNaN(newRotation.z) && !float.IsNaN(newRotation.w) &&
                !float.IsInfinity(newRotation.x) && !float.IsInfinity(newRotation.y) && !float.IsInfinity(newRotation.z) && !float.IsInfinity(newRotation.w);
        }

        private bool ValidPosition(Vector3 newPosition)
        {
            return !float.IsNaN(newPosition.x) && !float.IsNaN(newPosition.y) && !float.IsNaN(newPosition.z) &&
                !float.IsInfinity(newPosition.x) && !float.IsInfinity(newPosition.y) && !float.IsInfinity(newPosition.z);
        }

        protected virtual void SetupButtonMapping()
        {
            ButtonStates.Add(NVRButtons.Touchpad, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.Stick, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.Trigger, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.Grip, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.System, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.ApplicationMenu, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.Y, new ButtonState(0, 0));
            ButtonStates.Add(NVRButtons.B, new ButtonState(0, 0));
        }

        public override float GetAxis1D(NVRButtons button)
        {
            return ButtonStates[button].currentState;
        }

        public override bool GetPressDown(NVRButtons button)
        {
            return ButtonStates[button].currentState > sensitivity && ButtonStates[button].previousState <= sensitivity;
        }

        public override bool GetPressUp(NVRButtons button)
        {
            return ButtonStates[button].currentState < sensitivity && ButtonStates[button].previousState >= sensitivity;
        }

        public override bool GetPress(NVRButtons button)
        {
            return ButtonStates[button].currentState > sensitivity ? true : false;
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
            foreach (var sourceState in InteractionManager.GetCurrentReading())
            {
                if (sourceState.source.kind == InteractionSourceKind.Controller && IsHandednessCorrect(sourceState.source.handedness) && controllerSourceKey == GenerateKey(sourceState.source))
                {
                    StartCoroutine(DoHapticPulse(sourceState.source, durationMicroSec));
                    
                }
            }
        }

        private IEnumerator DoHapticPulse(InteractionSource interactionSource, ushort durationMicroSec)
        {
            interactionSource.StartHaptics(0.2f);
            float endTime = Time.time + ((float)durationMicroSec / 1000000);
            do
            {
                yield return null;
            } while (Time.time < endTime);
            interactionSource.StopHaptics();
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
            return Vector2.zero;
        }

        private void Update()
        {
            UpdateControllerState();
            this.transform.localPosition = lastTrackedPos;
            this.transform.localRotation = lastTrackedRot;
        }
    }
}
#else
namespace NewtonVR
{
    public class NVRWindowsMixedRealityInputDevice : NVRInputDevice
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

