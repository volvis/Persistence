using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Aijai.Persistence
{
    [CustomEditor(typeof(PersistentTriggerState))]
    public class PersistentTriggerStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
