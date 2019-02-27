using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Aijai.Persistence
{
    [CustomEditor(typeof(PersistentStateAsset))]
    public class PersistentStateAssetEditor : Editor
    {

        List<int> m_otherInstances;

        private void OnEnable()
        {
            m_otherInstances = Resources.FindObjectsOfTypeAll<PersistentStateAsset>()
                .Where(x => x != target)
                .Select(x => x.PersistentIndex)
                .ToList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var persistentIndex = serializedObject.FindProperty("m_persistentIndex");
            var index = persistentIndex.intValue;

            if ( m_otherInstances.IndexOf(index) != -1)
            {
                index = int.MinValue;
                while (m_otherInstances.IndexOf(index) != -1)
                {
                    index++;
                }
                persistentIndex.intValue = index;
            }

            EditorGUILayout.LabelField("ID", persistentIndex.intValue.ToString());

            serializedObject.ApplyModifiedProperties();
        }
    }
}