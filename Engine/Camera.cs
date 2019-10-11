using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine
{
    public class Camera
    {
        public Vector2 Position;
        public float Rotation { get; set; }
        public float Zoom
        {
            get
            {
                return this._zoom;
            }
            set
            {
                this._zoom = Math.Max(Math.Min(value, 10), 0.005f);
            }
        }
        public Rectangle WorldViewBounds { get; private set; }
        public Rectangle WorldTileBounds { get; private set; }
        public Rectangle WorldChunkBounds { get; private set; }
        public bool UpdateViewBounds { get; set; } = true;

        private float _zoom = 1f;
        private Matrix _matrix;
        private Matrix _inverted;

        public void UpdateMatrix(GraphicsDevice graphicsDevice)
        {
            _matrix =
              Matrix.CreateTranslation(new Vector3(-(int)Position.X, -(int)Position.Y, 0)) *
                                         Matrix.CreateRotationZ(MathHelper.ToRadians(-Rotation)) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));

            var matrix = this.GetMatrix();
            _inverted = Matrix.Invert(matrix);

            if (!UpdateViewBounds)
                return;

            var topLeft = Vector2.Transform(new Vector2(0, 0), _inverted);
            var bottomRight = Vector2.Transform(new Vector2(Screen.Width, Screen.Height), _inverted);

            var r = WorldViewBounds;
            r.X = (int)topLeft.X;
            r.Y = (int)topLeft.Y;
            r.Width = (int)Math.Ceiling(bottomRight.X - topLeft.X);
            r.Height = (int)Math.Ceiling(bottomRight.Y - topLeft.Y);
            WorldViewBounds = r;

            var tb = WorldTileBounds;
            tb.X = PixelToTile(r.X);
            tb.Y = PixelToTile(r.Y);
            tb.Width = PixelToTile(r.Right) - tb.X + 1;
            tb.Height = PixelToTile(r.Bottom) - tb.Y + 1;
            WorldTileBounds = tb;
        }

        public int PixelToTile(int pixelCoord)
        {
            return (int)Math.Floor((double)pixelCoord / 16);
        }

        public Vector2 ScreenToWorldPosition(Vector2 screenPos)
        {
            return Vector2.Transform(screenPos, _inverted);
        }

        public Vector2 WorldToScreenPositon(Vector2 worldPos)
        {
            return Vector2.Transform(worldPos, _matrix);
        }

        public Matrix GetMatrix()
        {
            return _matrix;
        }
    }
}
