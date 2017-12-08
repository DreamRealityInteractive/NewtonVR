using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;

namespace NewtonVR
{
    public class NVRAttachPoint : MonoBehaviour
    {
        private const float MaxVelocityChange = 5f;
        private const float MaxAngularVelocityChange = 10f;
        private const float VelocityMagic = 3000f;
        private const float AngularVelocityMagic = 25f;

        [HideInInspector]
        public Rigidbody Rigidbody;

        [HideInInspector]
        public NVRInteractableItem Item;

		[HideInInspector]
		public NVRAttachJoint AttachedJoint;

		public Collider ThisCollider; // Assign a ref if collider doesnt exist in the same game object

        public bool IsAttached;

        protected virtual void Awake()
        {
            IsAttached = false;

            Item = FindNVRItem(this.gameObject);
            if (Item == null)
            {
                Debug.LogError("No NVRInteractableItem found on this object. " + this.gameObject.name, this.gameObject);
            }

			// In case of collider not existing in the same entity, then allow user to assign one for this attach point
			//Collider thisCollider = (ThisCollider != null) ? ThisCollider: this.GetComponent<Collider>();
			//AttachPointMapper.Register(thisCollider, this);
        }

        protected virtual void Start()
        {
            Rigidbody = Item.Rigidbody;

			// In case of collider not existing in the same entity, then allow user to assign one for this attach point
			ThisCollider = (ThisCollider != null) ? ThisCollider: this.GetComponent<Collider>();
			if (null!=ThisCollider)
				AttachPointMapper.Register(ThisCollider, this);
        }

        private NVRInteractableItem FindNVRItem(GameObject gameobject)
        {
            NVRInteractableItem item = gameobject.GetComponent<NVRInteractableItem>();

            if (item != null)
                return item;

            if (gameobject.transform.parent != null)
                return FindNVRItem(gameobject.transform.parent.gameObject);

            return null;
        }

        public virtual void Attached(NVRAttachJoint joint)
        {
#if NVR_Daydream || NVR_Gear
            Rigidbody.isKinematic = true;
            Vector3 pointOffset = this.transform.position - joint.transform.position;
            Item.transform.position = Item.transform.position - pointOffset;
            if (joint.MatchRotation == true)
            {
                Item.transform.rotation = joint.transform.rotation;
            }
#else
            Vector3 targetPosition = joint.transform.position + (Item.transform.position - this.transform.position);
            Rigidbody.MovePosition(targetPosition);
            if (joint.MatchRotation == true)
            {
                Rigidbody.MoveRotation(joint.transform.rotation);
            }

            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero; 
#endif
            IsAttached = true;
            Rigidbody.useGravity = false;

			AttachedJoint = joint;
        }
        public virtual void Detached(NVRAttachJoint joint)
        {
#if NVR_Daydream || NVR_Gear
            Rigidbody.isKinematic = false;
#endif
            IsAttached = false;
            Rigidbody.useGravity = true;
            AttachedJoint = null;
        }

        public virtual void PullTowards(NVRAttachJoint joint)
        {
            Item.SetFrozen(false);
#if NVR_Daydream || NVR_Gear
            Rigidbody.isKinematic = true;
            Vector3 pointOffset = this.transform.position - joint.transform.position;
            Item.transform.position = Item.transform.position - pointOffset;
            if (joint.MatchRotation == true)
            {
                Item.transform.rotation = joint.transform.rotation;
            }

#else
			//Preventing objects being pulled while hands are interacting
			if (true==Item.IsAttached)
				return;
			
            float velocityMagic = VelocityMagic / (Time.deltaTime / NVRPlayer.NewtonVRExpectedDeltaTime);
            float angularVelocityMagic = AngularVelocityMagic / (Time.deltaTime / NVRPlayer.NewtonVRExpectedDeltaTime);

            Vector3 positionDelta = joint.transform.position - this.transform.position;
            Vector3 velocityTarget = (positionDelta * velocityMagic) * Time.deltaTime;

			if (float.IsNaN(velocityTarget.x) == false && Item.Rigidbody)
            {
                velocityTarget = Vector3.MoveTowards(Item.Rigidbody.velocity, velocityTarget, MaxVelocityChange);
                Item.AddExternalVelocity(velocityTarget);
            }


            if (joint.MatchRotation == true)
            {
                Quaternion rotationDelta = joint.transform.rotation * Quaternion.Inverse(Item.transform.rotation);

                float angle;
                Vector3 axis;

                rotationDelta.ToAngleAxis(out angle, out axis);

                if (angle > 180)
                    angle -= 360;

                if (angle != 0)
                {
                    Vector3 angularTarget = angle * axis;
                    if (float.IsNaN(angularTarget.x) == false)
                    {
                        angularTarget = (angularTarget * angularVelocityMagic) * Time.deltaTime;
                        angularTarget = Vector3.MoveTowards(Item.Rigidbody.angularVelocity, angularTarget, MaxAngularVelocityChange);
                        Item.AddExternalAngularVelocity(angularTarget);
                    }
                }
            }
#endif
        }

		void OnDestroy()
		{
			DeregisterCollider();
		}

		public void DeregisterCollider()
		{
			if(ThisCollider)
			{
				AttachPointMapper.Deregister(ThisCollider);
			}
		}

		public void RegisterNewCollider(Collider newCollider)
		{
			if(newCollider)
			{
				ThisCollider = newCollider;
				AttachPointMapper.Register(ThisCollider, this);
			}
		}
    }
}
