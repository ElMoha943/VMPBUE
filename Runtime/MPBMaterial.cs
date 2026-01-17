using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace valenvrc.Tools.MPB
{
    public class MPBMaterial : UdonSharpBehaviour
    {
        public Material material;
        public string[] propertyNames;
        public int[] propertyTypes;
        public float[] floatValues;
        public Color[] colorValues;
        public Vector4[] vectorValues;
        public Texture[] textureValues;
        
        public void ApplyToRenderer(Renderer renderer, int materialIndex)
        {
            if (renderer == null || propertyNames == null)
            {
                Debug.LogWarning("[MPBMaterial] Renderer or propertyNames is null");
                return;
            }
            
            if (propertyNames.Length == 0)
            {
                Debug.LogWarning($"[MPBMaterial] No properties to apply for material {material.name}");
                return;
            }
            
            Debug.Log($"[MPBMaterial] Array lengths - names:{propertyNames.Length} types:{propertyTypes.Length} floats:{floatValues.Length} colors:{colorValues.Length} vectors:{vectorValues.Length} textures:{textureValues.Length}");
            
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb, materialIndex);
            
            int appliedCount = 0;
            for (int i = 0; i < propertyNames.Length; i++)
            {
                string propName = propertyNames[i];
                int propType = propertyTypes[i];
                
                switch (propType)
                {
                    case 0:
                        if (i < colorValues.Length)
                        {
                            Color value = colorValues[i];
                            mpb.SetColor(propName, value);
                            Debug.Log($"[MPBMaterial] Set Color: {propName} = {value}");
                            appliedCount++;
                        }
                        break;
                    case 1:
                        if (i < vectorValues.Length)
                        {
                            Vector4 value = vectorValues[i];
                            mpb.SetVector(propName, value);
                            Debug.Log($"[MPBMaterial] Set Vector: {propName} = {value}");
                            appliedCount++;
                        }
                        break;
                    case 2:
                    case 3:
                        if (i < floatValues.Length)
                        {
                            float value = floatValues[i];
                            mpb.SetFloat(propName, value);
                            Debug.Log($"[MPBMaterial] Set Float: {propName} = {value}");
                            appliedCount++;
                        }
                        break;
                    case 4:
                        if (i < textureValues.Length)
                        {
                            Texture value = textureValues[i];
                            mpb.SetTexture(propName, value);
                            Debug.Log($"[MPBMaterial] Set Texture: {propName} = {(value != null ? value.name : "null")}");
                            appliedCount++;
                        }
                        break;
                }
            }
            
            renderer.SetPropertyBlock(mpb, materialIndex);
            
            MaterialPropertyBlock verifyMpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(verifyMpb, materialIndex);
            
            Debug.Log($"[MPBMaterial] Applied {appliedCount} properties to {renderer.name} material index {materialIndex} ({material.name})");
            Debug.Log($"[MPBMaterial] Verification - isEmpty: {verifyMpb.isEmpty}");
        }
    }
}