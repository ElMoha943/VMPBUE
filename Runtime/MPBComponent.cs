using System.Collections.Generic;
using UnityEngine;

namespace valenvrc.Tools.MPB
{
    public class MPBComponent : MonoBehaviour
    {
        public Renderer targetRenderer;
        public MPBMaterialConfig[] materials = new MPBMaterialConfig[0];

        private void Reset()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }

            RefreshFromRenderer();
            ApplyPropertiesToRenderer();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }

            RefreshFromRenderer();
            ApplyPropertiesToRenderer();
        }
#endif

        public void RefreshFromRenderer()
        {
            if (targetRenderer == null)
            {
                materials = new MPBMaterialConfig[0];
                return;
            }

            Material[] sharedMaterials = targetRenderer.sharedMaterials;
            Dictionary<Material, MPBMaterialConfig> existing = new Dictionary<Material, MPBMaterialConfig>();

            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    MPBMaterialConfig config = materials[i];
                    if (config == null || config.material == null)
                    {
                        continue;
                    }

                    if (!existing.ContainsKey(config.material))
                    {
                        existing.Add(config.material, config);
                    }
                }
            }

            List<MPBMaterialConfig> next = new List<MPBMaterialConfig>();

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material material = sharedMaterials[i];
                if (material == null)
                {
                    continue;
                }

                MPBMaterialConfig config;
                if (existing.TryGetValue(material, out config))
                {
                    next.Add(config);
                }
                else
                {
                    next.Add(new MPBMaterialConfig
                    {
                        material = material,
                        properties = new MPBPropertyConfig[0],
                        foldout = true
                    });
                }
            }

            materials = next.ToArray();
        }

        public bool HasAnyProperty()
        {
            if (materials == null)
            {
                return false;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                MPBMaterialConfig material = materials[i];
                if (material != null && material.properties != null && material.properties.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void ApplyPropertiesToRenderer()
        {
            if (targetRenderer == null || materials == null)
            {
                return;
            }

            Material[] sharedMaterials = targetRenderer.sharedMaterials;

            for (int matIndex = 0; matIndex < sharedMaterials.Length; matIndex++)
            {
                Material sharedMaterial = sharedMaterials[matIndex];
                if (sharedMaterial == null)
                {
                    continue;
                }

                MPBMaterialConfig source = null;
                for (int i = 0; i < materials.Length; i++)
                {
                    MPBMaterialConfig config = materials[i];
                    if (config != null && config.material == sharedMaterial)
                    {
                        source = config;
                        break;
                    }
                }

                if (source == null || source.properties == null)
                {
                    continue;
                }

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                for (int i = 0; i < source.properties.Length; i++)
                {
                    MPBPropertyConfig property = source.properties[i];
                    if (property == null || string.IsNullOrEmpty(property.name))
                    {
                        continue;
                    }

                    switch (property.type)
                    {
                        case 0:
                            mpb.SetColor(property.name, property.colorValue);
                            break;
                        case 1:
                            mpb.SetVector(property.name, property.vectorValue);
                            break;
                        case 2:
                        case 3:
                            mpb.SetFloat(property.name, property.floatValue);
                            break;
                        case 4:
                            mpb.SetTexture(property.name, property.textureValue);
                            break;
                    }
                }

                targetRenderer.SetPropertyBlock(mpb, matIndex);
            }
        }
    }
}
