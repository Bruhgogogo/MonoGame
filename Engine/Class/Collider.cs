using Microsoft.Xna.Framework;

namespace MonoEngine.Class
{
    public abstract class Collider : Component
    {
        public bool IsTrigger = false;

        public Vector2 Offset = Vector2.Zero;

        public abstract AABB GetAABB();

        public abstract Vector2 ClosestPoint(Vector2 position);

        public abstract bool Contains(Vector2 point);
    }

    public struct AABB
    {
        public Vector2 Min;
        public Vector2 Max;

        public Vector2 Center => (Min + Max) * 0.5f;
        public Vector2 Size => Max - Min;

        public AABB(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public AABB(Vector2 center, float halfWidth, float halfHeight)
        {
            Min = new Vector2(center.X - halfWidth, center.Y - halfHeight);
            Max = new Vector2(center.X + halfWidth, center.Y + halfHeight);
        }

        public bool Overlaps(AABB other)
        {
            return Min.X < other.Max.X && Max.X > other.Min.X &&
                   Min.Y < other.Max.Y && Max.Y > other.Min.Y;
        }

        public AABB Union(AABB other)
        {
            return new AABB
            {
                Min = new Vector2(MathHelper.Min(Min.X, other.Min.X), MathHelper.Min(Min.Y, other.Min.Y)),
                Max = new Vector2(MathHelper.Max(Max.X, other.Max.X), MathHelper.Max(Max.Y, other.Max.Y))
            };
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y;
        }

        public Vector2 ClosestPoint(Vector2 point)
        {
            return new Vector2(
                MathHelper.Clamp(point.X, Min.X, Max.X),
                MathHelper.Clamp(point.Y, Min.Y, Max.Y)
            );
        }
    }
}