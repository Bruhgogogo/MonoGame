using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MonoEngine.Class;
using MonoEngine.MonoUtils;

using MonoGame.Class;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms.Design.Behavior;

namespace MonoEngine
{
    public class MonoEngine : Game
    {
        static public GraphicsDeviceManager Graphics;
        static public SpriteBatch SpriteBatch;

        static public Vector2 ViewportSize
        {
            get
            {
                return new Vector2(Graphics.GraphicsDevice.Viewport.Width, Graphics.GraphicsDevice.Viewport.Height);
            }
            set
            {
                Graphics.PreferredBackBufferWidth = (int)value.X;
                Graphics.PreferredBackBufferHeight = (int)value.Y;
                Graphics.ApplyChanges();
            }
        }

        static public bool WindowResizable
        {
            get
            {
                return MainGameInstance.Window.AllowUserResizing;
            }
            set
            {
                MainGameInstance.Window.AllowUserResizing = value;
            }
        }

        static public bool IsFullScreen
        {
            get
            {
                return Graphics.IsFullScreen;
            }
            set
            {
                Graphics.IsFullScreen = value;
                Graphics.ApplyChanges();
            }
        }

        static public int TargetElaspedTime
        {
            get
            {
                return MainGameInstance.TargetElapsedTime.Milliseconds;
            }
            set
            {
                MainGameInstance.TargetElapsedTime = TimeSpan.FromMilliseconds(value);
            }
        }

        static public List<Renderer> RendererList;
        static public List<Behaviour> BehaviourList;
        static public List<GCObject> PendingGCList;

        public static ContentManager ResourceManager;
        private static MonoEngine MainGameInstance;

        public const float METER_TO_PIXEL = 192.0f;
        public const float PIXEL_TO_METER = 1.0f / 192.0f;

        private const int DEFAULT_WINDOW_WIDTH = 1280;
        private const int DEFAULT_WINDOW_HEIGHT = 720;
        private const string DEFAULT_WINDOW_TITLE = "My Test";

        public MonoEngine()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            ResourceManager = Content;
            MainGameInstance = this;

            Graphics.PreferredBackBufferHeight = DEFAULT_WINDOW_HEIGHT;
            Graphics.PreferredBackBufferWidth = DEFAULT_WINDOW_WIDTH;
            Graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000 / 60);

            Window.AllowUserResizing = true;
            Window.Title = DEFAULT_WINDOW_TITLE;
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            RendererList = new List<Renderer>();
            BehaviourList = new List<Behaviour>();
            PendingGCList = new List<GCObject>();

            LoadScene();
        }

        protected override void Update(GameTime gameTime)
        {
            FixedUpdateWorld(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ClearScreen(Color.CornflowerBlue);

            UpdateGCObject();
            UpdateWorld(gameTime);

            BeginRenderPass();
            RenderWorld();
            EndRenderPass();

            base.Draw(gameTime);
        }

        private void BeginRenderPass()
        {
            Matrix viewMatrix = (Camera.Main != null && Camera.Main.Enabled) ? Camera.Main.GetViewMatrix() : Matrix.Identity;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, viewMatrix);
        }

        private void EndRenderPass()
        {
            SpriteBatch.End();
        }

        private void ClearScreen(Color clearColor)
        {
            GraphicsDevice.Clear(clearColor);
        }

        private void UpdateGCObject()
        {
            for (int i = PendingGCList.Count - 1; i >= 0; i--)
            {
                GCObject gcObj = PendingGCList[i];

                if (gcObj.IsObjectDestroyed())
                {
                    PendingGCList.SwapAndRemove(i);
                    continue;
                }

                if (gcObj.ShouldDestroy())
                {
                    gcObj.Destroy();
                    PendingGCList.SwapAndRemove(i);
                }
            }
        }

        private void FixedUpdateWorld(GameTime gameTime)
        {
            for (int i = 0; i < BehaviourList.Count; i++)
            {
                Behaviour behaviour = BehaviourList[i];

                if (!behaviour || !behaviour.Enabled) continue;

                behaviour.FixedUpdate(gameTime);
            }
        }

        private void UpdateWorld(GameTime gameTime)
        {
            for (int i = BehaviourList.Count - 1; i >= 0; i--)
            {
                Behaviour behaviour = BehaviourList[i];

                if (!behaviour)
                {
                    BehaviourList.SwapAndRemove(i);
                    continue;
                }

                if (!behaviour.Enabled) continue;

                if (!behaviour.Started)
                {
                    behaviour.Started = true;
                    behaviour.Start();
                }

                behaviour.Update(gameTime);
            }
        }

        private void RenderWorld()
        {
            for (int i = RendererList.Count - 1; i >= 0; i--)
            {
                Renderer renderer = RendererList[i];

                if (!renderer)
                {
                    RendererList.SwapAndRemove(i);
                    continue;
                }

                if (!renderer.IsVisible() || !renderer.Enabled) continue;

                renderer.Draw();
            }
        }

        private void LoadScene()
        {
            GameObject playerObject = new GameObject("Player");
            playerObject.AddComponent<PlayerScript>();

            GameObject cameraObject = new GameObject("Camera");
            //cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }
    }
}