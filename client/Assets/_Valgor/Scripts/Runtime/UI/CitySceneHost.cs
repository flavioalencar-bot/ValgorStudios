using UnityEngine;
using UnityEngine.UIElements;
using Valgor.City;

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

            if (gameObject.GetComponent<ProvisionalCityBootstrap>() == null)
            {
                gameObject.AddComponent<ProvisionalCityBootstrap>();
            }
        }
    }
}
