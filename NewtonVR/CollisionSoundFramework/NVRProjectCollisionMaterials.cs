using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NVRCollisionMaterialDef{
	public string m_materialName;
	public AudioClip[] m_audioClips;
}

[CreateAssetMenu(fileName = "ProjectCollisionMaterials ", menuName = "ProjectCollisionMaterials", order = 1)]
public class NVRProjectCollisionMaterials : ScriptableObject {
	public NVRCollisionMaterialDef[] m_collisionMaterials;

	public string[] GetMaterialNames(){
		string[] materials = new string[m_collisionMaterials.Length];
		for (int i = 0; i < m_collisionMaterials.Length; i++) {
			materials[i] = m_collisionMaterials [i].m_materialName;
		}
		return materials;
	}

	public AudioClip[] GetClipsForMaterial(string material){
		for (int i = 0; i < m_collisionMaterials.Length; i++) {
			if (m_collisionMaterials [i].m_materialName == material) {
				return m_collisionMaterials [i].m_audioClips;
			}
		}
		return null;
	}
}
