using UnityEngine;
using UnityEditor;

namespace Aijai.Persistence
{
    [CustomEditor(typeof(PersistentTriggerState))]
    public class PersistentTriggerStateEditor : PersistentStateEditor
    {
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();
            
            var prop = serializedObject.FindProperty("m_respawn");
            prop.intValue = EditorGUILayout.IntSlider("Respawn distance", prop.intValue, 0, 6);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTriggeredBefore"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}
