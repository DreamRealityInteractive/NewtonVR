using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(NVRProjectCollisionMaterials))]
public class NVRProjectCollisionMaterialsEditor : Editor
{
    private NVRProjectCollisionMaterials sObject;
    private const string directoryPrefix = "surface - ";

    public void OnEnable()
    {
        sObject = (NVRProjectCollisionMaterials)target; 
    }

    public override void OnInspectorGUI()
    {
		GUILayout.Label ("Current folder path: " + sObject.m_currentDirectory);
		if (GUILayout.Button ("Change Directory")) {
			string path = EditorUtility.OpenFolderPanel("Select Collision Sounds Directory", "", "");
			path = path.Substring (Application.dataPath.Length - 6);
			sObject.m_currentDirectory = path;
			EditorUtility.SetDirty(sObject);
		}

        if (GUILayout.Button("Rebuild"))
        {
            
			string path = sObject.m_currentDirectory;
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] info = dir.GetDirectories("surface - " + "*");
			List<NVRCollisionMaterialDef> materials = new List<NVRCollisionMaterialDef>();
            foreach (DirectoryInfo d in info)
            {
				NVRCollisionMaterialDef matDef = new NVRCollisionMaterialDef ();
				matDef.m_materialName = d.Name.Substring (directoryPrefix.Length);
				List<AudioClip> clips = new List<AudioClip> ();
				foreach (FileInfo f in d.GetFiles()) {
					string clipPath = path + "/" + d.Name + "/" + f.Name;
					if (clipPath.StartsWith(Application.dataPath)) {
						clipPath = "Assets" + clipPath.Substring(Application.dataPath.Length);
					}
					AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip> (clipPath);
					if (clip != null) {
						clips.Add(clip);
					}
				}
				matDef.m_audioClips = clips.ToArray ();

				materials.Add(matDef);
            }
			sObject.m_collisionMaterials = materials.ToArray();
            EditorUtility.SetDirty(sObject);
        }

        DrawDefaultInspector();
    }
}
