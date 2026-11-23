using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.MonoUtils;
using MonoEngine.Class;
using System;
using Microsoft.Xna.Framework.Audio;

namespace MonoGame.Class
{
    public class PlayerScript : Behaviour
    {
        public float Speed = 800.0f;

        public float MoveBackwardPenaltyFactor = 0.5f;

        public float GroundAccel = 6.0f;
        public float AirAccel = 0.5f;

        public float GroundFric = 10.0f;
        public float AirFric = 0.25f;

        public float Gravity = 980.0f;
        public float JumpForce = 600.0f;

        public Vector2 Velocity = Vector2.Zero;

        private bool isFacingRight = false;
        public bool IsFacingRight { get => isFacingRight; }

        private bool isGrounded = false;

        private RendererSprite rendererSprite;
        private RendererSprite testRendererSprite;
        private Transform transform;
        private Transform testTransform;
        private AudioSource audioSource;
        private GameObject otherGameObject;

        private Vector2 cameraVelocity = Vector2.Zero;

        public override void Awake()
        {
            rendererSprite = GetOrAddComponent<RendererSprite>();
            transform = GetOrAddComponent<Transform>();
            audioSource = GetOrAddComponent<AudioSource>();
            audioSource.Is3D = true;

            otherGameObject = new GameObject("My Test Game Object");
            testRendererSprite = otherGameObject.GetOrAddComponent<RendererSprite>();
            testTransform = otherGameObject.GetOrAddComponent<Transform>();
        }
        public override void Start()
        {
            Texture2D playerTexture = MonoEngine.MonoEngine.ResourceManager.Load<Texture2D>("ExperimentPlushieConstruction");
            SoundEffect bgm = MonoEngine.MonoEngine.ResourceManager.Load<SoundEffect>("Ice");

            rendererSprite.Texture = playerTexture;
            rendererSprite.Origin = new Vector2(0.5f, 0.5f);

            transform.Scale = new Vector2(100.0f, 124f);

            audioSource.Clip = bgm;
            audioSource.Loop = true;
            audioSource.Play();

            testRendererSprite.Texture = playerTexture;
            testRendererSprite.Origin = new Vector2(0.5f, 0.5f);

            testTransform.Scale = new Vector2(1.5f, 1.5f);
            testTransform.Parent = transform;
        }

        private bool IsMouseOnTheRight(float x)
        {
            if (!Camera.Main)
                return x > transform.Position.X;
            
            return Camera.Main.ScreenToWorld(new Vector2(x, 0.0f)).X > transform.Position.X;
        }

        private void ProcessMove(KeyboardState keyboardState, float deltaTime)
        {
            float inputDirection = 0.0f;

            if (keyboardState.IsKeyDown(Keys.A))
            {
                inputDirection -= 1.0f;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                inputDirection += 1.0f;
            }

            bool isMovingBackward = ((isFacingRight && inputDirection < 0) || (!isFacingRight && inputDirection > 0)) && isGrounded;

            float speed = Speed * (isMovingBackward ? MoveBackwardPenaltyFactor : 1.0f);
            float accel = isGrounded ? GroundAccel : AirAccel;
            float fric = isGrounded ? GroundFric : AirFric;

            Velocity.X += inputDirection * speed * accel * deltaTime;
            Velocity.X *= MathF.Exp(-fric * deltaTime);

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                if (!isGrounded)
                    return;

                Velocity.Y = -JumpForce;
                isGrounded = false;
            }
        }

        private void ProcessLook(MouseState mouseState)
        {
            isFacingRight = IsMouseOnTheRight(mouseState.X);
            rendererSprite.FlipX = isFacingRight;
        }

        private void ApplyGravity(float deltaTime)
        {
            if (!isGrounded)
            {
                Velocity.Y += Gravity * deltaTime;
            }
        }

        private void ApplyVelocity(float deltaTime)
        {
            Velocity = Vector2Ex.ApplyDeadZone(Velocity, 1.0f);
            transform.Position += Velocity * deltaTime;
        }

        private void CameraFollow(float deltaTime)
        {
            Camera camera = Camera.Main;
            if (!camera)
                return;

            camera.transform.Position = Vector2Ex.SmoothDamp(camera.transform.Position, transform.Position, ref cameraVelocity, 0.1f, float.PositiveInfinity, deltaTime);
        }

        private void CheckGroundCollision()
        {
            float groundY = MonoEngine.MonoEngine.ViewportSize.Y - transform.Scale.Y * rendererSprite.Origin.Y;
            if (transform.Position.Y >= groundY)
            {
                transform.Position = new Vector2(transform.Position.X, groundY);
                Velocity.Y = 0f;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.1f);

            MouseState mouseState = Mouse.GetState();
            ProcessLook(mouseState);

            KeyboardState keyboardState = Keyboard.GetState();
            ProcessMove(keyboardState, deltaTime);

            ApplyGravity(deltaTime);
            ApplyVelocity(deltaTime);
            CameraFollow(deltaTime);
            CheckGroundCollision();
        }
    }
}