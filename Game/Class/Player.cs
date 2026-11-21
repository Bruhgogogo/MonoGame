using MonoEngine.MonoUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoEngine.Class
{
    public class Player : Behaviour
    {
        public float WalkSpeed = 400.0f;
        public float RunSpeed = 800.0f;

        public float GroundAccel = 6.0f;
        public float AirAccel = 0.5f;

        public float GroundFric = 10.0f;
        public float AirFric = 0.5f;

        public float Gravity = 980.0f;
        public float JumpForce = 600.0f;

        public Vector2 CurrentVelocity = Vector2.Zero;
        public Vector2 CurrentPosition = Vector2.Zero;

        private bool isFacingRight = false;
        public bool IsFacingRight { get => isFacingRight; }

        private bool isGrounded = false;
        private RendererSprite rendererSprite;

        public Player() : base()
        {
            Texture2D playerTexture = MonoEngine.ResourceManager.Load<Texture2D>("ExperimentPlushieConstruction");

            rendererSprite = new RendererSprite();
            rendererSprite.Texture = playerTexture;
            rendererSprite.Size = new Vector2(100, 124);
        }

        private void ProcessMove(KeyboardState keyboardState, float deltaTime)
        {
            float inputDirection = 0.0f;

            if (keyboardState.IsKeyDown(Keys.A))
            {
                inputDirection -= 1.0f;
                isFacingRight = false;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                inputDirection += 1.0f;
                isFacingRight = true;
            }

            float speed = keyboardState.IsKeyDown(Keys.LeftShift) ? RunSpeed : WalkSpeed;
            float accel = isGrounded ? GroundAccel : AirAccel;
            float fric = isGrounded ? GroundFric : AirFric;

            CurrentVelocity.X += inputDirection * speed * accel * deltaTime;
            CurrentVelocity.X *= 1.0f - fric * deltaTime;

            if (keyboardState.IsKeyDown(Keys.Space))
            {
                if (!isGrounded)
                    return;

                CurrentVelocity.Y = -JumpForce;
                isGrounded = false;
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            if (!isGrounded)
            {
                CurrentVelocity.Y += Gravity * deltaTime;
            }
        }

        private void ApplyVelocity(float deltaTime)
        {
            CurrentVelocity = Vector2Ex.ApplyDeadZone(CurrentVelocity, 1.0f);
            CurrentPosition += CurrentVelocity * deltaTime;
        }

        private void CheckGroundCollision()
        {
            float groundY = MonoEngine.WindowHeight - rendererSprite.Size.Y;
            if (CurrentPosition.Y >= groundY)
            {
                CurrentPosition.Y = groundY;
                CurrentVelocity.Y = 0f;
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }

        private void SyncRenderTransform()
        {
            rendererSprite.Position = CurrentPosition;
            rendererSprite.FlipX = isFacingRight;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();

            ProcessMove(keyboardState, deltaTime);
            ApplyGravity(deltaTime);
            ApplyVelocity(deltaTime);
            CheckGroundCollision();
            SyncRenderTransform();
        }
    }
}