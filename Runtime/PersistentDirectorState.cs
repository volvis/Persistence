using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Aijai.Persistence
{
    [RequireComponent(typeof(PlayableDirector))]
    public class PersistentDirectorState : PersistentTriggerStateBase
    {
        public override void TriggerState()
        {
            Persistence.SetState(this, true);
            GetComponent<PlayableDirector>().Play();
        }

        private void OnEnable()
        {
            if (Persistence.GetState(this))
            {
                var d = GetComponent<PlayableDirector>();
                d.time = d.playableAsset.duration;
                d.Evaluate();
            }
        }

        private void OnValidate()
        {
            m_saveIndex = Mathf.Max(0, m_saveIndex);
            //GetComponent<PlayableDirector>().playOnAwake = false;
        }
    }

}
