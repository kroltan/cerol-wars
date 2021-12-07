using System.Collections;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Kroltan.GrapplingHook
{
    public class DeathMarker : NetworkBehaviour
    {
        [Required]
        [SerializeField]
        private DecalProjector _decal;

        [SuffixLabel("x = seconds")]
        [SerializeField]
        private AnimationCurve _fade;
        
        private IEnumerator Start()
        {
            _decal.transform.Rotate(0, Random.Range(0, 360), 0);

            for (var startTime = Time.time; Time.time < startTime + _fade.length;)
            {
                _decal.fadeFactor = _fade.Evaluate(Time.time - startTime);
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}