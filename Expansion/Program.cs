﻿using Engine;
using Engine.Screens;
using Engine.Sprites;
using Engine.Tiles;
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

    internal class BaseScreen : GameScreen
    {
        public const float CHUNK_UNLOAD_TIME = 0.1f;

        internal static Sprite TreeLines, TreeColor;
        public static Sprite MissileSprite;
        private Sprite NoiseTileSprite;
        private TileDef NoiseTileDef;
        private TileLayer Layer { get { return JEngine.TileMap; } }

        public BaseScreen() : base("Base Screen")
        {
        }

        public override void Init()
        {
            JEngine.ScreenManager.GetScreen<CameraMoveScreen>().Active = true;
        }

        public override void LoadContent(Content contentManager)
        {
            TreeLines = contentManager.Load<Sprite>("NewTreeLines");
            TreeColor = contentManager.Load<Sprite>("NewTreeColor");
            NoiseTileSprite = contentManager.Load<Sprite>("TileNoise");
            MissileSprite = contentManager.Load<Sprite>("Missile");
            NoiseTileDef = new TileDef(1, "Test Tile");
            NoiseTileDef.Sprite = NoiseTileSprite;
            TileDef.Register(NoiseTileDef);
            TileDef.Register(new TestLinkedTile());
        }

        private List<long> toBin = new List<long>();
        public override void Update()
        {
            if (Input.KeyDown(Keys.F11))
            {
                Screen.ToggleFullscreen();
            }

            if (Input.KeyPressed(Keys.F))
            {
                for (int i = 0; i < 50; i++)
                {
                    var e = new TestEntity();
                    e.Center = Input.MouseWorldPos;
                    e.Velocity = Rand.UnitCircle() * Rand.Range(0.25f, 10f) * Tile.SIZE;
                }
            }

            if (Input.KeyPressed(Keys.Y))
            {
                var e = new TestActive();
                e.Center = Input.MouseWorldPos;
            }

            if (Input.KeyPressed(Keys.L))
            {
                float r = Tile.SIZE * 5;
                foreach (var entity in JEngine.Entities.GetAllInRange(Input.MouseWorldPos, r))
                {
                    entity.Destroy();
                }
            }

            if (Input.KeyDown(Keys.Space))
                System.GC.Collect();

            if (Input.KeyDown(Keys.T))
            {
                var missile = new MissileEntity();
                missile.Position = Input.MouseWorldPos - missile.Size * 0.5f;
            }

            if (Input.KeyPressed(Keys.V))
            {
                var p = JEngine.TileMap.PixelToTileCoords((int)Input.MouseWorldPos.X, (int)Input.MouseWorldPos.Y);
                JEngine.TileMap.SetTile(p.X, p.Y, 1, new Tile(2, ColorCache.EnsureColor(Color.White)));
            }

            if (Input.KeyPressed(Keys.C))
                return;

            const int MAX_PER_FRAME = 5;
            int count = 0;

            foreach (var chunk in Layer.GetLoadedChunks())
            {
                if (chunk != null)
                {
                    if (chunk.EntityCount != 0)
                    {
                        chunk.FlagAsNeeded();                        
                    }

                    if(chunk.TimeSinceNeeded > CHUNK_UNLOAD_TIME && chunk.FramesSinceNeeded > 2)
                    {
                        toBin.Add(chunk.ID);
                        continue;
                    }
                    if(chunk.TimeSinceRendered > CHUNK_UNLOAD_TIME && chunk.FramesSinceRendered > 2)
                    {
                        chunk.UnloadGraphics();
                    }
                    chunk.Decay(Time.unscaledDeltaTime);

                    if (chunk.RequiresRedraw && count < MAX_PER_FRAME && chunk.Graphics.Texture != null)
                    {
                        count++;
                        RequestRenderTargetDraw(new TargetRenderRequest(chunk.Graphics.Texture, true) { CustomData = chunk });
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

            // Debug texts.
            Debug.Text($"Chunk Bounds: {minX}->{maxX}, {minY}->{maxY}.");
            Debug.Text($"Cam pos: {JEngine.Camera.Position}");
            Debug.Text($"Chunks in memory: {Chunk.TotalCount}");
            Debug.Text($"Chunks loaded: {Layer.LoadedChunkCount}");
            Debug.Text($"Chunks textures in vid.mem.: {ChunkGraphics.TotalTextureCount}");
            long bytesPerChunk = Chunk.SIZE * Chunk.SIZE * Tile.SIZE * Tile.SIZE;
            long totalBytes = ChunkGraphics.TotalTextureCount * bytesPerChunk;
            Debug.Text($"EST. Chunk video mem: {totalBytes / (1024 * 256)} MB.");

            int count = 0;
            foreach (var item in GraphicsAdapter.Adapters)
            {
                Debug.Text($"-- Graphics Adapter {count}: {item.DeviceName.ToLower()} {(item.IsDefaultAdapter ? "[Default]" : "")} --");
                count++;

                Debug.Text($"[{item.DeviceId}]: {item.Description}");
                Debug.Text($"Curernt display mode: {item.CurrentDisplayMode.Width}x{item.CurrentDisplayMode.Height}, {item.CurrentDisplayMode.Format}");
                Debug.Text($"Widescreen? {item.IsWideScreen}");
                Debug.Text($"Vendor ID: {item.VendorId}");
            }

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (!Layer.IsChunkLoaded(x, y))
                    {
                        var loaded = Layer.LoadChunk(x, y);
                        loaded.RequestRedraw();
                    }
                    else
                    {
                        var c = Layer.GetChunk(x, y);
                        if (!c.Graphics.CanRender)
                            c.RequestRedraw();
                    }
                }
            }

            foreach (var chunk in Layer.GetLoadedChunks())
            {
                if (chunk.X < minX || chunk.X > maxX)
                    continue;
                if (chunk.Y < minY || chunk.Y > maxY)
                    continue;

                if(chunk.Graphics.CanRender)
                    spr.Draw(chunk.Graphics.Texture, new Vector2(chunk.X * Chunk.SIZE * Tile.SIZE, chunk.Y * Chunk.SIZE * Tile.SIZE), Color.White);

                chunk.FlagAsNeeded();
                chunk.FlagAsRendered();
            }
        }

        public override void DrawUI(SpriteBatch spr)
        {
            spr.Draw(JEngine.MainAtlas.Texture, new Vector2(10, 10), Color.White);
        }
    }
}
