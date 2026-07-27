using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    public sealed class CitySceneHost : MonoBehaviour
    {
        private void Awake()
        {
            var document = gameObject.GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            BetaUiPanels.ApplyTo(document);

            var cityBootstrapType = Type.GetType("Valgor.City.CityBootstrap, Valgor.City");
            if (cityBootstrapType == null)
            {
                throw new InvalidOperationException("Valgor.City.CityBootstrap não foi encontrado.");
            }

            if (gameObject.GetComponent(cityBootstrapType) == null)
            {
                gameObject.AddComponent(cityBootstrapType);
            }
        }
    }
}
