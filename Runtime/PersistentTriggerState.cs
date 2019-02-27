using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.IO;

namespace Aijai.Persistence
{
    public partial class PersistentTriggerState : PersistentTriggerStateBase
    {
        [SerializeField] UnityEvent OnTrigger;
        [SerializeField] UnityEvent OnTriggeredBefore;
        [SerializeField] int m_respawn;
        

        public override void TriggerState()
        {
            OnTrigger.Invoke();
            Persistence.SetState(this, true);
        }

        static LinkedList<PropertyName> SceneHistory = new LinkedList<PropertyName>();

        protected virtual void OnEnable()
        {
            var sceneID = this.gameObject.scene.name;
            if (SceneHistory.First == null || SceneHistory.First.Value != sceneID)
            {
                SceneHistory.AddFirst(sceneID);
                if (SceneHistory.Count > 6)
                    SceneHistory.RemoveLast();
            }
            
            if (Persistence.GetState(this))
            {
                if (m_respawn > 0)
                {
                    var head = SceneHistory.First.Next;
                    int steps = 0;
                    while (head != null)
                    {
                        if (head.Value == sceneID)
                            break;
                        steps++;
                        head = head.Next;
                    }

                    if (steps > m_respawn)
                    {
                        Persistence.SetState(this, false);
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
    


}
