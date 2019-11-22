
using Engine.ContentLoaders;
using Engine.Entities;
using Engine.Packer;
using Engine.Screens;
using Engine.Sprites;
using Engine.Tiles;
using GeonBit.UI;
using GeonBit.UI.Entities;
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
        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public static SpriteBatch MainSpriteBatch { get; private set; }
        public static JContent JContent { get; private set; }
        public static ContentManager XNAContent { get; private set; }
        public static readonly Camera Camera = new Camera();
        public static Color BackgroundColor = Color.CornflowerBlue;
        public static ScreenManager ScreenManager { get; private set; }
        public static EntityManager Entities { get; private set; }
        public static TileLayer TileMap { get; private set; }
        public static SpriteAtlas MainAtlas { get; private set; }
        public static Sprite Pixel { get; private set; }

        public static Action<JContent> UponRegisterContentLoaders;
        public static Action<ScreenManager> UponRegisterScreens;
        public static Action<JContent> UponLoadContent;

        private static FixedSizeSpritePacker packer;
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
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Make the back buffer be preserved even when we swap out render targets at runtime.
            GraphicsDeviceManager.PreparingDeviceSettings += (object s, PreparingDeviceSettingsEventArgs args) =>
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
            JContent = new JContent();
            JContent.RegisterLoader(new TextureLoader());
            JContent.RegisterLoader(new FontLoader());
            JContent.RegisterLoader(new SpriteLoader());
            UponRegisterContentLoaders?.Invoke(JContent);

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

        private void InitUI()
        {
            UserInterface.Initialize(XNAContent, BuiltinThemes.editor);
            UserInterface.Active.ShowCursor = false;

            return;

            var realPanel = new Panel(new Vector2(500, 650));
            realPanel.Draggable = true;
            var tabs = new PanelTabs();
            realPanel.AddChild(tabs);
            var MainPanel = tabs.AddTab("Some Tab").panel;
            var tab2 = tabs.AddTab("Some Other Tab");
            //tab2.button.Enabled = false;
            tab2.button.ToolTipText = "This is tab 2.";
            tab2.panel.AddChild(new TextInput() { CharactersLimit = 16 });

            UserInterface.Active.AddEntity(realPanel);

            MainPanel.AddChild(new Header("Some Header Text"));
            MainPanel.AddChild(new HorizontalLine());
            MainPanel.AddChild(new Paragraph("This is a UI test."));
            var b = new Button("This is a button!");
            b.ToolTipText = "Some tooltip!";
            MainPanel.AddChild(b);
            MainPanel.AddChild(new Slider(0, 100, SliderSkin.Default) { Enabled = false });
            var toggle = new GeonBit.UI.Entities.CheckBox("Some Check", Anchor.Auto);
            MainPanel.AddChild(toggle);

            var test = new Panel(new Vector2(300, 140), PanelSkin.Fancy, Anchor.AutoInline);
            test.AddChild(new Label("I am a label"));
            test.AddChild(new CheckBox("I'm a box"));
            test.AdjustHeightAutomatically = true;
            MainPanel.AddChild(test);
            
            var list = new SelectList();
            list.AddItem("Option A");
            list.AddItem("Option B");
            list.AddItem("Option C");
            list.SetHeightBasedOnChildren();
            var drop = new DropDown();
            drop.AddItem("Something A");
            drop.AddItem("Something B");
            drop.AddItem("Something C");
            MainPanel.AddChild(drop);

            var pan2 = new Panel();
            pan2.AddChild(new Header("Some sub-panel"));
            pan2.AddChild(new CheckBox("Cool?") { OnValueChange = (e) => { Debug.Log((e as CheckBox).Checked.ToString()); } });
            tab2.panel.AddChild(pan2, true);
            //pan2.Anchor = Anchor.AutoInline;
            Debug.Log(MainPanel.Padding.ToString());

            MainPanel.AddChild(list);

            MainPanel.Padding = new Vector2(10, 5);
            MainPanel.SetHeightBasedOnChildren();
        }

        protected override void LoadContent()
        {
            XNAContent = base.Content;

            // Create a new SpriteBatch, which can be used to draw textures.
            MainSpriteBatch = new SpriteBatch(GraphicsDevice);

            // Create the main atlas packer.
            packer = new FixedSizeSpritePacker(1024, 1024, 1);
            JContent.Packer = packer;

            UponLoadContent?.Invoke(JContent);
            ScreenManager.LoadContent(JContent);
            var pixel = new Texture2D(GraphicsDevice, 3, 3, false, SurfaceFormat.Color);
            pixel.SetData(new Color[9] { Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White, Color.White });
            Pixel = packer.TryPack(pixel, "White Pixel");

            MainAtlas = packer.CreateSpriteAtlas(true);
            JContent.Packer = null;
            packer.Dispose();
            packer = null;

            Debug.Log("After all screens loaded content, the main atlas has been created!");
            Debug.Log("Main Atlas: " + MainAtlas);

            InitUI();
            Debug.Log("Initialized UI engine.");

            Loop.Start();
        }

        protected override void Update(GameTime gameTime)
        {
            ConsumeActions();
        }

        protected override void EndRun()
        {
            // TODO loads more stuff needs disposing (or do they...)

            base.EndRun();
            ScreenManager.OnClose();
            Loop.StopAndWait();
        }

        protected override void EndDraw()
        {
            // Don't call the base method, as this would present the image. Let the image be
            // presented by the loop class.
        }

        internal static void EngineUpdate()
        {
            UserInterface.Active.Update(new GameTime(TimeSpan.FromSeconds(Time.unscaledTime), TimeSpan.FromSeconds(Time.unscaledDeltaTime)));
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
