using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UdonSharpEditor;

namespace valenvrc.Tools.MPB
{
    [InitializeOnLoad]
    public class MPBApplierComponentSync : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        static MPBApplierComponentSync()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            EnsureApplierExists();
            SyncAllAppliers();
            AssetDatabase.SaveAssets();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EnsureApplierExists();
                SyncAllAppliers();
            }
        }

        private static MPBApplierTool EnsureApplierExists()
        {
            MPBApplierTool[] appliers = Object.FindObjectsOfType<MPBApplierTool>(true);

            if (appliers.Length > 1)
                Debug.LogWarning("[MPB] Multiple MPBApplierTool instances found. Only the first will be used.");

            if (appliers.Length >= 1)
                return appliers[0];

            MPBComponent[] components = Object.FindObjectsOfType<MPBComponent>(true);
            bool hasAny = false;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i].HasAnyProperty()) { hasAny = true; break; }
            }

            if (!hasAny) return null;

            GameObject applierObj = new GameObject("MPBApplierTool");
            MPBApplierTool newApplier = applierObj.AddUdonSharpComponent<MPBApplierTool>();
            EditorSceneManager.MarkSceneDirty(applierObj.scene);
            Debug.Log("[MPB] No MPBApplierTool found � created one automatically.");
            return newApplier;
        }

        private static void SyncAllAppliers()
        {
            MPBApplierTool[] appliers = Object.FindObjectsOfType<MPBApplierTool>(true);
            if (appliers == null || appliers.Length == 0) return;

            if (appliers.Length > 1)
                Debug.LogWarning("[MPB] Multiple MPBApplierTool instances found. Only the first will be synced.");

            SyncApplier(appliers[0]);
        }

        public static void SyncApplier(MPBApplierTool applier)
        {
            if (applier == null) return;

            // Step 1: delete all existing children
            Transform root = applier.transform;
            for (int i = root.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(root.GetChild(i).gameObject);

            // Step 2: find all MPBComponents in the scene and build hierarchy
            MPBComponent[] components = Object.FindObjectsOfType<MPBComponent>(true);
            List<MPBMesh> meshList = new List<MPBMesh>();

            for (int i = 0; i < components.Length; i++)
            {
                MPBComponent component = components[i];
                if (component == null || component.targetRenderer == null || !component.HasAnyProperty())
                    continue;

                GameObject meshObj = new GameObject("Mesh_" + component.targetRenderer.name);
                meshObj.transform.SetParent(root);
                MPBMesh mpbMesh = meshObj.AddUdonSharpComponent<MPBMesh>();
                mpbMesh.targetRenderer = component.targetRenderer;

                List<MPBMaterial> matList = new List<MPBMaterial>();

                MPBMaterialConfig[] sourceMaterials = component.materials;
                for (int j = 0; j < sourceMaterials.Length; j++)
                {
                    MPBMaterialConfig sourceMat = sourceMaterials[j];
                    if (sourceMat == null || sourceMat.material == null ||
                        sourceMat.properties == null || sourceMat.properties.Length == 0)
                        continue;

                    GameObject matObj = new GameObject("Mat_" + sourceMat.material.name);
                    matObj.transform.SetParent(meshObj.transform);
                    MPBMaterial mpbMat = matObj.AddUdonSharpComponent<MPBMaterial>();
                    mpbMat.material = sourceMat.material;

                    int propCount = sourceMat.properties.Length;
                    string[]  names    = new string[propCount];
                    int[]     types    = new int[propCount];
                    float[]   floats   = new float[propCount];
                    Color[]   colors   = new Color[propCount];
                    Vector4[] vectors  = new Vector4[propCount];
                    Texture[] textures = new Texture[propCount];

                    for (int k = 0; k < propCount; k++)
                    {
                        MPBPropertyConfig prop = sourceMat.properties[k];
                        if (prop == null) continue;
                        names[k]    = prop.name;
                        types[k]    = prop.type;
                        floats[k]   = prop.floatValue;
                        colors[k]   = prop.colorValue;
                        vectors[k]  = prop.vectorValue;
                        textures[k] = prop.textureValue;
                    }

                    mpbMat.propertyNames = names;
                    mpbMat.propertyTypes = types;
                    mpbMat.floatValues   = floats;
                    mpbMat.colorValues   = colors;
                    mpbMat.vectorValues  = vectors;
                    mpbMat.textureValues = textures;

                    UdonSharpEditorUtility.CopyProxyToUdon(mpbMat);
                    EditorUtility.SetDirty(mpbMat);

                    matList.Add(mpbMat);
                }

                if (matList.Count > 0)
                {
                    mpbMesh.materials = matList.ToArray();
                    UdonSharpEditorUtility.CopyProxyToUdon(mpbMesh);
                    EditorUtility.SetDirty(mpbMesh);
                    meshList.Add(mpbMesh);
                }
                else
                {
                    Object.DestroyImmediate(meshObj);
                }
            }

            applier.meshes = meshList.ToArray();
            UdonSharpEditorUtility.CopyProxyToUdon(applier);
            EditorUtility.SetDirty(applier);

            if (applier.gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(applier.gameObject.scene);

            Debug.Log($"[MPB] Sync complete: {meshList.Count} mesh(es) configured on {applier.name}.");
        }
    }
}
