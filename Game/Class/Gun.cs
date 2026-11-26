using MonoEngine;
using MonoEngine.Class;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace MyMonoGame.Class
{
    public class GunScript : Behaviour
    {
        private GameObject body;
        private PlayerScript playerScript;

        public override void Start()
        {
            body = GO.Parent;
            playerScript = body.GetComponent<PlayerScript>();
        }

        public void Shoot(Vector2 origin, Vector2 direciton)
        {
            RaycastResult result = PhysicalEngine.Raycast(origin, direciton * 10000.0f, playerScript.boxCollider.CollisionActor);
            if (!result.Hit) return;
        }
    }
}