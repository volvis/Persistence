using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using L = System.Linq;
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
        
    }


}
