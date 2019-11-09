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
        /// <summary>
        /// The current in-game camera bounds, measured in pixels.
        /// Use this to perform culling.
        /// </summary>
        public Rectangle WorldViewBounds { get; private set; }
        /// <summary>
        /// When true, the <see cref="WorldViewBounds"/> is updated every frame, otherwise will not be updated.
        /// Default is true.
        /// </summary>
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
        }

        /// <summary>
        /// Converts a screen-space position to a world-space position.
        /// 
        /// If you are looking for the world mouse position, you should use <see cref="Input.MouseWorldPos"/>.
        /// </summary>
        /// <param name="screenPos">The screen space position.</param>
        /// <returns>The world space position corresponding to the screen space one.</returns>
        public Vector2 ScreenToWorldPosition(Vector2 screenPos)
        {
            return Vector2.Transform(screenPos, _inverted);
        }

        public Vector2 WorldToScreenPositon(Vector2 worldPos)
        {
            return Vector2.Transform(worldPos, _matrix);
        }

        public bool ContainsPoint(Vector2 point, float padX, float padY)
        {
            if(padX == 0 && padY == 0)
            {
                return WorldViewBounds.Contains(point);
            }
            else
            {
                var bounds = WorldViewBounds;
                bounds.Inflate(padX, padY);
                return bounds.Contains(point);
            }
        }

        public Matrix GetMatrix()
        {
            return _matrix;
        }
    }
}
