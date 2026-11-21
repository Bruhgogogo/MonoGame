using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.MonoUtils
{
    public static class Vector2Ex
    {
        public static Vector2 GetAnchorPoint(Vector2 position, Vector2 size, Vector2 anchor)
        {
            float x = position.X + size.X * anchor.X;
            float y = position.Y + size.Y * anchor.Y;
            return new Vector2(x, y);
        }

        public static Vector2 ApplyDeadZone(Vector2 input, float deadZone)
        {
            return new Vector2(
                MathF.Abs(input.X) <= deadZone ? 0f : input.X,
                MathF.Abs(input.Y) <= deadZone ? 0f : input.Y
            );
        }
    }
}