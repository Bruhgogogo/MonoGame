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

    public enum RaycastBroadphaseFilter
    {
        Static,
        Moveable,
        All,
    }

    public struct RaycastResult
    {
        public GameObject GameObject;
        public Vector2 ContactPoint;
        public Vector2 Normal;
        public Vector2 Direction;
        public float Distance;
        public float Fraction;
        public float ExitFraction;
        public bool Hit;
    }

    public struct RaycastParams
    {
        public HashSet<MonoCollisionActor> FilterColliders;
        public RaycastFilter FilterType;
        public RaycastBroadphaseFilter BroadPhaseFilterType;

        public RaycastParams(HashSet<MonoCollisionActor> filterColliders, RaycastFilter filterType, RaycastBroadphaseFilter broadPhaseFilterType)
        {
            FilterColliders = filterColliders;
            FilterType = filterType;
            BroadPhaseFilterType = broadPhaseFilterType;
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

        static private Layer staticLayer;
        static private Layer moveableLayer;

        public PhysicalEngine()
        {
            CollisionWorld = new CollisionWorld2D();

            SpatialHash staticSpatial = new SpatialHash(STATIC_LAYER_GRID_SIZE);
            SpatialHash moveableSpatial = new SpatialHash(MOVEABLE_LAYER_GRID_SIZE);

            staticLayer = new Layer(staticSpatial) { IsDynamic = false };
            moveableLayer = new Layer(moveableSpatial) { IsDynamic = true };

            CollisionWorld.AddLayer(STATIC_LAYER_NAME, staticLayer);
            CollisionWorld.AddLayer(MOVEABLE_LAYER_NAME, moveableLayer);
            CollisionWorld.EnableCollisionBetweenLayers(STATIC_LAYER_NAME, MOVEABLE_LAYER_NAME);
            CollisionWorld.EnableCollisionBetweenLayers(MOVEABLE_LAYER_NAME, MOVEABLE_LAYER_NAME);

            MonoColliders = new List<Collider>(MonoEngine.DEFAULT_COLLIDER_LIST_SIZE);
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction, RaycastParams raycastParams)
        {
            RaycastResult raycastResult = new RaycastResult();
            raycastResult.Hit = false;
            raycastResult.GameObject = null;
            raycastResult.ContactPoint = Vector2.Zero;
            raycastResult.Normal = Vector2.Zero;
            raycastResult.Direction = Vector2.Zero;
            raycastResult.Distance = 0.0f;
            raycastResult.Fraction = 0.0f;

            if (direction == Vector2.Zero) return raycastResult;

            LineSegment2D lineSegment = new LineSegment2D(origin, origin + direction);

            GameObject bestGameObject = null;
            MonoCollisionActor bestMonoCollision = null;

            float bestEnterTime = float.MaxValue;
            float bestExitTime = float.MaxValue;

            if (raycastParams.BroadPhaseFilterType == RaycastBroadphaseFilter.Static || raycastParams.BroadPhaseFilterType == RaycastBroadphaseFilter.All)
            {
                foreach (ICollisionActor colliderActor in staticLayer.Space.Query(lineSegment.GetBounds()))
                {
                    MonoCollisionActor monoCollisionActor = (MonoCollisionActor)colliderActor;

                    bool shouldSkip = raycastParams.FilterType switch
                    {
                        RaycastFilter.Include => !raycastParams.FilterColliders.Contains(monoCollisionActor),
                        RaycastFilter.Exclude => raycastParams.FilterColliders.Contains(monoCollisionActor),
                        _ => false
                    };
                    if (shouldSkip) continue;

                    CollisionShape2D shape2D = colliderActor.Shape;
                    if (!shape2D.Intersects(lineSegment, out float? enterTime, out float? exitTime)) continue;

                    float currentEnterTime = (float)enterTime;

                    if (currentEnterTime < bestEnterTime)
                    {
                        bestEnterTime = currentEnterTime;
                        bestExitTime = (float)exitTime;
                        bestGameObject = monoCollisionActor.GO;
                        bestMonoCollision = monoCollisionActor;
                    }
                }
            }

            if (raycastParams.BroadPhaseFilterType == RaycastBroadphaseFilter.Moveable || raycastParams.BroadPhaseFilterType == RaycastBroadphaseFilter.All)
            {
                foreach (ICollisionActor colliderActor in moveableLayer.Space.Query(lineSegment.GetBounds()))
                {
                    MonoCollisionActor monoCollisionActor = (MonoCollisionActor)colliderActor;

                    bool shouldSkip = raycastParams.FilterType switch
                    {
                        RaycastFilter.Include => !raycastParams.FilterColliders.Contains(monoCollisionActor),
                        RaycastFilter.Exclude => raycastParams.FilterColliders.Contains(monoCollisionActor),
                        _ => false
                    };
                    if (shouldSkip) continue;

                    CollisionShape2D shape2D = colliderActor.Shape;
                    if (!shape2D.Intersects(lineSegment, out float? enterTime, out float? exitTime)) continue;

                    float currentEnterTime = (float)enterTime;

                    if (currentEnterTime < bestEnterTime)
                    {
                        bestEnterTime = currentEnterTime;
                        bestExitTime = (float)exitTime;
                        bestGameObject = monoCollisionActor.GO;
                        bestMonoCollision = monoCollisionActor;
                    }
                }
            }

            if (bestGameObject) // Remeber replace this logic when MonoGameExtended has normal trace
            {
                Vector2 enterPoint = lineSegment.GetPoint(bestEnterTime);
                Vector2 exitPoint = lineSegment.GetPoint(bestExitTime);
                Vector2 center = (enterPoint + exitPoint) * 0.5f;

                Vector2 rayDirection = lineSegment.Direction;
                Vector2 lateralAxis = new Vector2(-rayDirection.Y, rayDirection.X);

                CollisionShape2D bestColliderShape = bestMonoCollision.Shape;
                Vector2 boundsSize = bestColliderShape.BoundingBox.Max - bestColliderShape.BoundingBox.Min;
                float characteristicLength = MathF.Max(boundsSize.X, boundsSize.Y);
                float probeHalfLength = MathFEx.Clamp(characteristicLength * 1e-4f, 1e-5f, 1e-2f);
                float probeHalfWidth = probeHalfLength;

                var probeObb = new OrientedBoundingBox2D(center, rayDirection, lateralAxis, new Vector2(probeHalfLength, probeHalfWidth));
                var probeShape = new CollisionShape2D(probeObb);

                if (!probeShape.TryGetCollision(bestColliderShape, out CollisionResult2D collisionResult)) return raycastResult;

                raycastResult.GameObject = bestGameObject;
                raycastResult.ContactPoint = enterPoint;
                raycastResult.Direction = rayDirection;
                raycastResult.Normal = collisionResult.Normal;
                raycastResult.Distance = (lineSegment.Start - enterPoint).Length();
                raycastResult.Fraction = bestEnterTime;
                raycastResult.ExitFraction = bestExitTime;
                raycastResult.Hit = true;

                return raycastResult;
            }

            return raycastResult;
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction, MonoCollisionActor collider, RaycastFilter raycastFilter = RaycastFilter.Exclude, RaycastBroadphaseFilter raycastBroadphaseFilter = RaycastBroadphaseFilter.All)
        {
            HashSet<MonoCollisionActor> filter = new HashSet<MonoCollisionActor> { collider };
            RaycastParams raycastParams = new RaycastParams(filter, raycastFilter, raycastBroadphaseFilter);
            return Raycast(origin, direction, raycastParams);
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction)
        {
            RaycastParams raycastParams = new RaycastParams(new HashSet<MonoCollisionActor>(), RaycastFilter.Exclude, RaycastBroadphaseFilter.All);
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