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
        
        [SerializeField] internal int m_saveIndex = 0;
        public int SceneIndex { get { return m_saveIndex; } }

        [SerializeField] int m_respawn;

        [ContextMenu("Debug trigger")]
        public void TriggerState()
        {
            OnTrigger.Invoke();
            SetState(this, true);
        }

        protected virtual void OnEnable()
        {
            var sceneID = new PropertyName(this.gameObject.scene.name).GetHashCode();
            if (SceneHistory.First == null || SceneHistory.First.Value != sceneID)
            {
                SceneHistory.AddFirst(sceneID);
                if (SceneHistory.Count > 6)
                    SceneHistory.RemoveLast();
            }

            
            
            if (GetState(this))
            {
                if (m_respawn > 0)
                {
                    var head = SceneHistory.First.Next;
                    int steps = 0;
                    while(head != null)
                    {
                        if (head.Value == sceneID)
                            break;
                        steps++;
                        head = head.Next;
                    }

                    if (steps > m_respawn)
                    {
                        SetState(this, false);
                    }
                    else
                    {
                        OnTriggeredBefore.Invoke();
                    }
                }
                else
                {
                    OnTriggeredBefore.Invoke();
                }
            }
                
        }

        private void OnValidate()
        {
            if (m_saveIndex < 0)
                m_saveIndex = 0;
        }
    }

    public partial class PersistentTriggerState
    {
        static Dictionary<int, HashSet<int>> Memory;
        static LinkedList<int> SceneHistory;
        static MemoryStream Checkpoint;

        public static string DebugString()
        {
            var str = new System.Text.StringBuilder();

            str.Append("History: ");
            foreach (var h in SceneHistory)
            {
                str.Append(h);
                str.Append(';');
            }
            str.AppendLine();
            str.AppendLine("----");
            foreach (var m in Memory)
            {
                str.AppendLine(string.Format("Scene: {0}----", m.Key));
                str.AppendLine("\t");
                foreach (var a in m.Value)
                {
                    str.Append(a);
                    str.Append(' ');
                }
            }

            return str.ToString();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialise()
        {
            Memory = new Dictionary<int, HashSet<int>>();
            SceneHistory = new LinkedList<int>();
            Checkpoint = new MemoryStream();
        }

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

        public static void SetState(PersistentTriggerState obj, bool state = true)
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
    }


}
