using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;

namespace MonoEngine.Class
{
    public sealed class MonoCollisionActor : ICollisionActor
    {
        static private int internalID = 0;

        public int Id { get; }

        public CollisionShape2D shape;
        public CollisionShape2D Shape => shape;

        public GameObject GO;

        public MonoCollisionActor(CollisionShape2D collisionShape, GameObject go)
        {
            Id = ++internalID;
            shape = collisionShape;
            GO = go;
        }
    }

    public class Collider : Behaviour
    {
        private bool isStatic = true;
        public bool IsStatic 
        { 
            get
            {
                return isStatic;
            }
            set
            {
                if (Started) return;
                isStatic = value;
            }
        }

        public Transform transform;

        protected MonoCollisionActor collisionActor;

        protected Vector2 lastPosition;
        protected Vector2 lastScale;
        protected float lastRotation;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
            RecordLastTransform();

            collisionActor = new MonoCollisionActor(BuildCollisionShape(), GO);

            PhysicalEngine.MonoColliders.Add(this);
        }

        public override void Start()
        {
            PhysicalEngine.CollisionWorld.Insert(collisionActor, IsStatic ? PhysicalEngine.StaticLayerName : PhysicalEngine.MoveableLayerName);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            if (IsStatic) return;
            if (IsTransformChanged())
            {
                RecordLastTransform();
                collisionActor.shape = BuildCollisionShape();
            }
        }

        protected override void OnDestroy()
        {
            if (PhysicalEngine.CollisionWorld.Contains(collisionActor))
                PhysicalEngine.CollisionWorld.Remove(collisionActor);
        }

        protected bool IsTransformChanged()
        {
            return lastPosition != transform.Position || lastScale != transform.Scale || lastRotation != transform.Rotation;
        }

        protected void RecordLastTransform()
        {
            lastPosition = transform.Position;
            lastScale = transform.Scale;
            lastRotation = transform.Rotation;
        }

        public bool IntersectsWith(Collider otherCollider)
        {
            return GetShape().Intersects(otherCollider.GetShape());
        }

        public bool IntersectsWith(Line2D line2D)
        {
            return GetShape().Intersects(line2D);
        }

        public bool IntersectsWith(LineSegment2D lineSegment2D)
        {
            return GetShape().Intersects(lineSegment2D);
        }

        public bool IntersectsWith(Ray2D ray2D)
        {
            return GetShape().Intersects(ray2D);
        }

        public bool IntersectsWith(LineSegment2D lineSegment2D, out float? TimeMin, out float? TimeMax)
        {
            return GetShape().Intersects(lineSegment2D, out TimeMin, out TimeMax);
        }

        public bool IntersectsWith(Ray2D ray2D, out float? TimeMin, out float? TimeMax)
        {
            return GetShape().Intersects(ray2D, out TimeMin, out TimeMax);
        }

        public bool BoundingBoxIntersectsWith(Collider otherCollider)
        {
            return GetShape().BoundingBox.Intersects(otherCollider.GetShape().BoundingBox);
        }

        public bool BoundingBoxIntersectsWith(LineSegment2D lineSegment2D)
        {
            return GetShape().BoundingBox.Intersects(lineSegment2D.GetBounds());
        }

        public bool ResovleCollision(Collider otherCollider, out CollisionResult2D result)
        {
            return GetShape().TryGetCollision(otherCollider.GetShape(), out result);
        }

        public bool ResovleBoundingBoxCollision(Collider otherCollider, out CollisionResult2D result)
        {
            return GetShape().BoundingBox.TryGetCollision(otherCollider.GetShape().BoundingBox, out result);
        }

        public CollisionShape2D GetShape()
        {
            return collisionActor.Shape;
        }

        public virtual CollisionShape2D BuildCollisionShape() { throw new NotSupportedException("MonoEngine: Attempt to call BuildCollisionShape on base Collider"); }
    }

    public class ColliderBox : Collider
    {
        public override CollisionShape2D BuildCollisionShape()
        {
            float c = MathF.Cos(transform.Rotation);
            float s = MathF.Sin(transform.Rotation);

            Vector2 center = transform.Position;
            Vector2 axisX = new Vector2(c, s);
            Vector2 axisY = new Vector2(-s, c);
            Vector2 halfExtent = transform.Scale * 0.5f;

            OrientedBoundingBox2D OBB = new OrientedBoundingBox2D(center, axisX, axisY, halfExtent);
            return new CollisionShape2D(OBB);
        }
    }
}