using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System;

namespace NewtonVR
{
    public class NVRPhysicalController : MonoBehaviour
    {
        private NVRHand Hand;
        public bool State = false;
        private Rigidbody Rigidbody;

        [HideInInspector]
        public GameObject PhysicalController;
        private Collider[] Colliders;

        protected float DropDistance { get { return 1f; } }
        protected Vector3 ClosestHeldPoint;

        protected float AttachedRotationMagic = 20f;
        protected float AttachedPositionMagic = 3000f;

        private Type[] KeepTypes = new Type[] {typeof(MeshFilter), typeof(Renderer), typeof(Transform), typeof(Rigidbody),
            typeof(Animator), typeof(HTW.HTWHandController)};

        // Custom hand mesh and grabbing animation
        private HTW.HTWHandController HandPhysicsAnimController;
        private HTW.HTWHandController HandPhysicsColliderAnimController;
        private HTW.HTWHandController HandAnimController;
        private SkinnedMeshRenderer[] PhysicalControllerRenderers;
        private SkinnedMeshRenderer HandMeshRenderer;
        private Animator[] anims = {null, null, null};
        private GameObject CustomColliderRoot;
        private HTW.HTWChildCollision[] CustomChildrenColliders = { null, null, null, null, null};

        public void Initialize(NVRHand trackingHand, bool initialState)
        {
            Hand = trackingHand;

            Hand.gameObject.SetActive(false);

            PhysicalController = GameObject.Instantiate(Hand.gameObject);
            PhysicalController.name = PhysicalController.name.Replace("(Clone)", " [Physical]");

            Hand.gameObject.SetActive(true);

            Component[] components = PhysicalController.GetComponentsInChildren<Component>(true);

            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Type componentType = components[componentIndex].GetType();
                if (KeepTypes.Any(keepType => keepType == componentType || componentType.IsSubclassOf(keepType)) == false)
                {
                    DestroyImmediate(components[componentIndex]);
                }
            }

            // Get references from script to toggle animation states, visibility, and physics
            HandPhysicsAnimController = PhysicalController.GetComponentInChildren<HTW.HTWHandController>();
            HandAnimController = Hand.GetComponentInChildren<HTW.HTWHandController>();
            PhysicalControllerRenderers = PhysicalController.GetComponentsInChildren<SkinnedMeshRenderer>();
            HandMeshRenderer = Hand.GetComponentInChildren<SkinnedMeshRenderer>();
            anims[0] = PhysicalController.GetComponentInChildren<Animator>();
            anims[1] = Hand.GetComponentInChildren<Animator>();


            PhysicalController.transform.parent = Hand.transform.parent;
            PhysicalController.transform.position = Hand.transform.position;
            PhysicalController.transform.rotation = Hand.transform.rotation;
            PhysicalController.transform.localScale = Hand.transform.localScale;

            PhysicalController.SetActive(true);

            if (Hand.HasCustomModel)
            {
                SetupCustomModel();
            }
            else
            {
                Colliders = Hand.SetupDefaultPhysicalColliders(PhysicalController.transform);
            }

            if (Colliders == null)
            {
                Debug.LogError("[NewtonVR] Error: Physical colliders on hand not setup properly.");
            }

            Rigidbody = PhysicalController.GetComponent<Rigidbody>();
            Rigidbody.isKinematic = false;
            Rigidbody.maxAngularVelocity = float.MaxValue;

            if (trackingHand.Player.AutomaticallySetControllerTransparency)
            {
                Renderer[] renderers = PhysicalController.GetComponentsInChildren<Renderer>();
                for (int index = 0; index < renderers.Length; index++)
                {
                    NVRHelpers.SetOpaque(renderers[index].material);
                }
            }

            if (initialState == false)
            {
                Off();
            }
            else
            {
                On();
            }
        }

        public void Kill()
        {
            Destroy(PhysicalController);
            Destroy(this);
        }

        private bool CheckForDrop()
        {
            float distance = Vector3.Distance(Hand.transform.position, this.transform.position);

            if (distance > DropDistance)
            {
                DroppedBecauseOfDistance();
                return true;
            }

            return false;
        }

        private void UpdatePosition()
        {
            Rigidbody.maxAngularVelocity = float.MaxValue; //this doesn't seem to be respected in nvrhand's init. or physical hand's init. not sure why. if anybody knows, let us know. -Keith 6/16/2016

            Quaternion rotationDelta;
            Vector3 positionDelta;

            float angle;
            Vector3 axis;

            rotationDelta = Hand.transform.rotation * Quaternion.Inverse(PhysicalController.transform.rotation);
            positionDelta = (Hand.transform.position - PhysicalController.transform.position);

            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0)
            {
                Vector3 angularTarget = angle * axis;
                this.Rigidbody.angularVelocity = angularTarget;
            }

            Vector3 velocityTarget = positionDelta / Time.deltaTime;
            this.Rigidbody.velocity = velocityTarget;
        }

        protected virtual void FixedUpdate()
        {
            if (State == true)
            {
                bool dropped = CheckForDrop();

                if (dropped == false)
                {
                    UpdatePosition();
                }
            }
        }

        protected virtual void DroppedBecauseOfDistance()
        {
            Hand.ForceGhost();
        }

        public void On()
        {
            PhysicalController.transform.position = Hand.transform.position;
            PhysicalController.transform.rotation = Hand.transform.rotation;

            State = true;

            // If a custom hand was assigned in Editor
            if (HandPhysicsAnimController)
            {
                // Turn on visibility of custom physics hand
                for (int index = 0; index < PhysicalControllerRenderers.Length; index++)
                {
                    PhysicalControllerRenderers[index].enabled = true;
                }
                // Turn on grabbing animation
                HandPhysicsAnimController.setIsGrabbing(true);
                HandAnimController.setIsGrabbing(true);
                HandMeshRenderer.enabled = false;
                HandPhysicsColliderAnimController.setIsGrabbing(true);
                // Turn on hand physics collisions
                toggleTriggerCollider(false);

            }
            else
            {
                PhysicalController.SetActive(true);
             //   Debug.Log("Trigger ON, no PhysicsAnimControll");
            }
        }

        public void Off()
        {

            State = false;

            // If a custom hand was assigned in Editor
            if (HandPhysicsAnimController)
            {
                // Turn off visibility of custom physics hand
                for (int index = 0; index < PhysicalControllerRenderers.Length; index++)
                {
                    PhysicalControllerRenderers[index].enabled = false;
                }

                // Turn off grabbing animaiton, return to idle
           //     Debug.Log("Trigger OFF");
                HandPhysicsAnimController.setIsGrabbing(false);
                HandAnimController.setIsGrabbing(false);
                HandMeshRenderer.enabled = true;
                HandPhysicsColliderAnimController.setIsGrabbing(false);
                // Turn off hand physics collisions
                toggleTriggerCollider(true);
                // Set animation speed to 1 for each finger layer
                foreach (HTW.HTWChildCollision col in CustomChildrenColliders)
                {
                    col.unfreezeHand();
                }
            }
            else
            {
                PhysicalController.SetActive(false);
            //    Debug.Log("Trigger OFF, no PhysicsAnimControll");
            }
        }

        protected void SetupCustomModel()
        {
            Transform customCollidersTransform = null;
            if (Hand.CustomPhysicalColliders == null)
            {
                GameObject customColliders = GameObject.Instantiate(Hand.CustomModel);
                customColliders.name = "CustomColliders";
                customCollidersTransform = customColliders.transform;

                customCollidersTransform.parent = PhysicalController.transform;
                customCollidersTransform.localPosition = Vector3.zero;
                customCollidersTransform.localRotation = Quaternion.identity;
                customCollidersTransform.localScale = Vector3.one;

                foreach (Collider col in customColliders.GetComponentsInChildren<Collider>())
                {
                    col.isTrigger = false;
                }

                Colliders = customCollidersTransform.GetComponentsInChildren<Collider>();
            }
            else
            {
                GameObject customColliders = GameObject.Instantiate(Hand.CustomPhysicalColliders);
                customColliders.name = "CustomColliders";
                customCollidersTransform = customColliders.transform;

                // Reference for animating the colliders as mesh bones animate
                HandPhysicsColliderAnimController = customColliders.GetComponentInChildren<HTW.HTWHandController>();
                anims[2] = customColliders.GetComponentInChildren<Animator>();      // assign 3rd of 3 animators associated with respective hand
                // pass animators to all children so they only animate corresponding hand
                CustomChildrenColliders = customColliders.GetComponentsInChildren<HTW.HTWChildCollision>();
                foreach (HTW.HTWChildCollision col in CustomChildrenColliders)
                {
                    // NO LONGER USING PHYSICAL HANDS OR MULTIPLE ANIMATORS -- TODO: REMOVE ALL REFERENCES TO ANIMATORS IN NVRPHYSICALCONTROLLER
                   // col.setAnimators(anims);
                }
                // Reference for toggling customColliders on/off to avoid physics collision when hand is not grabbing
                CustomColliderRoot = customColliders;
                toggleTriggerCollider(true);

                customCollidersTransform.parent = PhysicalController.transform;
                customCollidersTransform.localPosition = Vector3.zero;
                customCollidersTransform.localRotation = Quaternion.identity;
                customCollidersTransform.localScale = Hand.CustomPhysicalColliders.transform.localScale;
            }

            Colliders = customCollidersTransform.GetComponentsInChildren<Collider>();
        }

        public bool hasCustomPhysicalHandController()
        {
            return HandPhysicsAnimController;
        }

        // Toggles the isTrigger value of custom colliders
        // input 'true' parameter with hand is not grabbing so custom collider is not effecting physics collisions
        // input 'false' to enable physics collisions while the hand is grabbing
        public void toggleTriggerCollider(bool isTrigger)
        {
            foreach(Collider col in CustomColliderRoot.GetComponentsInChildren<Collider>())
            {
                col.isTrigger = isTrigger;
            }
        }

    }
}
