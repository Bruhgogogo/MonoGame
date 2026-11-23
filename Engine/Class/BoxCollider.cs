using Microsoft.Xna.Framework;
using System;

namespace MonoEngine.Class
{
    public sealed class BoxCollider : Collider
    {
        public Transform transform;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
        }

        public override AABB GetAABB()
        {
            float cos = MathF.Cos(transform.Rotation);
            float sin = MathF.Sin(transform.Rotation);
            Vector2 rotatedOffset = new Vector2(
                Offset.X * cos - Offset.Y * sin,
                Offset.X * sin + Offset.Y * cos
            );
            Vector2 center = transform.Position + rotatedOffset;
            Vector2 size = transform.Scale;

            if (transform.Rotation == 0f)
            {
                return new AABB(center - size * 0.5f, center + size * 0.5f);
            }

            float halfW = size.X * 0.5f;
            float halfH = size.Y * 0.5f;

            float x1 = cos * (-halfW) - sin * (-halfH) + center.X;
            float y1 = sin * (-halfW) + cos * (-halfH) + center.Y;
            float x2 = cos * (halfW) - sin * (-halfH) + center.X;
            float y2 = sin * (halfW) + cos * (-halfH) + center.Y;
            float x3 = cos * (halfW) - sin * (halfH) + center.X;
            float y3 = sin * (halfW) + cos * (halfH) + center.Y;
            float x4 = cos * (-halfW) - sin * (halfH) + center.X;
            float y4 = sin * (-halfW) + cos * (halfH) + center.Y;

            float minX = MathF.Min(x1, MathF.Min(x2, MathF.Min(x3, x4)));
            float minY = MathF.Min(y1, MathF.Min(y2, MathF.Min(y3, y4)));
            float maxX = MathF.Max(x1, MathF.Max(x2, MathF.Max(x3, x4)));
            float maxY = MathF.Max(y1, MathF.Max(y2, MathF.Max(y3, y4)));

            return new AABB(new Vector2(minX, minY), new Vector2(maxX, maxY));
        }

        public override Vector2 ClosestPoint(Vector2 point)
        {
            float cos = MathF.Cos(-transform.Rotation);
            float sin = MathF.Sin(-transform.Rotation);
            Vector2 localPoint = point - transform.Position;
            float localX = localPoint.X * cos - localPoint.Y * sin - Offset.X;
            float localY = localPoint.X * sin + localPoint.Y * cos - Offset.Y;

            Vector2 halfSize = transform.Scale * 0.5f;
            float clampedX = MathHelper.Clamp(localX, -halfSize.X, halfSize.X);
            float clampedY = MathHelper.Clamp(localY, -halfSize.Y, halfSize.Y);

            cos = MathF.Cos(transform.Rotation);
            sin = MathF.Sin(transform.Rotation);
            float worldX = clampedX * cos - clampedY * sin + transform.Position.X;
            float worldY = clampedX * sin + clampedY * cos + transform.Position.Y;

            return new Vector2(worldX, worldY);
        }

        public override bool Contains(Vector2 point)
        {
            float cos = MathF.Cos(-transform.Rotation);
            float sin = MathF.Sin(-transform.Rotation);
            Vector2 localPoint = point - transform.Position;
            float localX = localPoint.X * cos - localPoint.Y * sin - Offset.X;
            float localY = localPoint.X * sin + localPoint.Y * cos - Offset.Y;

            Vector2 halfSize = transform.Scale * 0.5f;
            return localX >= -halfSize.X && localX <= halfSize.X &&
                   localY >= -halfSize.Y && localY <= halfSize.Y;
        }
    }
}