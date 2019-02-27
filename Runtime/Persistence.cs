using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Aijai.Persistence
{
    public static class Persistence
    {
        class TriggerSet : SortedSet<int> { }
        class SceneTriggerSet : Dictionary<PropertyName, TriggerSet> { }

        static SceneTriggerSet LocalEvents;
        static HashSet<int> GlobalEvents;
        static MemoryStream Checkpoint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialise()
        {
            LocalEvents = new SceneTriggerSet();
            Checkpoint = new MemoryStream();
            GlobalEvents = new HashSet<int>();
        }

        public static void Purge()
        {
            LocalEvents.Clear();
            GlobalEvents.Clear();
        }

        #region Global events
        public static bool GetMajorEvent(int index)
        {
            return GlobalEvents.Contains(index);
        }

        public static void SetMajorEvent(int index)
        {
            GlobalEvents.Add(index);
        }
        #endregion

        #region Local Events
        public static bool GetState(PersistentTriggerStateBase obj)
        {
            return GetState(new PropertyName(obj.gameObject.scene.name).GetHashCode(), obj.SceneIndex);
        }

        public static bool GetState(int sceneID, int index)
        {
            TriggerSet section;
            if (LocalEvents.TryGetValue(sceneID, out section))
            {
                return section.Contains(index);
            }
            return false;
        }

        public static void SetState(PersistentTriggerStateBase obj, bool state = true)
        {
            SetState(new PropertyName(obj.gameObject.scene.name).GetHashCode(), obj.SceneIndex, state);
        }

        public static void SetState(int sceneID, int index, bool state = true)
        {
            TriggerSet section;
            if (!LocalEvents.TryGetValue(sceneID, out section))
            {
                section = new TriggerSet();
                LocalEvents.Add(sceneID, section);
            }

            if (state)
            {
                section.Add(index);
            }
            else
            {
                section.Remove(index);
                if (section.Count == 0)
                {
                    LocalEvents.Remove(sceneID);
                }
            }
        }
        #endregion

        #region IO

        const int TriggerStorageVersion = 1;
        public static void WriteMemory(BinaryWriter writer)
        {
            writer.Write(TriggerStorageVersion);
            writer.Write(LocalEvents.Count);
            foreach (var key in LocalEvents.Keys)
            {
                writer.Write(key.GetHashCode());
                var set = LocalEvents[key];

                writer.Write(set.Max);

                int empty = 0;
                foreach (var s in set)
                {
                    while (empty < s)
                    {
                        writer.Write(false);
                        empty++;
                    }
                    writer.Write(true);
                    empty = s + 1;
                }
            }

            writer.Write(GlobalEvents.Count);
            foreach (var evt in GlobalEvents)
                writer.Write(evt);
        }

        public static void ReadMemory(BinaryReader reader)
        {
            LocalEvents.Clear();
            GlobalEvents.Clear();

            int version = reader.ReadInt32();

            switch (version)
            {
                case 1:
                    int keyCount = reader.ReadInt32();
                    for (var i = 0; i < keyCount; i++)
                    {
                        int key = reader.ReadInt32();
                        int valueCount = reader.ReadInt32();
                        var set = new TriggerSet();

                        for (var a = 0; a <= valueCount; a++)
                        {
                            if (reader.ReadBoolean())
                            {
                                set.Add(a);
                            }

                        }

                        LocalEvents.Add(key, set);
                    }

                    int eventCount = reader.ReadInt32();
                    for (var i = 0; i < eventCount; i++)
                        GlobalEvents.Add(reader.ReadInt32());
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
