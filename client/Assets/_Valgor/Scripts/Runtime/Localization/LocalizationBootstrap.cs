using System.Collections;
using UnityEngine;

namespace Valgor.Localization
{
    /// <summary>
    /// Bootstrap de localização. Na beta sem catálogo Addressables,
    /// NÃO chama LocalizationSettings (evita InvalidKeyException / console vermelho).
    /// Strings usam fallbacks em código.
    /// </summary>
    public sealed class LocalizationBootstrap
    {
        public bool IsReady { get; private set; }

        public IEnumerator Initialize()
        {
            // Sem StreamingAssets/aa nem Locale Addressables — pular init do package Localization.
            // Chamar LocalizationSettings.InitializationOperation dispara Addressables.InitializeAsync
            // e InvalidKeyException (NyxSystemCollection / SpecialLocaleSelector).
            Debug.LogWarning(
                "[Valgor.Localization] Catálogo Addressables ausente — usando strings embutidas. " +
                "Localization package não será inicializado nesta build.");
            IsReady = true;
            yield break;
        }
    }
}
