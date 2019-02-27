using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace Aijai.Persistence.Tests
{
    public class NewTestScript
    {

        [UnityTest]
        public IEnumerator GeneratedValuesAreStored()
        {
            Persistence.Purge();
            foreach (var d in GenerateIndices(256, 256))
            {
                Persistence.SetState(d.x, d.y);
            }

            foreach (var d in GenerateIndices(256, 256))
            {
                Assert.IsTrue(Persistence.GetState(d.x, d.y));
            }

            Persistence.Purge();

            foreach (var d in GenerateIndices(256, 256))
            {
                Assert.IsFalse(Persistence.GetState(d.x, d.y));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator CheckpointsOverrideValues()
        {
            Persistence.Purge();
            foreach (var d in GenerateIndices(256, 256))
            {
                Persistence.SetState(d.x, d.y);
            }

            Assert.IsTrue(Persistence.GetState(256, 256));
            Assert.IsFalse(Persistence.GetState(257, 1));

            Persistence.CreateCheckpoint();

            Persistence.SetState(257, 1);

            Assert.IsTrue(Persistence.GetState(257, 1));

            Persistence.ReturnCheckpoint();

            Assert.IsFalse(Persistence.GetState(257, 1));
            foreach (var d in GenerateIndices(256, 256))
            {
                Assert.IsTrue(Persistence.GetState(d.x, d.y));
            }

            yield return null;
        }

        IEnumerable<Vector2Int> GenerateIndices(int Scenes, int Triggers)
        {
            for (var i = 0; i <= Scenes; i ++)
            {
                for (var a = 0; a <= Triggers; a++)
                {
                    yield return new Vector2Int(i, a);
                }
            }
        }
        
    }
}
