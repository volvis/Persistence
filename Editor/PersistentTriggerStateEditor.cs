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
        HashSet<int> intConflicts;

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

            intConflicts = new HashSet<int>();
            var others = FindObjectsOfType<PersistentTriggerState>();
            var @target = this.target as PersistentTriggerState;
            foreach (var o in others)
            {
                if (o != @target && o.gameObject.scene == @target.gameObject.scene)
                    intConflicts.Add(o.SceneIndex);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var conflict in sceneConflicts)
            {
                EditorGUILayout.HelpBox(string.Format("Hash conflict detected with {0}.\nRename scene to fix.", conflict), MessageType.Error);
            }

            var pts = target as PersistentTriggerState;

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Trigger state"))
                {
                    
                    pts.TriggerState();
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

            var saveIndex = serializedObject.FindProperty("m_saveIndex");
            if (intConflicts.Contains(saveIndex.intValue))
            {
                EditorGUILayout.HelpBox("Same SaveIndex found in two or more components.", MessageType.Error);
                if (GUILayout.Button("Find unique SaveIndex"))
                {
                    var index = 0;
                    while (intConflicts.Contains(index))
                        index++;
                    saveIndex.intValue = index;
                }
            }

            //string debug = string.Format("Scene Index: {0}\nSave Index: {1}", new PropertyName(pts.gameObject.scene.name).GetHashCode(), serializedObject.FindProperty("m_saveIndex").intValue);

            //EditorGUILayout.HelpBox(debug, MessageType.Info);

            EditorGUILayout.LabelField("Scene Index", new PropertyName(pts.gameObject.scene.name).GetHashCode().ToString());

            EditorGUILayout.PropertyField(saveIndex);
            var prop = serializedObject.FindProperty("m_respawn");
            prop.intValue = EditorGUILayout.IntSlider("Respawn distance", prop.intValue, 0, 6);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTriggeredBefore"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_respawn"));
            
            

            serializedObject.ApplyModifiedProperties();
        }
    }

}
