using MonoEngine;
using MonoEngine.Class;
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

        public void Shoot()
        {
            RaycastResult result = PhysicalEngine.Raycast(playerScript.transform.Position, playerScript.GetFoward() * 10000.0f, playerScript.boxCollider.CollisionActor);
            if (!result.Hit) return;
        }
    }
}