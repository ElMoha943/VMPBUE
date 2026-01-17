using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace valenvrc.Tools.MPB
{
    public class MPBMesh : UdonSharpBehaviour
    {
        public Renderer targetRenderer;
        public MPBMaterial[] materials;
        
        public void ApplyAllMaterials()
        {
            if (targetRenderer == null)
            {
                Debug.LogWarning("[MPBMesh] targetRenderer is null");
                return;
            }
            
            if (materials == null)
            {
                Debug.LogWarning($"[MPBMesh] materials array is null for {targetRenderer.name}");
                return;
            }
            
            Material[] sharedMaterials = targetRenderer.sharedMaterials;
            Debug.Log($"[MPBMesh] Processing {materials.Length} MPBMaterials for renderer {targetRenderer.name} with {sharedMaterials.Length} materials");
            
            for (int i = 0; i < materials.Length; i++)
            {
                MPBMaterial mpbMat = materials[i];
                if (mpbMat == null)
                {
                    Debug.LogWarning($"[MPBMesh] MPBMaterial at index {i} is null");
                    continue;
                }
                
                if (mpbMat.material == null)
                {
                    Debug.LogWarning($"[MPBMesh] MPBMaterial at index {i} has null material reference");
                    continue;
                }
                
                for (int matIndex = 0; matIndex < sharedMaterials.Length; matIndex++)
                {
                    if (sharedMaterials[matIndex] == mpbMat.material)
                    {
                        Debug.Log($"[MPBMesh] Found material match at index {matIndex}: {mpbMat.material.name}");
                        mpbMat.ApplyToRenderer(targetRenderer, matIndex);
                        break;
                    }
                }
            }
        }
    }
}