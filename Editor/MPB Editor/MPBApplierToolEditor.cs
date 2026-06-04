using UnityEditor;
using UnityEngine;
using UdonSharpEditor;

namespace valenvrc.Tools.MPB
{
    [CustomEditor(typeof(MPBApplierTool))]
    public class MPBApplierToolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            base.OnInspectorGUI();

            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Find All Components", GUILayout.Height(32)))
            {
                MPBApplierTool applier = (MPBApplierTool)target;
                MPBApplierComponentSync.SyncApplier(applier);
            }
        }
    }
}
