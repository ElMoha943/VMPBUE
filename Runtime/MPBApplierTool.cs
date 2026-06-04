using UdonSharp;
using UnityEngine;

namespace valenvrc.Tools.MPB
{
    public class MPBApplierTool : UdonSharpBehaviour
    {
        public MPBMesh[] meshes = new MPBMesh[0];

        void Start()
        {
            ApplyAllProperties();
        }

        public void ApplyAllProperties()
        {
            if (meshes == null || meshes.Length == 0)
            {
                Debug.LogWarning("[MPBApplierTool] No meshes configured. Use 'Find All Components' in the inspector.");
                return;
            }

            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] != null)
                    meshes[i].ApplyAllMaterials();
            }

            Debug.Log($"[MPBApplierTool] Applied properties to {meshes.Length} mesh(es).");
        }
    }
}
