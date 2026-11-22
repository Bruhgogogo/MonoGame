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

        public static Vector2 ScreenToWorld(Vector2 screenPos, Camera camera)
        {
            Vector2 offset = screenPos - MonoEngine.ViewportSize * 0.5f;
            return camera.Transform.Position + offset / camera.Transform.Scale;
        }
    }
}