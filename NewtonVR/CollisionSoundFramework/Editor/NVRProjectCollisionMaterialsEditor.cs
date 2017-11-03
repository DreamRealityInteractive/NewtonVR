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

        if (GUILayout.Button("Rebuild From Current Directory"))
        {
            
            string path = AssetDatabase.GetAssetPath(sObject);
            path = path.Substring(0, path.LastIndexOf("/"));
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] info = dir.GetDirectories("surface - " + "*");
            List<string> materialNames = new List<string>();
            foreach (DirectoryInfo d in info)
            {
                materialNames.Add(d.Name.Substring(directoryPrefix.Length));
            }
            sObject.m_collisionMaterials = materialNames.ToArray();
            EditorUtility.SetDirty(sObject);
        }

        DrawDefaultInspector();
    }
}
