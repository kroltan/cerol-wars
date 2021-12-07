using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Kroltan.GrapplingHook
{
    [RequireComponent(typeof(LineRenderer))]
    public class Rope : MonoBehaviour, IEnumerable<Rope.Segment>
    {
        public class Segment
        {
            public Vector3 AttachedNormal { get; }

            public Vector3 Start
            {
                get => _owner._line.GetPosition(_offset + 0);
                set => _owner._line.SetPosition(_offset + 0, value);
            }

            public Vector3 End
            {
                get => _owner._line.GetPosition(_offset + 1);
                set => _owner._line.SetPosition(_offset + 1, value);
            }

            private readonly Rope _owner;
            private readonly int _offset;

            public Segment(Rope rope, Vector3 normal, int offset)
            {
                _owner = rope;
                _offset = offset;
                AttachedNormal = normal;
            }
        }

        private const float DebugDurationSeconds = 2;

        public event Action<Segment> CurrentSegmentChanged;

        [CanBeNull]
        public Segment CurrentSegment
        {
            get
            {
                _segments.TryPeek(out var top);
                return top;
            }
        }

        [SerializeField]
        private LayerMask _collisionLayers;

        private LineRenderer _line;
        private readonly Stack<Segment> _segments = new();

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
        }

        private void OnEnable()
        {
            _line.positionCount = 2;
            _segments.Clear();
            _segments.Push(new(this, Vector3.zero, 0));
            CurrentSegmentChanged?.Invoke(CurrentSegment);
        }

        private void Update()
        {
            var current = CurrentSegment;
            if (current == null)
            {
                return;
            }

            if (Vector3.Dot(current.AttachedNormal, current.End - current.Start) > 0)
            {
                JoinLast();
            }
            else if (Physics.Linecast(current.Start, current.End, out var hit, _collisionLayers))
            {
                var (point, normal) = hit.collider switch
                {
                    MeshCollider mesh => CalculateMeshEdge(mesh, hit, current.Start, current.End),
                    _ => (hit.point, hit.normal)
                };

                Debug.DrawRay(point, normal, Color.black, DebugDurationSeconds);
                var thickness = _line.startWidth * _line.widthMultiplier / 2;
                SplitAt(point + normal * thickness, hit.normal);
            }
        }

        private void SplitAt(Vector3 point, Vector3 normal)
        {
            
            _line.positionCount++;

            var previous = _segments.Peek();
            var current = new Segment(this, normal, _segments.Count)
            {
                Start = point,
                End = previous.End,
            };

            _segments.Push(current);

            previous.End = point;

            CurrentSegmentChanged?.Invoke(current);
        }

        private void JoinLast()
        {
            if (_segments.Count == 1)
            {
                return;
            }

            var previous = _segments.Pop();
            var current = _segments.Peek();
            current.End = previous.End;
            
            _line.positionCount--;
            
            CurrentSegmentChanged?.Invoke(current);
        }

        private static (Vector3 Point, Vector3 Normal) CalculateMeshEdge(
            MeshCollider other,
            RaycastHit hit,
            Vector3 start,
            Vector3 end
        )
        {
            var mesh = other.sharedMesh;

            var localSegment = new LineSegment(
                other.transform.InverseTransformPoint(start),
                other.transform.InverseTransformPoint(end)
            );

            var a = TriangleVertex(mesh, hit.triangleIndex * 3 + 0);
            var b = TriangleVertex(mesh, hit.triangleIndex * 3 + 1);
            var c = TriangleVertex(mesh, hit.triangleIndex * 3 + 2);

            Debug.DrawLine(
                other.transform.TransformPoint(a),
                other.transform.TransformPoint(b),
                Color.blue,
                DebugDurationSeconds
            );
            Debug.DrawLine(
                other.transform.TransformPoint(b),
                other.transform.TransformPoint(c),
                Color.blue,
                DebugDurationSeconds
            );
            Debug.DrawLine(
                other.transform.TransformPoint(c),
                other.transform.TransformPoint(a),
                Color.blue,
                DebugDurationSeconds
            );

            var segmentFromEdge = EdgesOfFace(a, b, c)
                .Select(edge => LineSegment.ConnectAtNearest(edge, localSegment))
                .NonNull()
                .MinBy(delta => Vector3.Distance(delta.Start, delta.End));


            Debug.DrawLine(
                start,
                end,
                Color.red,
                DebugDurationSeconds
            );
            
            Debug.DrawLine(
                other.transform.TransformPoint(segmentFromEdge.Start),
                other.transform.TransformPoint(segmentFromEdge.End),
                Color.cyan,
                DebugDurationSeconds
            );

            var sourceFace = new Plane(a, b, c);

            var tangent = (sourceFace.ClosestPointOnPlane(localSegment.End) - localSegment.Start).normalized;

            return (
                other.transform.TransformPoint(segmentFromEdge.Start),
                other.transform.TransformDirection((sourceFace.normal + tangent).normalized)
            );
        }

        private static IEnumerable<LineSegment> EdgesOfFace(Vector3 a, Vector3 b, Vector3 c)
        {
            yield return new(a, b);
            yield return new(b, c);
            yield return new(c, a);
        }

        private static Vector3 TriangleVertex(Mesh mesh, int index)
        {
            return mesh.vertices[mesh.triangles[index]];
        }

        public IEnumerator<Segment> GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}