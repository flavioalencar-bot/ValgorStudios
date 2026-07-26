using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valgor.UI
{
    public sealed class CitySceneHost : MonoBehaviour
    {
        private void Awake()
        {
            if (gameObject.GetComponent<UIDocument>() == null)
            {
                gameObject.AddComponent<UIDocument>();
            }

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
