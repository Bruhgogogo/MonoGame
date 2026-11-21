using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoEngine.MonoUtils;
using System;

namespace MonoEngine.Class
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
        private Transform transform;

        public override void Awake()
        {
            Texture2D playerTexture = MonoEngine.ResourceManager.Load<Texture2D>("ExperimentPlushieConstruction");

            rendererSprite = GetOrAddComponent<RendererSprite>();
            rendererSprite.Texture = playerTexture;

            transform = GetOrAddComponent<Transform>();
            transform.Position = Vector2.Zero;
            transform.Scale = new Vector2(100.0f, 124f);
            transform.Rotation = 0.0f;
        }

        private bool IsOnTheRight(float x)
        {
            return x > transform.Position.X;
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
            isFacingRight = IsOnTheRight(mouseState.X);
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

        private void CheckGroundCollision()
        {
            float groundY = MonoEngine.WindowHeight - transform.Scale.Y;
            if (transform.Position.Y >= groundY)
            {
                transform.Position.Y = groundY;
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
            CheckGroundCollision();
        }
    }
}