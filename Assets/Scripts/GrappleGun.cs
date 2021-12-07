using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public class GrappleGun : MonoBehaviour
    {
        public event Action<Vector3, Vector3> Attached;
        public event Action Detached;

        public Vector3? AttachedOnWorld => _rope.gameObject.activeSelf
            ? _rope.CurrentSegment?.Start
            : null;

        [Required]
        [SerializeField]
        private Rope _rope;

        [Required]
        [SerializeField]
        private Transform _ropeAttachment;

        [Required]
        [SerializeField]
        private BaseSurfaceAim _aim;
        
        [Required]
        [SerializeField]
        private SpringJoint _joint;

        [MinValue(0)]
        [SerializeField]
        private float _retractionSpringValue;

        [SerializeField]
        private Projectile _projectile;
        
        private Rope.Segment _firstSegment;

        // https://forum.unity.com/threads/enable-disable-a-joint.24525/#post-3414063
        private float _jointActiveSpringValue;

        public void Attach(Vector3 point, Vector3 normal)
        {
            var startPoint = point;
            if (_projectile)
            {
                startPoint += normal * _projectile.RopeOffset;
            }

            _rope.gameObject.SetActive(true);

            _firstSegment = _rope.CurrentSegment!;
            _firstSegment.Start = startPoint;
            _firstSegment.End = _ropeAttachment.transform.position;

            if (_projectile)
            {
                _projectile.transform.position = point + normal * _projectile.AttachmentOffset;
                _projectile.transform.rotation = Quaternion.LookRotation(normal);
                _projectile.gameObject.SetActive(true);
            }

            _joint.connectedAnchor = _firstSegment.Start;
            _joint.spring = _jointActiveSpringValue;
            
            Attached?.Invoke(point, normal);
        }

        public void Detach()
        {
            _rope.gameObject.SetActive(false);
            _joint.spring = 0;

            if (_projectile)
            {
                _projectile.gameObject.SetActive(false);
            }
            
            Detached?.Invoke();
        }

        private void Awake()
        {
            _jointActiveSpringValue = _joint.spring;
            _rope.CurrentSegmentChanged += segment => _joint.connectedAnchor = segment.Start;

            _projectile.transform.parent = null;
        }

        private void OnDestroy()
        {
            Destroy(_projectile);
        }

        private void OnEnable()
        {
            Detach();
        }

        private void Update()
        {
            var target = _aim.Target;

            var current = _rope.CurrentSegment;
            if (current != null)
            {
                current.End = _ropeAttachment.transform.position;
            }

            if (Input.GetButton("Launch") && target.HasValue && !_rope.gameObject.activeSelf)
            {
                Attach(target.Value.point, target.Value.normal);
            }

            if (Input.GetButtonUp("Launch"))
            {
                Detach();
            }

            if (_rope.gameObject.activeSelf)
            {
                _joint.spring = Input.GetButton("Retract")
                    ? _retractionSpringValue
                    : _jointActiveSpringValue;
            }
        }
    }
}