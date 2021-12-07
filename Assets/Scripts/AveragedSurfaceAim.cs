using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public class AveragedSurfaceAim : BaseSurfaceAim
    {
        private const float Phi = 0.61803398875f;

        public override RaycastHit? Target => _target;
        
        [SerializeField]
        private LayerMask _targetLayers;

        [MinValue(0)]
        [SerializeField]
        private float _maxDistance;

        [PropertyRange(0, 1)]
        [SerializeField]
        private float _radiusPower;

        [MinValue(0)]
        [SerializeField]
        private float _radius;
        
        [MinValue(0)]
        [SerializeField]
        private int _samples;

        [MinValue(0)]
        [SerializeField]
        private float _startBias;

        private RaycastHit? _target;
        private readonly List<RaycastHit> _results = new();

        private void Update()
        {
            _results.Clear();

            Color4 hitColor = Color.magenta;
            
            foreach (var direction in GenerateSampleDirections())
            {
                var origin = Camera.main!.transform.position + direction * _startBias;
                if (!Physics.Raycast(origin, direction, out var hit, _maxDistance, _targetLayers))
                {
                    continue;
                }

                Debug.DrawLine(origin, hit.point, hitColor with { A = 0.5f });
                    
                _results.Add(hit);
            }

            var target = _results
                .GroupBy(hit => Vector3Int.RoundToInt(hit.normal * 100))
                .MaxBy(g => g.Count());

            if (target == null)
            {
                _target = null;
                return;
            }

            Color4 coplanarColor = Color.red;

            foreach (var hit in target)
            {
                Debug.DrawRay(hit.point, hit.normal / 2, coplanarColor with { A = 0.5f });
            }

            var result = target.First();
            result.point = target.Average(hit => hit.point);

            _target = result;
            
            Debug.DrawRay(result.point, result.normal, coplanarColor);
        }

        private IEnumerable<Vector3> GenerateSampleDirections()
        {
            var center = transform.forward;
            var piece = transform.right * (_radius / _samples);

            for (var i = 0; i < _samples; i++)
            {
                var rotation = Quaternion.AngleAxis(360 * Phi * i, center);
                yield return center + rotation * piece * Mathf.Pow(i, _radiusPower);
            }
        }
    }
}