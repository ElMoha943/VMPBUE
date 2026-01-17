using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace valenvrc.Tools.MPB
{
    public class MPBApplierTool : UdonSharpBehaviour
    {
        public MPBMesh[] meshes;
        
        void Start()
        {
            ApplyAllProperties();
        }
        
        public void ApplyAllProperties()
        {
            if (meshes == null) return;
            
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] != null)
                {
                    meshes[i].ApplyAllMaterials();
                }
            }
            
            Debug.Log($"[MPBApplierTool] Applied properties to {meshes.Length} meshes");
        }
    }
}