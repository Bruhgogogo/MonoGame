namespace MonoEngine.Class
{
    public sealed class Attachment : Component
    {
        public Transform transform;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
        }
    }
}