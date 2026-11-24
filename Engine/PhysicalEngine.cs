using Microsoft.Xna.Framework;
using MonoEngine.Class;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using System.Diagnostics;

namespace MonoEngine
{
    public sealed class PhysicalEngine
    {
        static public CollisionWorld2D CollisionWorld { get; private set; }

        public const string StaticLayerName = "StaticLayer";
        public const string MoveableLayerName = "MoveableLayer";

        private SizeF staticLayerGridSize = new SizeF(256.0f, 256.0f);
        private SizeF moveableLayerGridSize = new SizeF(256.0f, 256.0f);

        public PhysicalEngine()
        {
            CollisionWorld = new CollisionWorld2D();

            SpatialHash staticSpatial = new SpatialHash(staticLayerGridSize);
            SpatialHash moveableSpatial = new SpatialHash(moveableLayerGridSize);

            Layer staticLayer = new Layer(staticSpatial) { IsDynamic = false };
            Layer moveableLayer = new Layer(staticSpatial) { IsDynamic = true };

            CollisionWorld.AddLayer(StaticLayerName, staticLayer);
            CollisionWorld.AddLayer(MoveableLayerName, moveableLayer);
            CollisionWorld.EnableCollisionBetweenLayers(StaticLayerName, MoveableLayerName);
            CollisionWorld.EnableCollisionBetweenLayers(MoveableLayerName, MoveableLayerName);
        }

        public void UpdatePhysics(GameTime gameTime)
        {
            CollisionWorld.RebuildDynamicLayers();

            foreach (CollisionPair2D pair in CollisionWorld.QueryCollisionPairs(MoveableLayerName, StaticLayerName))
            {
                MonoCollisionActor first = (MonoCollisionActor)pair.First;
                MonoCollisionActor second = (MonoCollisionActor)pair.Second;

                Behaviour firstBehaviour = first.GO.GetComponent<Behaviour>() ?? null;
                Behaviour secondBehaviour = second.GO.GetComponent<Behaviour>() ?? null;

                firstBehaviour?.OnCollision(pair.FirstResult, second.GO);
                secondBehaviour?.OnCollision(pair.SecondResult, first.GO);
            }

            foreach (CollisionPair2D pair in CollisionWorld.QueryCollisionPairs(MoveableLayerName, MoveableLayerName))
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