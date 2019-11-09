
using Engine.Entities;
using Engine.Loaders;
using Engine.Packer;
using Engine.Screens;
using Engine.Sprites;
using Engine.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;

namespace Engine
{
    public class JEngine : Game
    {
        public static string GameName { get; set; } = "Engine";
        public static JEngine Instance { get; internal set; }

        public static GameWindow GameWindow { get; private set; }
        public static GraphicsDevice MainGraphicsDevice { get; private set; }
        public static SpriteBatch MainSpriteBatch { get; private set; }
        public static Content ContentManager { get; private set; }
        public static ContentManager XNAContent { get; private set; }
        public static readonly Camera Camera = new Camera();
        public static Color BackgroundColor = Color.CornflowerBlue;
        public static ScreenManager ScreenManager { get; private set; }
        public static EntityManager Entities { get; private set; }
        public static TileLayer TileMap { get; private set; }
        public static SpriteAtlas MainAtlas { get; private set; }
        public static Sprite Pixel { get; private set; }

        public static Action<Content> UponRegisterContentLoaders;
        public static Action<ScreenManager> UponRegisterScreens;
        public static Action<Content> UponLoadContent;

        private static FixedSizeSpritePacker packer;
        private readonly GraphicsDeviceManager graphics;
        private static readonly ConcurrentQueue<Action> pendingThreadActions = new ConcurrentQueue<Action>();

        [STAThread]
        public static void Start()
        {
            try
            {
                using (var game = new JEngine())
                {
                    JEngine.Instance = game;
                    game.Run();
                    JEngine.Instance = null;
                }
            }
            finally
            {
                Debug.Shutdown();
            }
        }

        public JEngine()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Make the back buffer be preserved even when we swap out render targets at runtime.
            graphics.PreparingDeviceSettings += (object s, PreparingDeviceSettingsEventArgs args) =>
            {
                args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            };
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
            Window.AllowUserResizing = true;
            
            Debug.Init();
            ContentManager = new Content();
            ContentManager.RegisterLoader(new TextureLoader());
            ContentManager.RegisterLoader(new FontLoader());
            ContentManager.RegisterLoader(new SpriteLoader());
            UponRegisterContentLoaders?.Invoke(ContentManager);

            ScreenManager = new ScreenManager();
            ScreenManager.RegisterNew(new CameraMoveScreen()).Active = false;
            ScreenManager.RegisterNew(new LoadingScreen()).Active = true;
            ScreenManager.RegisterNew(new DebugDisplayScreen()).Active = true;
            UponRegisterScreens?.Invoke(ScreenManager);
            // TODO register manager screens.

            Entities = new EntityManager();
            TileMap = new TileLayer();

            ScreenManager.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            XNAContent = base.Content;

            // Create a new SpriteBatch, which can be used to draw textures.
            MainSpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create the main atlas packer.
            packer = new FixedSizeSpritePacker(512, 512, 1);
            ContentManager.SpritePacker = packer;

            UponLoadContent?.Invoke(ContentManager);
            ScreenManager.LoadContent(ContentManager);
            var pixel = new Texture2D(GraphicsDevice, 3, 3, false, SurfaceFormat.Color);
            pixel.SetData(new Color[9] { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White });
            Pixel = packer.TryPack(pixel, "White Pixel");

            MainAtlas = packer.CreateSpriteAtlas(true);
            ContentManager.SpritePacker = null;
            packer.Dispose();
            packer = null;

            Debug.Log("After all screens loaded content, the main atlas has been created!");
            Debug.Log("Main Atlas: " + MainAtlas);

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
            Entities.UpdateAll();
        }

        internal static void EngineDraw()
        {
            ScreenManager.Draw(MainSpriteBatch);
            Entities.DrawAll(MainSpriteBatch);
        }

        internal static void EngineDrawUI()
        {
            ScreenManager.DrawUI(MainSpriteBatch);
        }
    }
}
