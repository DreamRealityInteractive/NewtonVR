using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NewtonVR;

[CustomEditor(typeof(NVRCollisionSoundObject))]
public class NVRCollisionSoundObjectEditor : Editor {

    private NVRCollisionSoundObject sObject;
    private int standardMaterialIndex;
    private int smallMaterialIndex;
    private int largeMaterialIndex;

	private SerializedProperty smallScaleThreshold;
	private SerializedProperty largeScaleThreshold;
	private SerializedProperty soundCooldown;

    public void OnEnable()
    {
        smallScaleThreshold = serializedObject.FindProperty("m_smallScaleThreshold");
        largeScaleThreshold = serializedObject.FindProperty("m_largeScaleThreshold");
        soundCooldown = serializedObject.FindProperty("m_soundCooldown"); 
        sObject = (NVRCollisionSoundObject)target;
    }

    public override void OnInspectorGUI()
    {
		// Standard Material
        GUILayout.BeginHorizontal();
        GUILayout.Label("Standard Material", GUILayout.Width(115));
        GUILayout.FlexibleSpace();
		standardMaterialIndex = EditorGUILayout.Popup(NVRCollisionSoundMaterialsList.MaterialKeys.IndexOf(sObject.m_material), NVRCollisionSoundMaterialsList.MaterialKeys.ToArray());
		if (standardMaterialIndex < 0) {
			// Material not in list - set to none
			standardMaterialIndex = 0;
		}
        sObject.m_material = NVRCollisionSoundMaterialsList.MaterialKeys[standardMaterialIndex];
        GUILayout.EndHorizontal();

		// Small Scale Threshold
        EditorGUILayout.PropertyField(smallScaleThreshold);

		// Small Material
        GUILayout.BeginHorizontal();
        GUILayout.Label("Small Material", GUILayout.Width(115));
        GUILayout.FlexibleSpace();
		smallMaterialIndex = EditorGUILayout.Popup(NVRCollisionSoundMaterialsList.MaterialKeys.IndexOf(sObject.m_smallScaleMaterial), NVRCollisionSoundMaterialsList.MaterialKeys.ToArray());
		if (smallMaterialIndex < 0) {
			// Material not in list - set to none
			smallMaterialIndex = 0;
		}
		sObject.m_smallScaleMaterial = NVRCollisionSoundMaterialsList.MaterialKeys[smallMaterialIndex];
        GUILayout.EndHorizontal();

		// Large Scale Threshold
        EditorGUILayout.PropertyField(largeScaleThreshold);

		// Large Material
        GUILayout.BeginHorizontal();
        GUILayout.Label("Large Material", GUILayout.Width(115));
        GUILayout.FlexibleSpace();
		largeMaterialIndex = EditorGUILayout.Popup(NVRCollisionSoundMaterialsList.MaterialKeys.IndexOf(sObject.m_largeScaleMaterial), NVRCollisionSoundMaterialsList.MaterialKeys.ToArray());
		if (largeMaterialIndex < 0) {
			// Material not in list - set to none
			largeMaterialIndex = 0;
		}
        sObject.m_largeScaleMaterial = NVRCollisionSoundMaterialsList.MaterialKeys[largeMaterialIndex];
        GUILayout.EndHorizontal();

		// Sound Cooldown
        EditorGUILayout.PropertyField(soundCooldown);

		if (GUI.changed) {
			ReimportClips ();
			serializedObject.ApplyModifiedProperties();
			if (!Application.isPlaying) {
				EditorUtility.SetDirty (sObject);
				UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty ();
			}
		}
    }

	public void ReimportClips(){
		List<AudioClip> clips = new List<AudioClip> ();
		NVRProjectCollisionMaterials projectSpecificMaterials = Resources.Load <NVRProjectCollisionMaterials>("ProjectCollisionMaterials");
		if (projectSpecificMaterials != null) {
			// Import main material audio files
			AudioClip[] standardClips = projectSpecificMaterials.GetClipsForMaterial (sObject.m_material);
			if (standardClips != null) {
				clips.AddRange (standardClips);
			}

			// Import small material audio - skip import if already imported
			if (sObject.m_smallScaleMaterial != sObject.m_material) {
				AudioClip[] smallClips = projectSpecificMaterials.GetClipsForMaterial (sObject.m_smallScaleMaterial);
				if (smallClips != null) {
					clips.AddRange (smallClips);
				}
			}

			// Import Large material audio - skip import if already imported
			if (sObject.m_largeScaleMaterial != sObject.m_material && sObject.m_largeScaleMaterial != sObject.m_smallScaleMaterial) {
				AudioClip[] largeClips = projectSpecificMaterials.GetClipsForMaterial (sObject.m_largeScaleMaterial);
				if (largeClips != null) {
					clips.AddRange (largeClips);
				}
			}
		}
		sObject.importedClips = clips.ToArray ();
	}
}
