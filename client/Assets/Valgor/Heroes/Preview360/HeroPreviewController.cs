using UnityEngine;

namespace Valgor.Heroes.Preview360
{
    /// <summary>
    /// Simple orbit preview. Replace dummy mesh with Addressables hero prefab when available.
    /// </summary>
    public sealed class HeroPreviewController : MonoBehaviour
    {
        [SerializeField] private Transform previewRoot;
        [SerializeField] private float rotationSpeedDegrees = 35f;
        [SerializeField] private bool autoRotate = true;

        private float _yaw;

        private void Update()
        {
            if (previewRoot == null) return;

            if (Input.GetMouseButton(0))
            {
                _yaw += Input.GetAxis("Mouse X") * rotationSpeedDegrees * Time.deltaTime * 10f;
            }
            else if (autoRotate)
            {
                _yaw += rotationSpeedDegrees * Time.deltaTime;
            }

            previewRoot.localRotation = Quaternion.Euler(0f, _yaw, 0f);
        }

        public void SetPreviewTarget(Transform target)
        {
            previewRoot = target;
            _yaw = 0f;
        }
    }
}
