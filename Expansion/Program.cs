using Engine;
using Engine.Screens;
using Engine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


/*
 * Game ideas:
 * Turn based house infiltrator with small player-controller squads.
 * Multiplayer ideally.
 * 
 * Large scale war 'simulator' with super simple graphics, control of troop position etc.
 * Have to manage production of stuff to supply your army. Aim of the game is to take over map.
 * Match duration: 20-80 minutes depending on map size and difficulty.
 */

namespace Expansion
{
    class Program
    {
        private static void Main(string[] args)
        {
            JEngine.UponRegisterScreens += (sm) =>
            {
                sm.RegisterNew(new BaseScreen()).Active = true;
            };

            JEngine.Start();
        }
    }

    class BaseScreen : GameScreen
    {
        public const float CHUNK_UNLOAD_TIME = 0.1f;

        private Sprite TreeLines, TreeColor;
        private Sprite NoiseTileSprite;
        private TileDef NoiseTileDef;
        private TileLayer Layer;

        public BaseScreen() : base("Base Screen")
        {
        }

        public override void Init()
        {
            JEngine.ScreenManager.GetScreen<CameraMoveScreen>().Active = true;
            Layer = new TileLayer();
        }

        public override void LoadContent(Content contentManager)
        {
            TreeLines = contentManager.Load<Sprite>("NewTreeLines");
            TreeColor = contentManager.Load<Sprite>("NewTreeColor");
            NoiseTileSprite = contentManager.Load<Sprite>("TileNoise");
            NoiseTileDef = new TileDef(1, "Test Tile");
            NoiseTileDef.Sprite = NoiseTileSprite;
            TileDef.Register(NoiseTileDef);
        }

        private List<long> toBin = new List<long>();
        public override void Update()
        {
            if (Input.KeyPressed(Keys.C))
                return;

            const int MAX_PER_FRAME = 5;
            int count = 0;

            foreach (var chunk in Layer.GetLoadedChunks())
            {
                if (chunk != null)
                {
                    if (chunk.TimeSinceVisible > CHUNK_UNLOAD_TIME && chunk.FramesSinceVisible > 3)
                    {
                        toBin.Add(chunk.ID);
                        continue;
                    }
                    chunk.TimeSinceVisible += Time.unscaledDeltaTime;
                    chunk.FramesSinceVisible++;

                    if (chunk.RequiresRedraw)
                    {
                        RequestRenderTargetDraw(new TargetRenderRequest(chunk.RT, true) { CustomData = chunk });
                        count++;
                        if (count >= MAX_PER_FRAME)
                            break;
                    }
                }
            }
            foreach (var id in toBin)
            {
                Layer.UnloadChunk(id);
            }
            toBin.Clear();
        }

        protected override void DrawRenderTarget(TargetRenderRequest request, SpriteBatch spr)
        {
            var chunk = request.CustomData as Chunk;
            if (chunk != null)
            {
                chunk.Redraw(spr);
            }
        }

        public override void Draw(SpriteBatch spr)
        {
            spr.Draw(TreeLines, Vector2.Zero, Color.White);
            spr.Draw(TreeColor, Vector2.Zero, Color.ForestGreen);

            const int PADDING = 1;
            int minX = (int)Math.Floor((float)JEngine.Camera.WorldViewBounds.X / (Tile.SIZE * Chunk.SIZE)) - PADDING;
            int minY = (int)Math.Floor((float)JEngine.Camera.WorldViewBounds.Y / (Tile.SIZE * Chunk.SIZE)) - PADDING;
            int maxX = minX + (int)Math.Ceiling((float)JEngine.Camera.WorldViewBounds.Width / (Tile.SIZE * Chunk.SIZE)) + PADDING * 2;
            int maxY = minY + (int)Math.Ceiling((float)JEngine.Camera.WorldViewBounds.Width / (Tile.SIZE * Chunk.SIZE)) + PADDING * 2;

            Debug.Text($"Chunk Bounds: {minX}->{maxX}, {minY}->{maxY}.");
            Debug.Text($"Cam pos: {JEngine.Camera.Position}");
            Debug.Text($"Chunks in memory: {Chunk.TotalCount}");
            long bytesPerChunk = Chunk.SIZE * Chunk.SIZE * Tile.SIZE * Tile.SIZE;
            long totalBytes = Chunk.TotalCount * bytesPerChunk;
            Debug.Text($"EST. Chunk video mem: {totalBytes / (1024 * 256)} MB.");
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!Layer.IsChunkLoaded(x, y))
                        Layer.LoadChunk(x, y);
                }
            }

            foreach (var chunk in Layer.GetLoadedChunks())
            {
                if (chunk.X < minX || chunk.X > maxX)
                    continue;
                if (chunk.Y < minY || chunk.Y > maxY)
                    continue;

                spr.Draw(chunk.RT, new Vector2(chunk.X * Chunk.SIZE * Tile.SIZE, chunk.Y * Chunk.SIZE * Tile.SIZE), Color.White);
                chunk.TimeSinceVisible = 0f;
                chunk.FramesSinceVisible = 0;
            }
        }
    }
}
