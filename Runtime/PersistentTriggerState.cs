using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.IO;

namespace Aijai.Persistence
{
    public partial class PersistentTriggerState : MonoBehaviour
    {
        [SerializeField] UnityEvent OnTrigger;
        
        [SerializeField] UnityEvent OnTriggeredBefore;
        
        [SerializeField] internal int m_sceneIndex = 0;
        public int SceneIndex { get { return m_sceneIndex; } }

        [ContextMenu("Debug trigger")]
        public void TriggerState()
        {
            OnTrigger.Invoke();
            SetState(this, true);
        }

        protected virtual void OnEnable()
        {
            if (GetState(this))
                OnTriggeredBefore.Invoke();
        }

        private void OnValidate()
        {
            int[] reservedIndices = (from pts in FindObjectsOfType<PersistentTriggerState>() where (pts != this && pts.gameObject.scene == this.gameObject.scene) select pts.SceneIndex).ToArray();
            if (reservedIndices.Contains(SceneIndex))
            {
                m_sceneIndex = 0;
                while (reservedIndices.Contains(SceneIndex))
                    m_sceneIndex++;
            }
        }
    }

    public partial class PersistentTriggerState
    {
        static Dictionary<int, HashSet<int>> Memory = new Dictionary<int, HashSet<int>>();
        static MemoryStream Checkpoint = new MemoryStream();

        public static bool HasCheckpoint() { return Checkpoint.Length != 0; }
        
        public static void CreateCheckpoint()
        {
            BinaryWriter writer = new BinaryWriter(Checkpoint);
            try
            {
                WriteMemory(writer);
            }
            finally
            {
                writer.Flush();
                Checkpoint.Position = 0;
            }
        }

        public static void ReturnCheckpoint()
        {
            BinaryReader reader = new BinaryReader(Checkpoint);
            try
            {
                ReadMemory(reader);
            }
            finally
            {
                Checkpoint.Position = 0;
            }
        }

        public static bool GetState(PersistentTriggerState obj)
        {
            return GetState(new PropertyName(obj.gameObject.scene.name).GetHashCode(), obj.m_sceneIndex);
        }

        public static bool GetState(int sceneID, int index)
        {
            HashSet<int> section;
            if (Memory.TryGetValue(sceneID, out section))
            {
                return section.Contains(index);
            }
            return false;
        }

        public static void SetState(PersistentTriggerState obj, bool state = true)
        {
            SetState(new PropertyName(obj.gameObject.scene.name).GetHashCode(), obj.m_sceneIndex);
        }

        public static void SetState(int sceneID, int index, bool state = true)
        {
            HashSet<int> section;
            if (!Memory.TryGetValue(sceneID, out section))
            {
                section = new HashSet<int>();
                Memory.Add(sceneID, section);
            }
            if (state)
            {
                section.Add(index);
            }
            else
            {
                section.Remove(index);
            }
        }

        public static void WriteMemory(BinaryWriter writer)
        {
            // Reduce memory
            Memory = Memory.Where(x => Memory[x.Key].Count > 0).ToDictionary(t => t.Key, t => t.Value);

            writer.Write(Memory.Count);
            foreach (var key in Memory.Keys)
            {
                writer.Write(key);
                var set = Memory[key];
                writer.Write(set.Count);
                foreach (var i in set)
                    writer.Write(i);
            }
        }

        public static void ReadMemory(BinaryReader reader)
        {
            Memory = new Dictionary<int, HashSet<int>>();
            int keyCount = reader.ReadInt32();
            for (var i = 0; i < keyCount; i++)
            {
                int key = reader.ReadInt32();
                int valueCount = reader.ReadInt32();
                var set = new HashSet<int>();

                for (var a = 0; a < valueCount; a++)
                    set.Add(reader.ReadInt32());

                Memory.Add(key, set);
            }
        }
    }


}
