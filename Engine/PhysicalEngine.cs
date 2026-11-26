using Microsoft.Xna.Framework;

using MonoEngine.Class;
using MonoEngine.MonoUtils;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoEngine
{
    public enum PhysicsFilter
    {
        Exclude,
        Include,
    }

    public enum PhysicsBroadphaseFilter
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
        public PhysicsFilter FilterType;
        public PhysicsBroadphaseFilter BroadPhaseFilterType;

        public RaycastParams(HashSet<MonoCollisionActor> filterColliders, PhysicsFilter filterType, PhysicsBroadphaseFilter broadPhaseFilterType)
        {
            FilterColliders = filterColliders;
            FilterType = filterType;
            BroadPhaseFilterType = broadPhaseFilterType;
        }
    }

    public struct CollisionQueryParams
    {
        public HashSet<MonoCollisionActor> FilterColliders;
        public PhysicsFilter FilterType;
        public PhysicsBroadphaseFilter BroadPhaseFilterType;

        public CollisionQueryParams(HashSet<MonoCollisionActor> filterColliders, PhysicsFilter filterType, PhysicsBroadphaseFilter broadPhaseFilterType)
        {
            FilterColliders = filterColliders;
            FilterType = filterType;
            BroadPhaseFilterType = broadPhaseFilterType;
        }
    }

    public struct CollisionQueryResult
    {
        GameObject GameObject;
        Vector2 Normal;
        Vector2 MinimumTranslationVector;
        float PenetrationDepth;

        public CollisionQueryResult(GameObject gameObject, Vector2 normal, Vector2 minimumTranslationVector, float penetrationDepth)
        {
            GameObject = gameObject;
            Normal = normal;
            MinimumTranslationVector = minimumTranslationVector;
            PenetrationDepth = penetrationDepth;
        }
    }

    public sealed class PhysicalEngine
    {
        static public CollisionWorld2D CollisionWorld { get; private set; }

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
        }

        static public IEnumerable<CollisionQueryResult> QueryCollision(MonoCollisionActor targetMonoCollisionActor, CollisionQueryParams collisionQueryParams)
        {
            List<CollisionQueryResult> collisionQueryResults = new List<CollisionQueryResult>();

            void Query(Layer layer)
            {
                foreach (ICollisionActor colliderActor in layer.Space.Query(targetMonoCollisionActor.shape.BoundingBox))
                {
                    MonoCollisionActor monoCollisionActor = (MonoCollisionActor)colliderActor;

                    bool shouldSkip = collisionQueryParams.FilterType switch
                    {
                        PhysicsFilter.Include => !collisionQueryParams.FilterColliders.Contains(monoCollisionActor),
                        PhysicsFilter.Exclude => collisionQueryParams.FilterColliders.Contains(monoCollisionActor),
                        _ => false
                    };
                    if (shouldSkip) continue;

                    CollisionShape2D shape2D = monoCollisionActor.shape;
                    if (!shape2D.TryGetCollision(shape2D, out CollisionResult2D result)) continue;

                    CollisionQueryResult collisionQueryResult = new CollisionQueryResult(monoCollisionActor.GO, result.Normal, result.MinimumTranslationVector, result.PenetrationDepth);
                    collisionQueryResults.Add(collisionQueryResult);
                }
            }

            if (collisionQueryParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.Static || collisionQueryParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.All)
            {
                Query(staticLayer);
            }

            if (collisionQueryParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.Moveable || collisionQueryParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.All)
            {
                Query(moveableLayer);
            }

            return collisionQueryResults.AsEnumerable();
        }

        static public IEnumerable<CollisionQueryResult> QueryCollision(MonoCollisionActor targetMonoCollisionActor)
        {
            CollisionQueryParams collisionQueryParams = new CollisionQueryParams(new HashSet<MonoCollisionActor>(), PhysicsFilter.Exclude, PhysicsBroadphaseFilter.All);
            return QueryCollision(targetMonoCollisionActor, collisionQueryParams);
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

            void Query(Layer layer)
            {
                foreach (ICollisionActor colliderActor in layer.Space.Query(lineSegment.GetBounds()))
                {
                    MonoCollisionActor monoCollisionActor = (MonoCollisionActor)colliderActor;

                    bool shouldSkip = raycastParams.FilterType switch
                    {
                        PhysicsFilter.Include => !raycastParams.FilterColliders.Contains(monoCollisionActor),
                        PhysicsFilter.Exclude => raycastParams.FilterColliders.Contains(monoCollisionActor),
                        _ => false
                    };
                    if (shouldSkip) continue;

                    CollisionShape2D shape2D = monoCollisionActor.shape;
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

            if (raycastParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.Static || raycastParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.All)
            {
                Query(staticLayer);
            }

            if (raycastParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.Moveable || raycastParams.BroadPhaseFilterType == PhysicsBroadphaseFilter.All)
            {
                Query(moveableLayer);
            }

            if (bestGameObject)
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

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction, MonoCollisionActor collider, PhysicsFilter raycastFilter = PhysicsFilter.Exclude, PhysicsBroadphaseFilter raycastBroadphaseFilter = PhysicsBroadphaseFilter.All)
        {
            HashSet<MonoCollisionActor> filter = new HashSet<MonoCollisionActor> { collider };
            RaycastParams raycastParams = new RaycastParams(filter, raycastFilter, raycastBroadphaseFilter);
            return Raycast(origin, direction, raycastParams);
        }

        static public RaycastResult Raycast(Vector2 origin, Vector2 direction)
        {
            RaycastParams raycastParams = new RaycastParams(new HashSet<MonoCollisionActor>(), PhysicsFilter.Exclude, PhysicsBroadphaseFilter.All);
            return Raycast(origin, direction, raycastParams);
        }

        public void UpdatePhysics(GameTime gameTime)
        {
            CollisionWorld.RebuildDynamicLayers();

            void Query(string firstLayer, string secondLayer)
            {
                foreach (CollisionPair2D pair in CollisionWorld.QueryCollisionPairs(firstLayer, secondLayer))
                {
                    MonoCollisionActor first = (MonoCollisionActor)pair.First;
                    MonoCollisionActor second = (MonoCollisionActor)pair.Second;

                    Behaviour firstBehaviour = first.GO.GetComponent<Behaviour>() ?? null;
                    Behaviour secondBehaviour = second.GO.GetComponent<Behaviour>() ?? null;

                    firstBehaviour?.OnCollision(pair.FirstResult, second.GO);
                    secondBehaviour?.OnCollision(pair.SecondResult, first.GO);
                }
            }

            Query(MOVEABLE_LAYER_NAME, STATIC_LAYER_NAME);
            Query(MOVEABLE_LAYER_NAME, MOVEABLE_LAYER_NAME);
        }
    }
}