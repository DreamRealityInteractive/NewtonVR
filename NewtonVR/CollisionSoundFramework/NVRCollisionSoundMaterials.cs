using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NewtonVR
{
    public class NVRCollisionSoundMaterialsList
    {
		public const string DefaultMaterial = "_default";
		public const string EmptyMaterialName = "none";

        private static List<string> materialKeys;
        public static List<string> MaterialKeys
        {
            get
            {
                if(materialKeys == null)
                {
                    materialKeys = new List<string>
                    {
                        "none",
                        "_default",
                        "carpet",
                        "wood",
                        "metal",
                        "glass",
                        "plastic",
                        "cardboard",
                        "EndNewtonVRMaterials"
                    };
                    // Load User Specific Materials
                    NVRProjectCollisionMaterials projectSpecificMaterials = Resources.Load <NVRProjectCollisionMaterials>("CollisionSounds/ProjectCollisionMaterials");
                    if(projectSpecificMaterials != null)
                    {
                        materialKeys.AddRange(projectSpecificMaterials.m_collisionMaterials);
                    }
                }

                return materialKeys;
            }
        }
    }
}