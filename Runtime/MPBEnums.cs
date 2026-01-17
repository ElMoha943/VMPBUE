using UnityEngine;

namespace valenvrc.Tools.MPB
{
    [System.Serializable]
    public class MPBConfig
    {
        public MPBRendererConfig[] renderers;
    }
    
    [System.Serializable]
    public class MPBRendererConfig
    {
        public Renderer renderer;
        public MPBMaterialConfig[] materials;
        public bool foldout;
        public bool hasPendingChanges;
    }
    
    [System.Serializable]
    public class MPBMaterialConfig
    {
        public Material material;
        public MPBPropertyConfig[] properties;
        public bool foldout;
    }
    
    [System.Serializable]
    public class MPBPropertyConfig
    {
        public string name;
        public int type;
        public float floatValue;
        public Color colorValue;
        public Vector4 vectorValue;
        public Texture textureValue;
    }
}