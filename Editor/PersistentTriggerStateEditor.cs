using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace Aijai.Persistence
{
    [CustomEditor(typeof(PersistentTriggerState))]
    public class PersistentTriggerStateEditor : Editor
    {
        List<string> sceneConflicts;

        private void OnEnable()
        {
            sceneConflicts = new List<string>();
            var hashes = new HashSet<int>();
            foreach(var asset in AssetDatabase.FindAssets("t:scene"))
            {
                var p = AssetDatabase.GUIDToAssetPath(asset);
                var hash = new PropertyName(Path.GetFileNameWithoutExtension(p)).GetHashCode();
                if (!hashes.Add(hash))
                {
                    sceneConflicts.Add(p);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var conflict in sceneConflicts)
            {
                EditorGUILayout.HelpBox(string.Format("Hash conflict detected with {0}.\nRename scene to fix.", conflict), MessageType.Error);
            }

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Trigger state"))
                {
                    var a = target as PersistentTriggerState;
                    a.TriggerState();
                }
                if (GUILayout.Button("Create checkpoint"))
                {
                    PersistentTriggerState.CreateCheckpoint();
                }
                if (PersistentTriggerState.HasCheckpoint() && GUILayout.Button("Restore checkpoint"))
                {
                    PersistentTriggerState.ReturnCheckpoint();
                }
            }

            EditorGUILayout.HelpBox("Save Index: " + serializedObject.FindProperty("m_sceneIndex").intValue, MessageType.Info);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTriggeredBefore"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}
