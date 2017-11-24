using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NewtonVR
{
    public class NVRCollisionSoundProviderUnity : NVRCollisionSoundProvider
    {
        private static string AudioSourcePrefabPath = "CollisionSoundPrefab";
        private static string CollisionSoundsPath = "CollisionSounds";
        private GameObject AudioSourcePrefab;

        private AudioSource[] AudioPool;
        private int CurrentPoolIndex;

        private Dictionary<string, List<AudioClip>> Clips;

        public override void Awake()
        {
            AudioPool = new AudioSource[NVRCollisionSoundController.Instance.SoundPoolSize];

            AudioSourcePrefab = Resources.Load<GameObject>(AudioSourcePrefabPath);

            for (int index = 0; index < AudioPool.Length; index++)
            {
                AudioPool[index] = GameObject.Instantiate<GameObject>(AudioSourcePrefab).GetComponent<AudioSource>();
                AudioPool[index].transform.parent = this.transform;
            }

			// Load Default sounds
			List<AudioClip> clips = new List<AudioClip>();
			clips.AddRange(Resources.LoadAll<AudioClip>(CollisionSoundsPath));

			// Load Custom sounds
			NVRCollisionSoundObject[] collisionSoundObjects = FindObjectsOfType<NVRCollisionSoundObject>();
			foreach (NVRCollisionSoundObject soundObject in collisionSoundObjects) {
				// Loop through obejcts and import clips (if they aren't already imported)
				if(soundObject.importedClips != null && soundObject.importedClips.Length > 0 && !clips.Contains(soundObject.importedClips[0])){
					clips.AddRange (soundObject.importedClips);
				}
			}
            Clips = new Dictionary<string, List<AudioClip>>();
			for (int index = 0; index < clips.Count; index++)
            {
                string materialName = clips[index].name;
                int dividerIndex = materialName.IndexOf("__");
                if (dividerIndex >= 0)
                    materialName = materialName.Substring(0, dividerIndex);

                if (NVRCollisionSoundMaterialsList.MaterialKeys.Contains(materialName))
                {
                    if (Clips.ContainsKey(materialName) == false || Clips[materialName] == null)
                        Clips[materialName] = new List<AudioClip>();
                    Clips[materialName].Add(clips[index]);
                }
                else
                {
                    // Custom material found, add mateiral key
                    NVRCollisionSoundMaterialsList.MaterialKeys.Add(materialName);
                }
            }
        }

        public override void Play(string material, Vector3 position, float impactVolume)
        {
			if (material.Equals(NVRCollisionSoundMaterialsList.EmptyMaterialName))
                return;

            if (NVRCollisionSoundController.Instance.PitchModulationEnabled == true)
            {
                AudioPool[CurrentPoolIndex].pitch = Random.Range(1 - NVRCollisionSoundController.Instance.PitchModulationRange, 1 + NVRCollisionSoundController.Instance.PitchModulationRange);
            }

            AudioPool[CurrentPoolIndex].transform.position = position;
            AudioPool[CurrentPoolIndex].volume = impactVolume;
            AudioPool[CurrentPoolIndex].clip = GetClip(material);
            AudioPool[CurrentPoolIndex].Play();

            CurrentPoolIndex++;

            if (CurrentPoolIndex >= AudioPool.Length)
            {
                CurrentPoolIndex = 0;
            }
        }

        private AudioClip GetClip(string material)
        { 
            if (Clips.ContainsKey(material) == false)
            {
				material = NVRCollisionSoundMaterialsList.DefaultMaterial;
                Debug.LogError("[NewtonVR] CollisionSound: Trying to play sound for material without a clip. Need a clip at: " + CollisionSoundsPath + "/" + material);
            }

            int index = Random.Range(0, Clips[material].Count);

            return Clips[material][index];
        }
    }
}
