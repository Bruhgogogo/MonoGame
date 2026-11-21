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

        static public List<MonoObject> ObjectList;

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

            ObjectList = new List<MonoObject>();

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

            BeginRenderPass();
            UpdateWorld(gameTime);
            EndRenderPass();

            base.Draw(gameTime);
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
            for (int i = 0; i < ObjectList.Count; i++)
            {
                if (ObjectList[i] is Behaviour behaviour && !behaviour.Destroyed)
                {
                    behaviour.FixedUpdate(gameTime);
                }
            }
        }

        private void UpdateWorld(GameTime gameTime)
        {
            for (int i = ObjectList.Count - 1; i >= 0;)
            {
                MonoObject obj = ObjectList[i];

                if (obj.Destroyed)
                {
                    int lastIndex = ObjectList.Count - 1;
                    ObjectList[i] = ObjectList[lastIndex];
                    ObjectList.RemoveAt(lastIndex);
                    continue;
                }

                if (obj is RendererSprite renderer && renderer.IsVisible())
                {
                    renderer.Draw();
                }
                else if (obj is Behaviour behaviour)
                {
                    if (!behaviour.Started)
                    {
                        behaviour.Started = true;
                        behaviour.Start();
                    }
                    behaviour.Update(gameTime);
                }

                i--;
            }
        }

        private void LoadScene()
        {
            Player player = new Player();
        }
    }
}