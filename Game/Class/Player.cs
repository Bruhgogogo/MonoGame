using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoEngine;
using MonoEngine.Class;
using MonoEngine.MonoUtils;

using System;
using System.Collections.Generic;

using Engine = MonoEngine.MonoEngine;

namespace MyMonoGame.Class
{
    public class PlayerScript : Behaviour
    {
        public float Speed = 800.0f;

        public float MoveBackwardPenaltyFactor = 0.5f;

        public float GroundAccel = 6.0f;
        public float AirAccel = 0.5f;

        public float GroundFric = 10.0f;
        public float AirFric = 0.25f;

        public float Gravity = 1000.0f;
        public float JumpForce = 600.0f;

        public Transform transform;
        public BoxCollider boxCollider;

        public Vector2 Velocity = Vector2.Zero;

        private bool isFacingRight = false;
        public bool IsFacingRight { get => isFacingRight; }

        private float maxSlopeAngle = 0.5f;
        public float MaxSlopeAngle
        {
            get => maxSlopeAngle;
            set
            {
                maxSlopeAngle = MathF.Cos(value);
            }
        }

        private bool isGrounded = false;

        private SpriteRenderer rendererSprite;
        private GunScript gunScript;
        private AudioSource audioSource;

        private Vector2 cameraVelocity = Vector2.Zero;

        public override void Awake()
        {
            transform = GetOrAddComponent<Transform>();
            rendererSprite = GetOrAddComponent<SpriteRenderer>();
            boxCollider = GetOrAddComponent<BoxCollider>();
            audioSource = GetOrAddComponent<AudioSource>();

            boxCollider.IsStatic = false;
            audioSource.Is3D = true;
        }

        public override void Start()
        {
            GetComponentInChildren(out gunScript);

            Texture2D playerTexture = Engine.ResourceManager.Load<Texture2D>("ExperimentPlushieConstruction");
            SoundEffect bgm = Engine.ResourceManager.Load<SoundEffect>("Ice");

            rendererSprite.Texture = playerTexture;
            rendererSprite.Origin = new Vector2(0.5f, 0.5f);

            transform.Scale = new Vector2(100.0f, 124f);

            audioSource.Clip = bgm;
            audioSource.Loop = true;
            audioSource.Play();
        }

        public Vector2 GetFoward()
        {
            return isFacingRight ? transform.Forward : -transform.Forward;
        }

        private bool IsMouseOnTheRight(float x)
        {
            Camera camera = Camera.Main;
            if (!camera) return x > transform.Position.X;
            
            return camera.ScreenToWorld(new Vector2(x, 0.0f)).X > transform.Position.X;
        }

        private void ProcessInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Camera camera = Camera.Main;
                if (!camera) return;

                Vector2 mouseWorldPos = camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                
                Vector2 origin = transform.Position;
                Vector2 direction = Vector2Ex.DirectionTo(origin, mouseWorldPos);

                gunScript.Shoot(origin, direction);
            }
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

                Velocity.Y = JumpForce;
                isGrounded = false;
            }
        }

        private void ProcessLook(MouseState mouseState)
        {
            isFacingRight = IsMouseOnTheRight(mouseState.X);
            rendererSprite.FlipX = isFacingRight;
        }

        private void CameraFollow(float deltaTime)
        {
            Camera camera = Camera.Main;
            if (!camera)
                return;

            camera.transform.Position = Vector2Ex.SmoothDamp(camera.transform.Position, transform.Position, ref cameraVelocity, 0.1f, deltaTime);
        }

        private void ApplyGravity(float deltaTime)
        {
            if (!isGrounded)
                Velocity.Y -= Gravity * deltaTime;
        }

        private void ApplyVelocity(float deltaTime)
        {
            Velocity = Vector2Ex.ApplyDeadZone(Velocity, 1.0f);
            transform.Translate(Velocity * deltaTime);
        }

        private void ResolveCollision(IEnumerable<CollisionQueryResult> results)
        {
            isGrounded = false;

            foreach (var result in results)
            {
                transform.Position += result.MinimumTranslationVector;

                if (Vector2.Dot(result.Normal, Vector2Ex.Up) > 0.5f)
                    isGrounded = true;
            }
        }

        private void ClipVelocity(IEnumerable<CollisionQueryResult> results)
        {
            Vector2 velocity = Velocity;

            for (int i = 0; i < 4; i++)
            {
                foreach (var result in results)
                {
                    float vn = Vector2.Dot(velocity, result.Normal);
                    if (vn < -1e-3f)
                        velocity -= result.Normal * vn;
                }
            }

            Velocity = velocity;
        }

        private void MoveAndSlide(float deltaTime)
        {
            ApplyVelocity(deltaTime);

            IEnumerable<CollisionQueryResult> collisionResult = PhysicalEngine.QueryCollision(boxCollider.CollisionActor);
            ResolveCollision(collisionResult);
            ClipVelocity(collisionResult);
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.1f);

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            ProcessLook(mouseState);
            ProcessInput(keyboardState, mouseState);
            ProcessMove(keyboardState, deltaTime);

            CameraFollow(deltaTime);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            float deltaTime = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.1f);

            ApplyGravity(deltaTime);
            MoveAndSlide(deltaTime);
        }
    }
}