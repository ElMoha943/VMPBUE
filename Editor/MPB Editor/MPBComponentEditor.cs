using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace valenvrc.Tools.MPB
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MPBComponent))]
    public class MPBComponentEditor : Editor
    {
        private MPBComponent component;

        private void OnEnable()
        {
            component = (MPBComponent)target;
            foreach (Object t in targets)
            {
                MPBComponent comp = (MPBComponent)t;
                if (comp.targetRenderer == null)
                    comp.targetRenderer = comp.GetComponent<Renderer>();
                comp.ApplyPropertiesToRenderer();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox(
                    $"Editing {targets.Length} MPB Components — property changes will apply to all selected components with a matching material and property name.",
                    MessageType.Info);
                EditorGUILayout.Space(4f);

                if (GUILayout.Button("Read Materials From All Renderers"))
                {
                    foreach (Object t in targets)
                    {
                        MPBComponent comp = (MPBComponent)t;
                        Undo.RecordObject(comp, "Refresh MPB Materials");
                        comp.RefreshFromRenderer();
                        comp.ApplyPropertiesToRenderer();
                        EditorUtility.SetDirty(comp);
                    }
                }

                EditorGUILayout.Space(8f);
            }
            else
            {
                DrawRendererHeader();
            }

            DrawMaterials();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRendererHeader()
        {
            EditorGUILayout.LabelField("MPB Component", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            EditorGUI.BeginChangeCheck();
            Renderer nextRenderer = (Renderer)EditorGUILayout.ObjectField("Target Renderer", component.targetRenderer, typeof(Renderer), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(component, "Set Target Renderer");
                component.targetRenderer = nextRenderer;
                component.RefreshFromRenderer();
                component.ApplyPropertiesToRenderer();
                EditorUtility.SetDirty(component);
            }

            using (new EditorGUI.DisabledScope(component.targetRenderer == null))
            {
                if (GUILayout.Button("Read Materials From Renderer"))
                {
                    Undo.RecordObject(component, "Refresh MPB Materials");
                    component.RefreshFromRenderer();
                    component.ApplyPropertiesToRenderer();
                    EditorUtility.SetDirty(component);
                }
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawMaterials()
        {
            if (component.targetRenderer == null)
            {
                EditorGUILayout.HelpBox("Attach this component to a GameObject with a Renderer, or assign Target Renderer.", MessageType.Info);
                return;
            }

            if (component.materials == null || component.materials.Length == 0)
            {
                EditorGUILayout.HelpBox("No materials found on renderer.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < component.materials.Length; i++)
            {
                MPBMaterialConfig materialConfig = component.materials[i];
                if (materialConfig == null || materialConfig.material == null)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                materialConfig.foldout = EditorGUILayout.Foldout(materialConfig.foldout, "Material: " + materialConfig.material.name, true);

                if (materialConfig.foldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Shader", materialConfig.material.shader != null ? materialConfig.material.shader.name : "None");
                    EditorGUILayout.Space(3f);

                    DrawProperties(materialConfig);

                    if (GUILayout.Button("Add Property"))
                    {
                        ShowPropertyMenu(materialConfig);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2f);
            }
        }

        private void DrawProperties(MPBMaterialConfig materialConfig)
        {
            if (materialConfig.properties == null)
            {
                materialConfig.properties = new MPBPropertyConfig[0];
            }

            for (int i = 0; i < materialConfig.properties.Length; i++)
            {
                MPBPropertyConfig property = materialConfig.properties[i];
                if (property == null)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(property.name, GUILayout.Width(160));

                EditorGUI.BeginChangeCheck();
                DrawPropertyValueField(property, materialConfig.material);
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyPropertyChangeToAll(materialConfig.material, property);
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    RemovePropertyFromAll(materialConfig.material, property.name);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void ApplyPropertyChangeToAll(Material material, MPBPropertyConfig sourceProperty)
        {
            foreach (Object t in targets)
            {
                MPBComponent comp = (MPBComponent)t;
                if (comp.materials == null) continue;

                bool dirty = false;
                foreach (MPBMaterialConfig matConfig in comp.materials)
                {
                    if (matConfig == null || matConfig.material != material || matConfig.properties == null) continue;

                    foreach (MPBPropertyConfig prop in matConfig.properties)
                    {
                        if (prop == null || prop.name != sourceProperty.name || prop.type != sourceProperty.type) continue;

                        Undo.RecordObject(comp, "Edit MPB Property");
                        prop.floatValue = sourceProperty.floatValue;
                        prop.colorValue = sourceProperty.colorValue;
                        prop.vectorValue = sourceProperty.vectorValue;
                        prop.textureValue = sourceProperty.textureValue;
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    comp.ApplyPropertiesToRenderer();
                    EditorUtility.SetDirty(comp);
                }
            }
        }

        private void RemovePropertyFromAll(Material material, string propertyName)
        {
            foreach (Object t in targets)
            {
                MPBComponent comp = (MPBComponent)t;
                if (comp.materials == null) continue;

                foreach (MPBMaterialConfig matConfig in comp.materials)
                {
                    if (matConfig == null || matConfig.material != material || matConfig.properties == null) continue;

                    List<MPBPropertyConfig> props = matConfig.properties.ToList();
                    if (props.RemoveAll(p => p != null && p.name == propertyName) > 0)
                    {
                        Undo.RecordObject(comp, "Remove MPB Property");
                        matConfig.properties = props.ToArray();
                        comp.ApplyPropertiesToRenderer();
                        EditorUtility.SetDirty(comp);
                    }
                }
            }
        }

        private void DrawPropertyValueField(MPBPropertyConfig property, Material material)
        {
            switch (property.type)
            {
                case 0:
                    bool isHDR = IsHDRColorProperty(material, property.name);
                    property.colorValue = EditorGUILayout.ColorField(
                        GUIContent.none, property.colorValue,
                        showEyedropper: true, showAlpha: true, hdr: isHDR);
                    break;
                case 1:
                    property.vectorValue = EditorGUILayout.Vector4Field(string.Empty, property.vectorValue);
                    break;
                case 2:
                case 3:
                    property.floatValue = EditorGUILayout.FloatField(property.floatValue);
                    break;
                case 4:
                    property.textureValue = (Texture)EditorGUILayout.ObjectField(property.textureValue, typeof(Texture), false);
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported");
                    break;
            }
        }

        private static bool IsHDRColorProperty(Material material, string propertyName)
        {
            if (material == null) return false;
            MaterialProperty[] matProps = MaterialEditor.GetMaterialProperties(new Object[] { material });
            for (int i = 0; i < matProps.Length; i++)
            {
                if (matProps[i].name == propertyName)
                    return (matProps[i].flags & MaterialProperty.PropFlags.HDR) != 0;
            }
            return false;
        }

        private void ShowPropertyMenu(MPBMaterialConfig materialConfig)
        {
            GenericMenu menu = new GenericMenu();

            if (materialConfig.material == null || materialConfig.material.shader == null)
            {
                menu.AddDisabledItem(new GUIContent("No shader available"));
                menu.ShowAsContext();
                return;
            }

            HashSet<string> existing = new HashSet<string>();
            if (materialConfig.properties != null)
            {
                for (int i = 0; i < materialConfig.properties.Length; i++)
                {
                    MPBPropertyConfig prop = materialConfig.properties[i];
                    if (prop != null)
                    {
                        existing.Add(prop.name);
                    }
                }
            }

            Shader shader = materialConfig.material.shader;
            int count = ShaderUtil.GetPropertyCount(shader);
            Dictionary<ShaderUtil.ShaderPropertyType, List<string>> byType = new Dictionary<ShaderUtil.ShaderPropertyType, List<string>>();

            for (int i = 0; i < count; i++)
            {
                string propName = ShaderUtil.GetPropertyName(shader, i);
                if (existing.Contains(propName))
                {
                    continue;
                }

                ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
                if (!byType.ContainsKey(type))
                {
                    byType[type] = new List<string>();
                }

                byType[type].Add(propName);
            }

            if (byType.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No properties available"));
            }
            else
            {
                foreach (KeyValuePair<ShaderUtil.ShaderPropertyType, List<string>> entry in byType.OrderBy(k => k.Key.ToString()))
                {
                    ShaderUtil.ShaderPropertyType shaderType = entry.Key;

                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        string propName = entry.Value[i];
                        string path = shaderType + "/" + propName;
                        menu.AddItem(new GUIContent(path), false, () =>
                        {
                            AddPropertyToAll(materialConfig, propName, shaderType);
                        });
                    }
                }
            }

            menu.ShowAsContext();
        }

        private void AddPropertyToAll(MPBMaterialConfig primaryMatConfig, string name, ShaderUtil.ShaderPropertyType shaderType)
        {
            int propType = ConvertShaderPropertyType(shaderType);

            foreach (Object t in targets)
            {
                MPBComponent comp = (MPBComponent)t;
                if (comp.materials == null) continue;

                foreach (MPBMaterialConfig matConfig in comp.materials)
                {
                    if (matConfig == null || matConfig.material != primaryMatConfig.material) continue;

                    if (matConfig.properties != null && matConfig.properties.Any(p => p != null && p.name == name)) continue;

                    Undo.RecordObject(comp, "Add MPB Property");
                    List<MPBPropertyConfig> properties = matConfig.properties != null
                        ? matConfig.properties.ToList()
                        : new List<MPBPropertyConfig>();

                    properties.Add(new MPBPropertyConfig
                    {
                        name = name,
                        type = propType,
                        floatValue = 0f,
                        colorValue = Color.white,
                        vectorValue = Vector4.zero,
                        textureValue = null
                    });

                    matConfig.properties = properties.ToArray();
                    comp.ApplyPropertiesToRenderer();
                    EditorUtility.SetDirty(comp);
                }
            }
        }

        private int ConvertShaderPropertyType(ShaderUtil.ShaderPropertyType type)
        {
            switch (type)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    return 0;
                case ShaderUtil.ShaderPropertyType.Vector:
                    return 1;
                case ShaderUtil.ShaderPropertyType.Float:
                    return 2;
                case ShaderUtil.ShaderPropertyType.Range:
                    return 3;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    return 4;
                default:
                    return 2;
            }
        }
    }
}
