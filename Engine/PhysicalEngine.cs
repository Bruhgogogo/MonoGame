using Microsoft.Xna.Framework;
using MonoEngine.Class;
using MonoEngine.MonoUtils;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using System;
using System.Collections.Generic;

namespace MonoEngine
{
    public enum RaycastFilter
    {
        Exclude,
        Include,
    }

    public struct RaycastResult
    {
        public Collider ColliderComponent;
        public Vector2 ContactPoint;
        public Vector2 Normal;
        public Vector2 Direction;
        public float Distance;
        public float Fraction;
        public bool Hit;
    }
    
    public struct RaycastParams
    {
        public HashSet<Collider> FilterColliders;
        public RaycastFilter FilterType;

        public RaycastParams(HashSet<Collider> filterColliders, RaycastFilter filterType)
        {
            FilterColliders = filterColliders;
            FilterType = filterType;
        }
    }
    public sealed class PhysicalEngine
    {
        static public CollisionWorld2D CollisionWorld { get; private set; }
        static public List<Collider> MonoColliders { get; private set; }

        public const string STATIC_LAYER_NAME = "StaticLayer";
        public const string MOVEABLE_LAYER_NAME = "MoveableLayer";

        private readonly SizeF STATIC_LAYER_GRID_SIZE = new SizeF(256.0f, 256.0f);
        private readonly SizeF MOVEABLE_LAYER_GRID_SIZE = new SizeF(256.0f, 256.0f);

        public PhysicalEngine()
        {
            CollisionWorld = new CollisionWorld2D();
            
            SpatialHash staticSpatial = new SpatialHash(STATIC_LAYER_GRID_SIZE);
            SpatialHash moveableSpatial = new SpatialHash(MOVEABLE_LAYER_GRID_SIZE);

            Layer staticLayer = new Layer(staticSpatial) { IsDynamic = false };
            Layer moveableLayer = new Layer(staticSpatial) { IsDynamic = true };

            CollisionWorld.AddLayer(STATIC_LAYER_NAME, staticLayer);
            CollisionWorld.AddLayer(MOVEABLE_LAYER_NAME, moveableLayer);
            CollisionWorld.EnableCollisionBetweenLayers(STATIC_LAYER_NAME, MOVEABLE_LAYER_NAME);
            CollisionWorld.EnableCollisionBetweenLayers(MOVEABLE_LAYER_NAME, MOVEABLE_LAYER_NAME);

            MonoColliders = new List<Collider>(MonoEngine.DEFAULT_COLLIDER_LIST_SIZE);
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction, RaycastParams raycastParams)
        {
            LineSegment2D lineSegment = new LineSegment2D(origin, origin + direction);
            RaycastResult raycastResult = new RaycastResult();
            raycastResult.Hit = false;
            raycastResult.ColliderComponent = null;
            raycastResult.ContactPoint = Vector2.Zero;
            raycastResult.Normal = Vector2.Zero;
            raycastResult.Direction = Vector2.Zero;
            raycastResult.Distance = 0.0f;
            raycastResult.Fraction = 0.0f;

            Collider bestCollider = null;

            float bestEnterTime = float.MaxValue;
            float bestExitTime = float.MaxValue;

            for (int i = MonoColliders.Count - 1; i >= 0; i--)
            {
                Collider collider = MonoColliders[i];

                if (!collider)
                {
                    MonoColliders.SwapAndRemove(i);
                    continue;
                }

                bool shouldSkip = raycastParams.FilterType switch
                {
                    RaycastFilter.Include => !raycastParams.FilterColliders.Contains(collider),
                    RaycastFilter.Exclude => raycastParams.FilterColliders.Contains(collider),
                    _ => false
                };
                if (shouldSkip) continue;
                if (!collider.BoundingBoxIntersectsWith(lineSegment)) continue;
                if (!collider.IntersectsWith(lineSegment, out float? enterTime, out float? exitTime)) continue;

                float currentEnterTime = (float)enterTime;

                if (currentEnterTime < bestEnterTime)
                {
                    bestEnterTime = currentEnterTime;
                    bestExitTime = (float)exitTime;
                    bestCollider = collider;
                }
            }

            if (bestCollider)
            {
                Vector2 enterPoint = lineSegment.GetPoint(bestEnterTime);
                Vector2 exitPoint = lineSegment.GetPoint(bestExitTime);
                Vector2 center = (enterPoint + exitPoint) * 0.5f;

                Vector2 rayDirection = lineSegment.Direction;
                Vector2 lateralAxis = new Vector2(-rayDirection.Y, rayDirection.X);

                CollisionShape2D bestColliderShape = bestCollider.GetShape();
                Vector2 boundsSize = bestColliderShape.BoundingBox.Max - bestColliderShape.BoundingBox.Min;
                float characteristicLength = MathF.Max(boundsSize.X, boundsSize.Y);
                float probeHalfLength = MathFEx.Clamp(characteristicLength * 1e-4f, 1e-5f, 1e-2f);
                float probeHalfWidth = probeHalfLength;

                var probeObb = new OrientedBoundingBox2D(center, rayDirection, lateralAxis, new Vector2(probeHalfLength, probeHalfWidth));
                var probeShape = new CollisionShape2D(probeObb);

                if (!probeShape.TryGetCollision(bestColliderShape, out CollisionResult2D collisionResult)) return raycastResult;
                raycastResult.ColliderComponent = bestCollider;
                raycastResult.ContactPoint = enterPoint;
                raycastResult.Direction = rayDirection;
                raycastResult.Normal = collisionResult.Normal;
                raycastResult.Distance = (lineSegment.Start - enterPoint).Length();
                raycastResult.Fraction = bestEnterTime;
                raycastResult.Hit = true;
                return raycastResult;
            }

            return raycastResult;
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction, Collider collider, bool exclude = true)
        {
            HashSet<Collider> filter = new HashSet<Collider> { collider };
            RaycastFilter filterType = exclude ? RaycastFilter.Exclude : RaycastFilter.Include;
            RaycastParams raycastParams = new RaycastParams(filter, filterType);
            return Raycast(origin, direction, raycastParams);
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction)
        {
            RaycastParams raycastParams = new RaycastParams(new HashSet<Collider>(), RaycastFilter.Exclude);
            return Raycast(origin, direction, raycastParams);
        }

        public void UpdatePhysics(GameTime gameTime)
        {
            CollisionWorld.RebuildDynamicLayers();

            foreach (CollisionPair2D pair in CollisionWorld.QueryCollisionPairs(MOVEABLE_LAYER_NAME, STATIC_LAYER_NAME))
            {
                MonoCollisionActor first = (MonoCollisionActor)pair.First;
                MonoCollisionActor second = (MonoCollisionActor)pair.Second;

                Behaviour firstBehaviour = first.GO.GetComponent<Behaviour>() ?? null;
                Behaviour secondBehaviour = second.GO.GetComponent<Behaviour>() ?? null;

                firstBehaviour?.OnCollision(pair.FirstResult, second.GO);
                secondBehaviour?.OnCollision(pair.SecondResult, first.GO);
            }

            foreach (CollisionPair2D pair in CollisionWorld.QueryCollisionPairs(MOVEABLE_LAYER_NAME, MOVEABLE_LAYER_NAME))
            {
                MonoCollisionActor first = (MonoCollisionActor)pair.First;
                MonoCollisionActor second = (MonoCollisionActor)pair.Second;

                Behaviour firstBehaviour = first.GO.GetComponent<Behaviour>() ?? null;
                Behaviour secondBehaviour = second.GO.GetComponent<Behaviour>() ?? null;

                firstBehaviour?.OnCollision(pair.FirstResult, second.GO);
                secondBehaviour?.OnCollision(pair.SecondResult, first.GO);
            }
        }
    }
}