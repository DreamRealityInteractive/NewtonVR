using UnityEngine;
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
        public bool CanAttach = true;

        public float PullRange = 0.2f;
        public float AttachRange = 0.01f;
        public float DropDistance = 0.1f;

        public bool MatchRotation = true;
		public bool IsHalo = false;

		protected ScaleState	m_currentScaleState = ScaleState.kIdle;
		protected Vector3		m_objectInitialScale = Vector3.zero;

		protected enum ScaleState
		{
			kIdle,
			kScale
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
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if (IsAttached == true)
            {
                FixedUpdateAttached();
            }
			else if(AttachedItem && !IsAttached && NVRPlayer.Instance.m_haloScaleState == NVRPlayer.HaloScaleState.kObjectScaleUpDown && NVRPlayer.Instance.m_isHaloScaleActive && 
				m_currentScaleState == ScaleState.kScale)
			{
				// Scale down the object
				ScaleAttachedObject(AttachedItem.transform.localScale, m_objectInitialScale * AttachedItem.m_afterHaloObjectScale);
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

				// Scale Up the object
				if(AttachedItem && NVRPlayer.Instance.m_isHaloScaleActive && m_currentScaleState == ScaleState.kScale)
				{
					ScaleAttachedObject(AttachedItem.transform.localScale, m_objectInitialScale * AttachedItem.m_haloObjectScale);
				}
            }
        }

        protected virtual void Attach(NVRAttachPoint point)
        {
            point.Attached(this);

            AttachedItem = point.Item;
            AttachedPoint = point;

			if(IsHalo)
			{
				m_currentScaleState = ScaleState.kScale;
				m_objectInitialScale = AttachedPoint.InitialScale;
			}
        }

        protected virtual void Detach()
        {
            AttachedPoint.Detached(this);
            AttachedItem = null;
            AttachedPoint = null;

			if (IsHalo)
			{
				m_currentScaleState = ScaleState.kScale;
			}
        }

		private void ScaleAttachedObject(Vector3 startScale, Vector3 endScale)
		{
			Vector3 currentScale = Vector3.Lerp (startScale, endScale, Time.deltaTime / NVRPlayer.Instance.m_haloObjectScaleDuration);

			if(AttachedItem)
			{
				AttachedItem.transform.localScale = currentScale;
			}

			if(Mathf.Approximately (currentScale.x, endScale.x))
			{
				m_currentScaleState = ScaleState.kIdle;
			}
		}
    }
}
