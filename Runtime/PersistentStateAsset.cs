using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aijai.Persistence
{
    [CreateAssetMenu(fileName = "Persitent State Asset", menuName = "Aijai/Persistence/Persistent State Asset")]
    public class PersistentStateAsset : ScriptableObject
    {
        [SerializeField] int m_persistentIndex = int.MinValue;

        public bool Poll()
        {
            return Persistence.GetMajorEvent(m_persistentIndex);
        }

        public void Toggle()
        {
            Persistence.SetMajorEvent(m_persistentIndex);
        }

        public int PersistentIndex
        {
            get
            {
                return m_persistentIndex;
            }
        }

        
    }

}
