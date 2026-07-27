using System.Collections;
using UnityEngine;

namespace Valgor.Localization
{
    /// <summary>
    /// Bootstrap de localização da Beta 0.1.
    /// PT-BR fixo via strings embutidas — sem catálogo Addressables / remoto.
    /// O pacote Localization permanece no projeto para evolução futura.
    /// </summary>
    public sealed class LocalizationBootstrap
    {
        public const string BetaLocaleCode = "pt-BR";

        public bool IsReady { get; private set; }
        public string ActiveLocaleCode { get; private set; } = BetaLocaleCode;

        public IEnumerator Initialize()
        {
            // Não chama LocalizationSettings / Addressables nesta beta:
            // evita InvalidKeyException e warnings de catálogo ausente.
            ActiveLocaleCode = BetaLocaleCode;
            IsReady = true;
            yield break;
        }
    }
}
