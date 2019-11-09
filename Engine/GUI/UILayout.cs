using Microsoft.Xna.Framework;

namespace Engine.GUI
{
    public static class UILayout
    {
        // The area stack stores the current position and the bounds of the current area.
        private static UIStack stack = new UIStack();

        internal static void FrameReset()
        {
            stack.Reset(Point.Zero, UIFlow.Vertical);
        }

        private static void UpdateArea(Rectangle area)
        {
            // Got to update the current position and bounds.
            var flow = stack.GetCurrentFlow();
            var point = GetUpdatedPoint(stack.GetCurrentFlow(), stack.GetCurrentPos(), area);

            stack.UpdateCurrentPos(point);
            stack.UpdateCurrentBounds(area);
        }

        internal static Point GetUpdatedPoint(UIFlow flow, Point start, Rectangle area, int padding = 2)
        {
            switch (flow)
            {
                case UIFlow.Horizontal:
                    return start + new Point(area.Width + padding, 0);

                case UIFlow.Vertical:
                    return start + new Point(0, area.Height + padding);

                default:
                    return Point.Zero;
            }
        }

        public static bool Button(string text, Color color)
        {
            Point pos = stack.GetCurrentPos();
            var bounds = UI.DrawButton(text, pos.ToVector2(), color, out bool clicked);
            UpdateArea(bounds);
            return clicked;
        }

        #region Area updates

        public static void BeginHorizontal()
        {
            stack.Push(stack.GetCurrentPos(), UIFlow.Horizontal);
        }

        public static void EndHorizontal()
        {
            stack.Pop(true);
        }

        public static void BeginVertical()
        {
            stack.Push(stack.GetCurrentPos(), UIFlow.Vertical);
        }

        public static void EndVertical()
        {
            stack.Pop(true);
        }

        #endregion
    }
}
