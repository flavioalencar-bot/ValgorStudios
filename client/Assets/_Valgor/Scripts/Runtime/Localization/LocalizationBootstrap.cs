using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Valgor.Localization
{
    public sealed class LocalizationBootstrap : MonoBehaviour
    {
        public IEnumerator Initialize()
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
                yield return LocalizationSettings.InitializationOperation;
        }
    }
}
