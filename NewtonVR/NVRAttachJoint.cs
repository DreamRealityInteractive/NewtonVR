﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;

namespace NewtonVR
{
    public class NVRAttachJoint : MonoBehaviour
    {
        public NVRInteractableItem AttachedItem;
        public NVRAttachPoint AttachedPoint;

        public bool IsAttached { get { return AttachedItem != null; } }
        private bool CanAttach = true;

        public float PullRange = 0.2f;
        public float AttachRange = 0.01f;
        public float DropDistance = 0.1f;

        public bool MatchRotation = true;
		public bool IsTryingToAttach = false;
		public NVRInteractableItem TryingToAttachItem;

        public bool JointCanAttach {
            get { return CanAttach; }
            set { CanAttach = value; }
        }

		public void ForceDetach()
		{
			Detach();
		}

        protected virtual void OnTriggerStay(Collider col)
        {
            if (IsAttached == false && CanAttach == true)
            {
                NVRAttachPoint point = AttachPointMapper.GetAttachPoint(col);
                if (point != null && point.IsAttached == false)
                {
                    float distance = Vector3.Distance(point.transform.position, this.transform.position);

                    if (distance < AttachRange)
                    {
                        Attach(point);
                    }
                    else
                    {
                        point.PullTowards(this);
                    }

					IsTryingToAttach = true;
					TryingToAttachItem = col.gameObject.GetComponentInParent<NVRInteractableItem> ();
                }
            }
        }

		protected virtual void OnTriggerExit(Collider col)
		{
			IsTryingToAttach = false;
			TryingToAttachItem = null;
		}

        protected virtual void FixedUpdate()
        {
            if (IsAttached == true)
            {
                FixedUpdateAttached();
            }
        }

        protected virtual void FixedUpdateAttached()
        {
            float distance = Vector3.Distance(AttachedPoint.transform.position, this.transform.position);

            if (distance > DropDistance)
            {
                Detach();
            }
            else
            {
                AttachedPoint.PullTowards(this);
            }
        }

        protected virtual void Attach(NVRAttachPoint point)
        {
            point.Attached(this);

            AttachedItem = point.Item;
            AttachedPoint = point;
        }

        protected virtual void Detach()
        {
			if(AttachedPoint)
			{
            	AttachedPoint.Detached(this);
			}

            AttachedItem = null;
            AttachedPoint = null;
			IsTryingToAttach = false;
			TryingToAttachItem = null;
        }
	
    }
}
