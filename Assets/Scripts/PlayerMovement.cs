using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        public bool Grounded => _groundCollisions.Count > 0;
        
        [SuffixLabel("m/s")]
        [LabelText("Top Speed")]
        [MinValue(0)]
        [SerializeField]
        private float _moveSpeed;

        [SuffixLabel("m/s²")]
        [PropertyRange(0, nameof(_moveSpeed))]
        [SerializeField]
        private float _stationaryAcceleration;

        [SuffixLabel("Ns")]
        [MinValue(0)]
        [SerializeField]
        private float _jumpForce;

        [PropertyRange(0, 1)]
        [SerializeField]
        private float _airDeadZone;

        [BoxGroup("Ground Check")]
        [LabelText("Maximum Distance")]
        [SuffixLabel("m")]
        [MinValue(0)]
        [SerializeField]
        private float _groundCheckMaxDistance;

        [BoxGroup("Ground Check")]
        [LabelText("Radius")]
        [SuffixLabel("m")]
        [MinValue(0)]
        [SerializeField]
        private float _groundCheckRadius;

        [BoxGroup("Ground Check")]
        [LabelText("Layers")]
        [SerializeField]
        private LayerMask _groundCheckLayers;

        private readonly HashSet<Collider> _groundCollisions = new(32);
        private Rigidbody _rigidbody;
        private Transform _camera;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _camera = Camera.main!.transform;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump") && _groundCollisions.Count > 0)
            {
                _rigidbody.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
            }

            var horizontalMovement = Input.GetAxis("Horizontal");
            var verticalMovement = Input.GetAxis("Vertical");

            var plane = GetMovementPlane();
            var (forward, right) = ComputeTangentSpace(plane.normal);

            var desiredDirection =
                forward * verticalMovement
                + right * horizontalMovement;

            if (_groundCollisions.Count == 0 && desiredDirection.magnitude < _airDeadZone)
            {
                return;
            }
            
            desiredDirection.Normalize();

            var desiredVelocity = desiredDirection * _moveSpeed;
            var delta = plane.ClosestPointOnPlane(desiredVelocity - _rigidbody.velocity);
            
            var force = desiredVelocity + delta * _stationaryAcceleration;

            Debug.DrawRay(transform.position, force, Color.red);
            _rigidbody.AddForce(force, ForceMode.Acceleration);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsGroundCollision(collision))
            {
                _groundCollisions.Add(collision.collider);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            _groundCollisions.Remove(collision.collider);
        }

        private bool IsGroundCollision(Collision collision)
        {
            var contact = collision.GetContact(0);

            var fixNormalOrientation = Vector3.Dot(contact.normal, transform.position - contact.point);

            return
                Vector3.Dot(transform.up, contact.normal) * fixNormalOrientation > 0.1
                && (_groundCheckLayers & 1 << collision.gameObject.layer) > 0;
        }

        private (Vector3 Forward, Vector3 Right) ComputeTangentSpace(Vector3 normal)
        {
            var tangentPlane = new Plane(normal, Vector3.zero);

            var forwardWorldSpace = tangentPlane.ClosestPointOnPlane(_camera.forward);
            var rightWorldSpace = tangentPlane.ClosestPointOnPlane(_camera.right);
            return (forwardWorldSpace, rightWorldSpace);
        }

        private Plane GetMovementPlane()
        {
            const int samples = 7;

            var direction = -transform.up;

            var result = Vector3.zero;
            var count = 0;
            for (var i = 0; i < samples; i++)
            {
                var rotate = Quaternion.AngleAxis(360f / samples * i, direction);
                var start = transform.position + rotate * transform.forward * _groundCheckRadius;
                var found = Physics.Raycast(
                    start,
                    direction,
                    out var hit,
                    _groundCheckMaxDistance,
                    _groundCheckLayers
                );

                if (!found)
                {
                    continue;
                }

                count++;
                result += hit.normal;

                Debug.DrawRay(
                    start,
                    direction * _groundCheckMaxDistance,
                    Color.black
                );
            }

            var normal = count == 0
                ? -direction
                : result / count;

            return new(normal, 0);
        }
    }
}