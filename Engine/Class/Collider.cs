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
        public Transform transform;

        private Vector2 positionOffset = Vector2.Zero;
        public Vector2 PositionOffset
        {
            get => positionOffset;
            set
            {
                if (positionOffset == value) return;
                positionOffset = value;
                offsetDirty = true;
            }
        }

        private float rotationOffset = 0.0f;
        public float RotationOffset
        {
            get => rotationOffset;
            set
            {
                if (rotationOffset == value) return;
                rotationOffset = value;
                offsetDirty = true;
            }
        }

        private Vector2 scaleOffset = Vector2.One;
        public Vector2 ScaleOffset
        {
            get => scaleOffset;
            set
            {
                if (scaleOffset == value) return;
                scaleOffset = value;
                offsetDirty = true;
            }
        }

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

        public CollisionShape2D Shape
        {
            get
            {
                return collisionActor.shape;
            }
        }

        protected MonoCollisionActor collisionActor;
        public MonoCollisionActor CollisionActor
        {
            get
            {
                return collisionActor;
            }
        }

        protected Vector2 lastPosition;
        protected Vector2 lastScale;
        protected float lastRotation;

        private bool offsetDirty = false;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
            RecordLastTransform();

            collisionActor = new MonoCollisionActor(BuildCollisionShape(), GO);
        }

        public override void Start()
        {
            PhysicalEngine.CollisionWorld.Insert(collisionActor, IsStatic ? PhysicalEngine.STATIC_LAYER_NAME : PhysicalEngine.MOVEABLE_LAYER_NAME);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            if (IsStatic) return;
            if (IsTransformChanged())
            {
                RecordLastTransform();
                collisionActor.shape = BuildCollisionShape();
            }

            if (offsetDirty)
            {
                offsetDirty = false;
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
            return Shape.Intersects(otherCollider.Shape);
        }

        public bool IntersectsWith(Line2D line2D)
        {
            return Shape.Intersects(line2D);
        }

        public bool IntersectsWith(LineSegment2D lineSegment2D)
        {
            return Shape.Intersects(lineSegment2D);
        }

        public bool IntersectsWith(Ray2D ray2D)
        {
            return Shape.Intersects(ray2D);
        }

        public bool IntersectsWith(LineSegment2D lineSegment2D, out float? TimeMin, out float? TimeMax)
        {
            return Shape.Intersects(lineSegment2D, out TimeMin, out TimeMax);
        }

        public bool IntersectsWith(Ray2D ray2D, out float? TimeMin, out float? TimeMax)
        {
            return Shape.Intersects(ray2D, out TimeMin, out TimeMax);
        }

        public bool BoundingBoxIntersectsWith(Collider otherCollider)
        {
            return Shape.BoundingBox.Intersects(otherCollider.Shape.BoundingBox);
        }

        public bool BoundingBoxIntersectsWith(LineSegment2D lineSegment2D)
        {
            return Shape.BoundingBox.Intersects(lineSegment2D.GetBounds());
        }

        public bool ResovleCollision(Collider otherCollider, out CollisionResult2D result)
        {
            return Shape.TryGetCollision(otherCollider.Shape, out result);
        }

        public bool ResovleBoundingBoxCollision(Collider otherCollider, out CollisionResult2D result)
        {
            return Shape.BoundingBox.TryGetCollision(otherCollider.Shape.BoundingBox, out result);
        }

        protected virtual CollisionShape2D BuildCollisionShape() { throw new NotSupportedException("MonoEngine: Attempt to call BuildCollisionShape on base Collider"); }
    }

    public class ColliderBox : Collider
    {
        protected override CollisionShape2D BuildCollisionShape()
        {
            float c = MathF.Cos(transform.Rotation + RotationOffset);
            float s = MathF.Sin(transform.Rotation + RotationOffset);

            Vector2 center = transform.Position + PositionOffset;
            Vector2 axisX = new Vector2(c, s);
            Vector2 axisY = new Vector2(-s, c);
            Vector2 halfExtent = transform.Scale * ScaleOffset * 0.5f;

            OrientedBoundingBox2D OBB = new OrientedBoundingBox2D(center, axisX, axisY, halfExtent);
            return new CollisionShape2D(OBB);
        }
    }
}