
using Engine.Loaders;
using Engine.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Engine
{
    public class Engine : Game
    {
        public static string GameName { get; set; } = "Engine";
        public static Engine Instance { get; internal set; }

        public static GameWindow GameWindow { get; private set; }
        public static GraphicsDevice MainGraphicsDevice { get; private set; }
        public static SpriteBatch MainSpriteBatch { get; private set; }
        public static Content ContentManager { get; private set; }
        public static ContentManager XNAContent { get; private set; }
        public static readonly Camera Camera = new Camera();
        public static Color BackgroundColor = Color.CornflowerBlue;
        public static ScreenManager ScreenManager { get; private set; }
        private readonly GraphicsDeviceManager graphics;
        private static readonly ConcurrentQueue<Action> pendingThreadActions = new ConcurrentQueue<Action>();

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public static bool AddAction(Action action)
        {
            if (action == null)
                return false;

            pendingThreadActions.Enqueue(action);
            return true;
        }

        private void ConsumeActions()
        {
            while(pendingThreadActions.Count > 0)
            {
                bool worked = pendingThreadActions.TryDequeue(out Action action);
                if (worked)
                {
                    action?.Invoke();
                }
            }
        }

        protected override void Initialize()
        {
            MainGraphicsDevice = base.GraphicsDevice;
            GameWindow = base.Window;
            IsMouseVisible = true;

            Debug.Init();
            ContentManager = new Content();
            ContentManager.RegisterLoader(new TextureLoader());
            ContentManager.RegisterLoader(new FontLoader());

            ScreenManager = new ScreenManager();
            ScreenManager.RegisterNew(new TestScreen()).Active = true;
            ScreenManager.RegisterNew(new LoadingScreen()).Active = true;
            // TODO register manager screens.

            ScreenManager.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            XNAContent = base.Content;

            // Create a new SpriteBatch, which can be used to draw textures.
            MainSpriteBatch = new SpriteBatch(GraphicsDevice);

            ScreenManager.LoadContent(ContentManager);

            Loop.Start();
        }

        protected override void Update(GameTime gameTime)
        {
            ConsumeActions();
        }

        protected override void EndRun()
        {
            base.EndRun();
            Loop.StopAndWait();
        }

        protected override void EndDraw()
        {
            // Don't call the base method, as this would present the image. Let the image be
            // presented by the loop class.
        }

        internal static void EngineUpdate()
        {
            ScreenManager.Update();
        }

        internal static void EngineDraw()
        {
            ScreenManager.Draw(MainSpriteBatch);
        }

        internal static void EngineDrawUI()
        {
            ScreenManager.DrawUI(MainSpriteBatch);
        }
    }
}
