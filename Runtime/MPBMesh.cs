using UdonSharp;
using UnityEngine;

namespace valenvrc.Tools.MPB
{
    public class MPBMesh : UdonSharpBehaviour
    {
        public Renderer targetRenderer;
        public MPBMaterial[] materials = new MPBMaterial[0];

        public void ApplyAllMaterials()
        {
            if (targetRenderer == null)
            {
                Debug.LogWarning("[MPBMesh] targetRenderer is null");
                return;
            }

            if (materials == null || materials.Length == 0)
            {
                Debug.LogWarning($"[MPBMesh] No materials configured for {targetRenderer.name}");
                return;
            }

            Material[] sharedMaterials = targetRenderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                MPBMaterial mpbMat = materials[i];
                if (mpbMat == null || mpbMat.material == null) continue;

                for (int matIndex = 0; matIndex < sharedMaterials.Length; matIndex++)
                {
                    if (sharedMaterials[matIndex] == mpbMat.material)
                    {
                        mpbMat.ApplyToRenderer(targetRenderer, matIndex);
                        break;
                    }
                }
            }
        }
    }
}
