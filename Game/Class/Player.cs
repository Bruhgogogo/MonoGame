using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoEngine.MonoUtils;
using MonoEngine.Class;
using Engine = MonoEngine.MonoEngine;

using System;

using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoEngine;
using System.Diagnostics;
using System.Collections.Generic;

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

        public Vector2 Velocity = Vector2.Zero;

        private bool isFacingRight = false;
        public bool IsFacingRight { get => isFacingRight; }

        private bool isGrounded = false;

        private RendererSprite rendererSprite;
        private ColliderBox colliderBox;
        private Transform transform;
        private AudioSource audioSource;

        private Vector2 cameraVelocity = Vector2.Zero;

        public override void Awake()
        {
            rendererSprite = GetOrAddComponent<RendererSprite>();
            colliderBox = GetOrAddComponent<ColliderBox>();
            transform = GetOrAddComponent<Transform>();
            audioSource = GetOrAddComponent<AudioSource>();

            colliderBox.IsStatic = false;
            audioSource.Is3D = true;
        }

        public override void Start()
        {
            Texture2D playerTexture = Engine.ResourceManager.Load<Texture2D>("ExperimentPlushieConstruction");
            SoundEffect bgm = Engine.ResourceManager.Load<SoundEffect>("Ice");

            rendererSprite.Texture = playerTexture;
            rendererSprite.Origin = new Vector2(0.5f, 0.5f);

            transform.Scale = new Vector2(100.0f, 124f);

            audioSource.Clip = bgm;
            audioSource.Loop = true;
            audioSource.Play();
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

                Velocity.Y = JumpForce;
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
                Velocity.Y -= Gravity * deltaTime;
            }
            else if (isGrounded && Velocity.Y < 0.0f)
            {
                Velocity.Y = 0.0f;
            }
        }

        private void ProcessGround(float deltaTime)
        {
            Vector2 origin = transform.Position - new Vector2(0.0f, transform.Scale.Y * rendererSprite.Origin.Y + 1.0f);
            Vector2 direction = new Vector2(0.0f, -1.0f);

            RaycastResult result = PhysicalEngine.Raycast(origin, direction, colliderBox);
            isGrounded = result.Hit;
        }

        private void ApplyVelocity(float deltaTime)
        {
            Velocity = Vector2Ex.ApplyDeadZone(Velocity, 1.0f);
            transform.Translate(Velocity * deltaTime);
        }

        private void CameraFollow(float deltaTime)
        {
            Camera camera = Camera.Main;
            if (!camera)
                return;

            camera.transform.Position = Vector2Ex.SmoothDamp(camera.transform.Position, transform.Position, ref cameraVelocity, 0.1f, deltaTime);
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.1f);

            MouseState mouseState = Mouse.GetState();
            ProcessLook(mouseState);

            KeyboardState keyboardState = Keyboard.GetState();
            ProcessMove(keyboardState, deltaTime);

            CameraFollow(deltaTime);
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            float deltaTime = MathF.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 0.1f);

            ApplyGravity(deltaTime);
            ApplyVelocity(deltaTime);
            ProcessGround(deltaTime);
        }
    }
}