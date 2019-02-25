using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.IO;

namespace Aijai.Persistence
{
    public abstract class PersistentTriggerStateBase : MonoBehaviour
    {
        [SerializeField] protected int m_saveIndex = 0;

        public int SceneIndex { get { return m_saveIndex; } }

        public abstract void TriggerState();

        private void OnValidate()
        {
            if (m_saveIndex < 0)
                m_saveIndex = 0;
        }

        static Dictionary<int, HashSet<int>> Memory;
        
        static MemoryStream Checkpoint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialise()
        {
            Memory = new Dictionary<int, HashSet<int>>();
            Checkpoint = new MemoryStream();
        }


        public static bool GetState(PersistentTriggerStateBase obj)
        {
            return GetState(new PropertyName(obj.gameObject.scene.name).GetHashCode(), obj.m_saveIndex);
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

        public static void SetState(PersistentTriggerStateBase obj, bool state = true)
        {
            SetState(new PropertyName(obj.gameObject.scene.name).GetHashCode(), obj.m_saveIndex, state);
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

        #region IO

        const int TriggerStorageVersion = 1;
        public static void WriteMemory(BinaryWriter writer)
        {
            // Reduce memory
            Memory = Memory.Where(x => Memory[x.Key].Count > 0).ToDictionary(t => t.Key, t => t.Value);

            writer.Write(TriggerStorageVersion);
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

            int version = reader.ReadInt32();

            switch (version)
            {
                case 1:
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
                    break;
            }
        }

        #endregion

        #region Checkpoint

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

        public static void WriteCheckpoint(BinaryWriter writer)
        {
            if (HasCheckpoint())
                throw new System.Exception("Checkpoint has not been created");

            Checkpoint.Seek(0, SeekOrigin.Begin);
            Checkpoint.CopyTo(writer.BaseStream);
        }

        #endregion
        

    }


}
