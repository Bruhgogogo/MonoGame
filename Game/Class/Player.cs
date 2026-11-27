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
        public float Speed = 5.0f * Engine.METER_TO_PIXEL;

        public float MoveBackwardPenaltyFactor = 0.5f;

        public float GroundAccel = 6.0f;
        public float AirAccel = 0.5f;

        public float GroundFric = 10.0f;
        public float AirFric = 0.25f;

        public float Gravity = 9.8f * Engine.METER_TO_PIXEL;
        public float JumpForce = 4.5f * Engine.METER_TO_PIXEL;

        public Transform transform;
        public ColliderBox colliderBox;

        public Vector2 Velocity = Vector2.Zero;

        private bool isFacingRight = false;
        public bool IsFacingRight { get => isFacingRight; }

        private float maxSlopeAngle = 0.7f;
        public float MaxSlopeAngle
        {
            get
            {
                return MathF.Acos(maxSlopeAngle);
            }
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
            colliderBox = GetOrAddComponent<ColliderBox>();
            audioSource = GetOrAddComponent<AudioSource>();

            colliderBox.IsStatic = false;
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

            colliderBox.ScaleOffset = new Vector2(0.85f, 0.85f);

            audioSource.Clip = bgm;
            audioSource.Loop = true;
            audioSource.Play();
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

        private void ResolveCollision(List<OverlapResult> results)
        {
            isGrounded = false;

            foreach (var result in results)
            {
                transform.Position += result.MinimumTranslationVector;

                if (Vector2.Dot(result.Normal, Vector2Ex.Up) > maxSlopeAngle)
                    isGrounded = true;
            }
        }

        private void ClipVelocity(List<OverlapResult> results, float overbounce = 1.0f)
        {
            Vector2 velocity = Velocity;

            for (int i = 0; i < 4; i++)
            {
                foreach (var result in results)
                {
                    float normalVelocity = Vector2.Dot(velocity, result.Normal);
                    if (normalVelocity >= 0f) continue;
                    velocity -= result.Normal * (normalVelocity * overbounce);

                    float adjust = Vector2.Dot(velocity, result.Normal);
                    if (adjust < 0f)
                        velocity -= result.Normal * adjust;

                    Vector2Ex.ApplyDeadZoneRef(ref velocity, 1.0f);
                }
            }

            Velocity = velocity;
        }

        private void MoveAndSlide(float deltaTime)
        {
            ApplyVelocity(deltaTime);

            List<OverlapResult> collisionResult = PhysicalEngine.OverlapCollider(colliderBox.CollisionActor);
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