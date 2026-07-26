using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Valgor.Heroes.SpecialPowers;

namespace Valgor.Heroes.Tests.PlayMode
{
    public sealed class SpecialPowerPlayModeTests
    {
        [UnityTest]
        public IEnumerator Runtime_Controller_Starts_Ready()
        {
            var go = new GameObject("SpecialPowerTest");
            var controller = go.AddComponent<SpecialPowerController>();
            yield return null;
            Assert.AreEqual(Data.SpecialPowerState.Ready, controller.Evaluate(0));
            Object.Destroy(go);
        }
    }
}
