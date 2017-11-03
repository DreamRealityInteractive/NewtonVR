using UnityEngine;
using System.Collections;

namespace NewtonVR
{
    public abstract class NVRCollisionSoundProvider : MonoBehaviour
    {
        public abstract void Awake();
        public abstract void Play(string material, Vector3 position, float impactVolume);
    }
}