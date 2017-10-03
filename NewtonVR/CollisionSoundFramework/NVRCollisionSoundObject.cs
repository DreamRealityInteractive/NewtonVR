using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NewtonVR
{
    public class NVRCollisionSoundObject : MonoBehaviour
    {
        private static Dictionary<Collider, NVRCollisionSoundObject> SoundObjects = new Dictionary<Collider, NVRCollisionSoundObject>();

        public NVRCollisionSoundMaterials Material;

		[Tooltip("Sound used when the object is scaled below this threshold")]
		public float m_smallScaleThreshold;
		public NVRCollisionSoundMaterials m_smallScaleMaterial;

		[Tooltip("Sound used when the object is scaled above this threshold")]
		public float m_largeScaleThreshold;
		public NVRCollisionSoundMaterials m_largeScaleMaterial;

        private Collider[] Colliders;

        protected virtual void Awake()
        {
            Colliders = this.GetComponentsInChildren<Collider>(true);

            for (int index = 0; index < Colliders.Length; index++)
            {
                SoundObjects[Colliders[index]] = this;
            }
        }

        protected virtual void OnDestroy()
        {
            Colliders = this.GetComponentsInChildren<Collider>(true);

            for (int index = 0; index < Colliders.Length; index++)
            {
                SoundObjects.Remove(Colliders[index]);
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            Collider collider = collision.collider;
            if (SoundObjects.ContainsKey(collider))
            {
                NVRCollisionSoundObject collisionSoundObject = SoundObjects[collider];

				float thisScale = transform.localScale.x;
				float otherScale = collision.gameObject.transform.localScale.x;

                float volume = CalculateImpactVolume(collision);
                if (volume < NVRCollisionSoundController.Instance.MinCollisionVolume)
                {
                    //Debug.Log("Volume too low to play: " + Volume);
                    return;
                }

				// Play this object's collision sound, depending on scale if so configured
				NVRCollisionSoundMaterials thisMat = Material;

				if (m_smallScaleThreshold != 0.0f && thisScale < m_smallScaleThreshold)
					thisMat = m_smallScaleMaterial;
				else if (m_largeScaleThreshold != 0.0f && thisScale > m_largeScaleThreshold)
					thisMat = m_largeScaleMaterial;

				NVRCollisionSoundController.Play(thisMat, collision.contacts[0].point, volume);

				// Play other object's collision sound
				NVRCollisionSoundMaterials otherMat = collisionSoundObject.Material;

				if (collisionSoundObject.m_smallScaleThreshold != 0.0f && otherScale < collisionSoundObject.m_smallScaleThreshold)
					otherMat = collisionSoundObject.m_smallScaleMaterial;
				else if (collisionSoundObject.m_largeScaleThreshold != 0.0f && otherScale > collisionSoundObject.m_largeScaleThreshold)
					otherMat = collisionSoundObject.m_largeScaleMaterial;
			
				NVRCollisionSoundController.Play(otherMat, collision.contacts[0].point, volume);
            }
        }

        private float CalculateImpactVolume(Collision collision)
        {
            float Volume;
            //Debug.Log("Velocity: " + Collision.relativeVelocity.magnitude.ToString());
            Volume = CubicEaseOut(collision.relativeVelocity.magnitude);
            return Volume;
        }

        /// <summary>
        /// Easing equation function for a cubic (t^3) easing out: 
        /// decelerating from zero velocity.
        /// </summary>
        /// <param name="velocity">Current time in seconds.</param>
        /// <param name="startingValue">Starting value.</param>
        /// <param name="changeInValue">Change in value.</param>
        /// <param name="maxCollisionVelocity">Duration of animation.</param>
        /// <returns>The correct value.</returns>
        public static float CubicEaseOut(float velocity, float startingValue = 0, float changeInValue = 1)
        {
            return changeInValue * ((velocity = velocity / NVRCollisionSoundController.Instance.MaxCollisionVelocity - 1) * velocity * velocity + 1) + startingValue;
        }
    }
}