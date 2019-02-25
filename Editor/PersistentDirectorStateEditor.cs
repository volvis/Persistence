using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;

namespace Aijai.Persistence
{
    [CustomEditor(typeof(PersistentDirectorState))]
    public class PersistentDirectorStateEditor : PersistentStateEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var @target = this.target as PersistentDirectorState;
            var director = target.GetComponent<PlayableDirector>();

            if (director.playOnAwake == true)
            {
                EditorGUILayout.HelpBox("Playable Director should not play on awake.", MessageType.Error);
            }

            if (director.extrapolationMode != DirectorWrapMode.Hold)
            {
                EditorGUILayout.HelpBox("Playable Director wrap mode should be Hold.", MessageType.Error);
            }

            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }

}
