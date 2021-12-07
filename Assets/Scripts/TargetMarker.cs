using Sirenix.OdinInspector;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public class TargetMarker : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private BaseSurfaceAim _aim;

        [Required]
        [SerializeField]
        private GameObject _visual;

        [PropertyRange(0, 1)]
        [SerializeField]
        private float _convergence;

        private void Update()
        {
            var active = _aim.Target.HasValue;

            _visual.gameObject.SetActive(active);

            if (!active)
            {
                return;
            }

            var target = _aim.Target.Value;

            var point = Vector3.Dot(target.normal, _visual.transform.up) > 0.9
                ? Vector3.Lerp(_visual.transform.position, target.point, _convergence)
                : target.point;

            _visual.transform.position = point;
            _visual.transform.up = target.normal;
        }
    }
}