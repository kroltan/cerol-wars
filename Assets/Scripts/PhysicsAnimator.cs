using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    [RequireComponent(typeof(Animator))]
    public class PhysicsAnimator : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private PlayerMovement _player;

        [Required]
        [SerializeField]
        private GrappleGun _gun;

        [Required]
        [SerializeField]
        private Transform _camera;
        
        [BoxGroup("Heading")]
        [SuffixLabel("degrees/s")]
        [PropertyRange(0, 90)]
        [SerializeField]
        private float _turnSpeed;

        [BoxGroup("Parameters")]
        [ValueDropdown(nameof(Parameters))]
        [SuffixLabel("float")]
        [SerializeField]
        private int _speed;
        
        [BoxGroup("Parameters")]
        [ValueDropdown(nameof(Parameters))]
        [SuffixLabel("float")]
        [SerializeField]
        private int _horizontal;

        [BoxGroup("Parameters")]
        [ValueDropdown(nameof(Parameters))]
        [SuffixLabel("float")]
        [SerializeField]
        private int _vertical;

        [BoxGroup("Parameters")]
        [ValueDropdown(nameof(Parameters))]
        [SuffixLabel("boolean")]
        [SerializeField]
        private int _jumping;

        [BoxGroup("Parameters")]
        [ValueDropdown(nameof(Parameters))]
        [SuffixLabel("boolean")]
        [SerializeField]
        private int _grappling;

        [BoxGroup("Parameters")]
        [ValueDropdown(nameof(Parameters))]
        [SuffixLabel("boolean")]
        [SerializeField]
        private int _grapplePosition;

        [BoxGroup("Swinging")]
        [SerializeField]
        private AnimationCurve _swingDotCurve = AnimationCurve.Linear(-1, 0, 1, 1);

        [BoxGroup("Swinging")]
        [SuffixLabel("units/s")]
        [MinValue(0)]
        [SerializeField]
        private float _swingChangeRate;

        private Animator _animator;
        private Rigidbody _rigidbody;
        private Vector3 _previousVelocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponentInParent<Rigidbody>();
        }

        private void Update()
        {
            var targetDirection = new Plane(transform.up, Vector3.zero).ClosestPointOnPlane(_camera.forward);
            var targetRotation = Quaternion.LookRotation(targetDirection, transform.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed);

            var movementDirection = _rigidbody.velocity.normalized;

            _animator.SetFloat(_speed, _rigidbody.velocity.magnitude);
            _animator.SetFloat(_horizontal, Vector3.Dot(transform.right, movementDirection));
            _animator.SetFloat(_vertical, Vector3.Dot(transform.forward, movementDirection));
            _animator.SetBool(_jumping, !_player.Grounded);
            _animator.SetBool(_grappling, _gun.AttachedOnWorld.HasValue);

            if (_gun.AttachedOnWorld.HasValue)
            {
                var dot = Vector3.Dot((_rigidbody.velocity - _previousVelocity).normalized, transform.forward);
                var current = Mathf.MoveTowards(
                    _animator.GetFloat(_grapplePosition),
                    _swingDotCurve.Evaluate(dot),
                    _swingChangeRate * Time.deltaTime
                );
                
                _animator.SetFloat(_grapplePosition, current);
            }

            _previousVelocity = _rigidbody.velocity;
        }

        private IEnumerable<ValueDropdownItem<int>> Parameters()
        {
            return GetComponent<Animator>()
                .parameters
                .Select(p => new ValueDropdownItem<int>(p.name, p.nameHash))
                .Prepend(new("<None>", 0));
        }
    }
}