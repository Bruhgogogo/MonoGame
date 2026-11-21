using MonoEngine.Class;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MonoEngine
{
    public class MonoEngine : Game
    {
        static public GraphicsDeviceManager Graphics;
        static public SpriteBatch SpriteBatch;

        static public int WindowWidth
        {
            get
            {
                if (Graphics == null)
                    throw new InvalidOperationException("Cannot access window width when GraphicsDeviceManager is not exist");
                return Graphics.GraphicsDevice.Viewport.Width;
            }
            set
            {
                if (Graphics == null)
                    throw new InvalidOperationException("Cannot access window width when GraphicsDeviceManager is not exist");
                Graphics.PreferredBackBufferWidth = value;
                Graphics.ApplyChanges();
            }
        }
        static public int WindowHeight
        {
            get
            {
                if (Graphics == null)
                    throw new InvalidOperationException("Cannot access window height when GraphicsDeviceManager is not exist");
                return Graphics.GraphicsDevice.Viewport.Height;
            }
            set
            {
                if (Graphics == null)
                    throw new InvalidOperationException("Cannot access window height when GraphicsDeviceManager is not exist");
                Graphics.PreferredBackBufferHeight = value;
                Graphics.ApplyChanges();
            }
        }

        static public bool WindowResizable
        {
            get
            {
                if (MainGameInstance == null)
                    throw new InvalidOperationException("Cannot access window resizable when MainGameInstance is not exist");
                return MainGameInstance.Window.AllowUserResizing;
            }
            set
            {
                if (MainGameInstance == null)
                    throw new InvalidOperationException("Cannot access window resizable when MainGameInstance is not exist");
                MainGameInstance.Window.AllowUserResizing = value;
            }
        }

        static public bool IsFullScreen
        {
            get
            {
                if (Graphics == null)
                    throw new InvalidOperationException("Cannot access is fullscreen when GraphicsDeviceManager is not exist");
                return Graphics.IsFullScreen;
            }
            set
            {
                if (Graphics == null)
                    throw new InvalidOperationException("Cannot access is fullscreen when GraphicsDeviceManager is not exist");
                Graphics.IsFullScreen = value;
                Graphics.ApplyChanges();
            }
        }

        static public List<Renderer> RendererList;
        static public List<Behaviour> BehaviourList;

        public static ContentManager ResourceManager;
        private static MonoEngine MainGameInstance;

        private const int DEFAULT_WINDOW_WIDTH = 1280;
        private const int DEFAULT_WINDOW_HEIGHT = 720;
        private const string DEFAULT_WINDOW_TITLE = "My Test";

        public MonoEngine()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            ResourceManager = Content;

            Graphics.PreferredBackBufferHeight = DEFAULT_WINDOW_HEIGHT;
            Graphics.PreferredBackBufferWidth = DEFAULT_WINDOW_WIDTH;
            Graphics.SynchronizeWithVerticalRetrace = false;

            Window.AllowUserResizing = true;
            Window.Title = DEFAULT_WINDOW_TITLE;
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            RendererList = new List<Renderer>();
            BehaviourList = new List<Behaviour>();

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

            CleanupDestroyed();

            BeginRenderPass();
            UpdateWorld(gameTime);
            EndRenderPass();

            base.Draw(gameTime);
        }

        private void CleanupDestroyed()
        {
            BehaviourList.RemoveAll(b => !b);
            RendererList.RemoveAll(r => !r);
        }

        private void BeginRenderPass()
        {
            SpriteBatch.Begin();
        }

        private void EndRenderPass()
        {
            SpriteBatch.End();
        }

        private void ClearScreen(Color clearColor)
        {
            GraphicsDevice.Clear(clearColor);
        }

        private void FixedUpdateWorld(GameTime gameTime)
        {
            foreach (var behaviour in BehaviourList)
            {
                if (!behaviour || !behaviour.Enabled) continue;

                behaviour.FixedUpdate(gameTime);
            }
        }

        private void UpdateWorld(GameTime gameTime)
        {
            foreach (var behaviour in BehaviourList)
            {
                if (!behaviour || !behaviour.Enabled) continue;

                behaviour.Update(gameTime);
            }

            foreach (var renderer in RendererList)
            {
                if (!renderer || !renderer.IsVisible() || !renderer.Enabled) continue;

                renderer.Draw();
            }
        }

        private void LoadScene()
        {
            GameObject playerObject = new GameObject();
            playerObject.Name = "Player";
            playerObject.AddComponent<PlayerScript>();
        }
    }
}