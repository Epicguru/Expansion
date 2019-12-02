using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Screens
{
    /// <summary>
    /// A game screen is an object that allows you to hook into the content loading, updating and rendering
    /// loop. They can be used to represent actual in-game screens, such as the main menu, or they can be used
    /// as utility classes such as to update a group of entities or to draw certain parts of UI.
    /// </summary>
    public abstract class GameScreen
    {
        internal static int RTDrawCount = 0;

        public string Name { get; protected set; }
        /// <summary>
        /// Gets or sets the active state of this screen. When a screen is not active, it is not updated or
        /// drawn. When active, it is only drawn if <see cref="Visible"/> is true (defautl is true).
        /// Note that when setting this value, the changed value will not be reflected until next frame unless
        /// changed between the update and draw calls, in which case it will be updated before the draw call,
        /// preventing the screen from drawing.
        /// </summary>
        public bool Active { get { return realActive; } set { targetActive = value; } }
        /// <summary>
        /// Gets or sets the visible state of this screen. A screen that is not visible will be updated but not drawn.
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// The unique id of this screen. This is automatically assigned when registered to a 
        /// <see cref="ScreenManager"/>.
        /// </summary>
        public ushort ID { get; internal set; }

        internal bool targetActive = false;
        internal bool realActive = false;
        internal List<TargetRenderRequest> renderTargetsToDraw = new List<TargetRenderRequest>();

        public GameScreen(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Called when the game is initializing. Only called if the screen in registered early in startup!
        /// Default implmenetation does nothing.
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// Called when the game is loading content. Use this to load any content that this screen
        /// or related components need. You should also build GeonBit UI here since it cannot be done in Init().
        /// Only called if the screen is registered early in startup!
        /// Default implementation does nothing.
        /// </summary>
        /// <param name="content">The default content manager that you can load content with.</param>
        public virtual void LoadContent(JContent content)
        {

        }

        internal void InternalUpdate()
        {
            if(Active)
                this.Update();
        }

        /// <summary>
        /// Called every frame, you should update your objects here and perform input logic.
        /// Use <see cref="Time.deltaTime"/> to make your app frame-rate independent.
        /// Default implementation does nothing, so you don't need to call base.Update();
        /// </summary>
        public virtual void Update()
        {

        }

        internal void InternalDraw(SpriteBatch spr)
        {
            spr.DrawString(Engine.Screens.LoadingScreen.font, "Yeet", new Vector2(200, 100), Color.White);

            for (int i = 0; i < renderTargetsToDraw.Count; i++)
            {
                var request = renderTargetsToDraw[i];

                if (request.IsPreDraw)
                {
                    renderTargetsToDraw.RemoveAt(i);
                    i--;

                    spr.End();
                    Debug.Assert(request.RenderTarget != null, "Render target should not be null at this point.");
                    JEngine.MainGraphicsDevice.SetRenderTarget(request.RenderTarget);
                    spr.Begin(request.SortMode);
                    this.DrawRenderTarget(request, spr);
                    spr.End();
                    JEngine.MainGraphicsDevice.SetRenderTarget(null);
                    SamplerState s = JEngine.Camera.Zoom > 1 ? SamplerState.PointClamp : SamplerState.LinearClamp;
                    spr.Begin(SpriteSortMode.Deferred, null, s, null, null, null, JEngine.Camera.GetMatrix());

                    RTDrawCount++;
                }
            }

            if(Active && Visible)
                this.Draw(spr);

            for (int i = 0; i < renderTargetsToDraw.Count; i++)
            {
                var request = renderTargetsToDraw[i];

                if (!request.IsPreDraw)
                {
                    renderTargetsToDraw.RemoveAt(i);
                    i--;

                    spr.End();
                    JEngine.MainGraphicsDevice.SetRenderTarget(request.RenderTarget);
                    spr.Begin(request.SortMode);
                    this.DrawRenderTarget(request, spr);
                    spr.End();
                    JEngine.MainGraphicsDevice.SetRenderTarget(null);
                    SamplerState s = JEngine.Camera.Zoom > 1 ? SamplerState.PointClamp : SamplerState.LinearClamp;
                    spr.Begin(SpriteSortMode.Deferred, null, s, null, null, null, JEngine.Camera.GetMatrix());

                    RTDrawCount++;
                }
            }
        }

        /// <summary>
        /// Adds a request for the drawing to a render target to take place either at the end of this frame or at the
        /// beginning of the next frame.
        /// </summary>
        protected void RequestRenderTargetDraw(TargetRenderRequest request)
        {
            if(request.RenderTarget == null)
            {
                Debug.Error("Null render target, cannot request draw.");
                return;
            }

            renderTargetsToDraw.Add(request);
        }

        /// <summary>
        /// Called whenever <see cref="RequestRenderTargetDraw(RenderTarget2D, bool)"/> is used to request a draw to a target.
        /// All the draw code within this method (such as spritebatch.Draw(texture, pos, color)) will write to the render target and not to the screen.
        /// Default implementation does nothing.
        /// </summary>
        /// <param name="target">The render target that is being drawn to.</param>
        /// <param name="spr">The spritebatch you should draw with.</param>
        /// <param name="isPreDraw">If true, this method is being called from before a draw call. If false, it is being called after a draw call.</param>
        protected virtual void DrawRenderTarget(TargetRenderRequest request, SpriteBatch spr)
        {

        }

        /// <summary>
        /// Called whenever the game world is being drawn, and this screen in visible.
        /// Default implementation does nothing, so you don't have to call base.OnDraw();
        /// </summary>
        /// <param name="spr">The spritebatch that you can use to draw.</param>
        public virtual void Draw(SpriteBatch spr)
        {

        }

        internal void InternalDrawUI(SpriteBatch spr)
        {
            if (Active && Visible)
                DrawUI(spr);
        }

        /// <summary>
        /// Called once per frame to draw any UI elements.
        /// </summary>
        /// <param name="spr">The spritebatch that you can use to draw UI elements. May not be necessary
        /// if you are using built-in UI components.</param>
        public virtual void DrawUI(SpriteBatch spr)
        {

        }

        public virtual void OnEnabled()
        {

        }

        public virtual void OnDisabled()
        {

        }

        /// <summary>
        /// Called when the game is closing.
        /// </summary>
        public virtual void OnClosing()
        {

        }
    }

    public struct TargetRenderRequest
    {
        public bool IsPreDraw;
        public RenderTarget2D RenderTarget;
        public SpriteSortMode SortMode;
        // TODO add other spritebatch options
        // ...
        public object CustomData;

        public TargetRenderRequest(RenderTarget2D target, bool preDraw)
        {
            RenderTarget = target;
            IsPreDraw = preDraw;
            CustomData = null;
            SortMode = SpriteSortMode.Deferred;
        }
    }
}
