using UnityEngine;

namespace Kroltan.GrapplingHook
{
    public readonly struct LineSegment
    {
        public Vector3 Start { get; }

        public Vector3 Direction { get; }

        public float Length { get; }

        public Vector3 End => PointAt(Length);

        public LineSegment(Vector3 from, Vector3 to)
        {
            var delta = to - from;
            var length = delta.magnitude;

            Start = from;   
            Direction = delta / length;
            Length = length;
        }

        public Vector3 PointAt(float distance)
        {
            return Start + Direction * Mathf.Clamp(distance, 0, Length);
        }

        public Vector3 ClosestPoint(Vector3 point)
        {
            return PointAt(Vector3.Dot(Direction, point - Start));
        }

        public static LineSegment? ConnectAtNearest(LineSegment segmentA, LineSegment segmentB)
        {
            // Magic
            // https://gamedev.stackexchange.com/q/9738/40474

            var a = Vector3.Dot(segmentA.Direction, segmentA.Direction);
            var b = Vector3.Dot(segmentA.Direction, segmentB.Direction);
            var e = Vector3.Dot(segmentB.Direction, segmentB.Direction);

            var d = a * e - b * b;
            if (d == 0)
            {
                return null;
            }

            var r = segmentA.Start - segmentB.Start;
            var c = Vector3.Dot(segmentA.Direction, r);
            var f = Vector3.Dot(segmentB.Direction, r);

            var s = (b * f - c * e) / d;
            var t = (a * f - b * c) / d;
            
            return new(
                segmentA.PointAt(s),
                segmentB.PointAt(t)
            );
        }

        public static float Distance(LineSegment lineSegment, Vector3 point)
        {
            return Vector3.Distance(point, lineSegment.ClosestPoint(point));
        }

        public static float Distance(LineSegment a, LineSegment b)
        {
            var cross = Vector3.Cross(b.Direction, a.Direction);
            var denominator = Vector3.Dot(cross, b.Start - a.Start);
            var divisor = cross.magnitude;

            return Mathf.Abs(denominator / divisor);
        }
        
        public static implicit operator Ray(LineSegment segment)
        {
            return new(segment.Start, segment.Direction);
        }
    }
}