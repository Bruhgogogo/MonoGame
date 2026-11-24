using Microsoft.Xna.Framework;
using MonoEngine.Class;
using System;

namespace MonoEngine.MonoUtils
{
    public static class Vector2Ex
    {
        public static Vector2 ApplyDeadZone(Vector2 input, float deadZone)
        {
            return new Vector2(
                MathF.Abs(input.X) <= deadZone ? 0f : input.X,
                MathF.Abs(input.Y) <= deadZone ? 0f : input.Y
            );
        }

        public static Vector2 MoveToward(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            Vector2 direction = target - current;
            float distance = direction.Length();

            if (distance <= maxDistanceDelta || distance == 0f)
                return target;

            return current + direction / distance * maxDistanceDelta;
        }

        public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            Vector2 change = current - target;
            Vector2 originalTo = target;

            float maxChange = maxSpeed * smoothTime;
            float changeLength = change.Length();
            if (changeLength > maxChange)
            {
                change = change / changeLength * maxChange;
            }

            target = current - change;
            Vector2 temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            Vector2 output = target + (change + temp) * exp;

            if (Vector2.Dot(originalTo - current, output - originalTo) > 0f)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float deltaTime)
        {
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, deltaTime);
        }

        public static Vector2 ScreenToWorld(Vector2 screenPos, Camera camera)
        {
            Vector2 offset = screenPos - MonoEngine.ViewportSize * 0.5f;
            return camera.transform.Position + offset / camera.transform.Scale;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return new Vector2(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 value)
        {
            Vector2 result = Vector2.Zero;
            if (b.X != a.X)
                result.X = MathHelper.Clamp((value.X - a.X) / (b.X - a.X), 0f, 1f);
            if (b.Y != a.Y)
                result.Y = MathHelper.Clamp((value.Y - a.Y) / (b.Y - a.Y), 0f, 1f);
            return result;
        }

        public static Vector2 Reflect(Vector2 direction, Vector2 normal)
        {
            return direction - 2f * Vector2.Dot(direction, normal) * normal;
        }

        public static Vector2 Rotate(Vector2 vector, float angleRadians)
        {
            float cos = MathF.Cos(angleRadians);
            float sin = MathF.Sin(angleRadians);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }

        public static Vector2 RotateAround(Vector2 point, Vector2 pivot, float angleRadians)
        {
            Vector2 dir = point - pivot;
            float cos = MathF.Cos(angleRadians);
            float sin = MathF.Sin(angleRadians);
            return new Vector2(
                pivot.X + dir.X * cos - dir.Y * sin,
                pivot.Y + dir.X * sin + dir.Y * cos
            );
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            return MathF.Atan2(to.Y - from.Y, to.X - from.X);
        }

        public static float SignedAngle(Vector2 from, Vector2 to)
        {
            return MathF.Atan2(
                from.X * to.Y - from.Y * to.X,
                from.X * to.X + from.Y * to.Y
            );
        }

        public static Vector2 DirectionTo(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            if (dir != Vector2.Zero)
                dir.Normalize();
            return dir;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        public static float SqrDistance(Vector2 a, Vector2 b)
        {
            return Vector2.DistanceSquared(a, b);
        }

        public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
        {
            float sqrMag = vector.LengthSquared();
            if (sqrMag > maxLength * maxLength)
            {
                float mag = MathF.Sqrt(sqrMag);
                return vector / mag * maxLength;
            }
            return vector;
        }

        public static Vector2 PerpendicularClockwise(Vector2 vector)
        {
            return new Vector2(vector.Y, -vector.X);
        }

        public static Vector2 PerpendicularCounterClockwise(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }

        public static Vector2 Project(Vector2 vector, Vector2 onto)
        {
            float dot = Vector2.Dot(vector, onto);
            float ontoLenSq = onto.LengthSquared();
            if (ontoLenSq == 0f) return Vector2.Zero;
            return onto * (dot / ontoLenSq);
        }

        public static Vector2 Reject(Vector2 vector, Vector2 onto)
        {
            return vector - Project(vector, onto);
        }

        public static Vector2 Midpoint(Vector2 a, Vector2 b)
        {
            return (a + b) * 0.5f;
        }

        public static bool Approximately(Vector2 a, Vector2 b, float tolerance = 0.001f)
        {
            return Vector2.DistanceSquared(a, b) <= tolerance * tolerance;
        }

        public static Vector2 SmoothStep(Vector2 a, Vector2 b, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            t = t * t * (3f - 2f * t);
            return new Vector2(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        public static Vector2 CatmullRom(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * v1) +
                (-v0 + v2) * t +
                (2f * v0 - 5f * v1 + 4f * v2 - v3) * t2 +
                (-v0 + 3f * v1 - 3f * v2 + v3) * t3
            );
        }

        public static Vector2 Hermite(Vector2 v1, Vector2 t1, Vector2 v2, Vector2 t2, float s)
        {
            float s2 = s * s;
            float s3 = s2 * s;
            float h1 = 2f * s3 - 3f * s2 + 1f;
            float h2 = -2f * s3 + 3f * s2;
            float h3 = s3 - 2f * s2 + s;
            float h4 = s3 - s2;
            return h1 * v1 + h2 * v2 + h3 * t1 + h4 * t2;
        }

        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }
    }
}