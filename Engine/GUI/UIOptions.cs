namespace Engine.GUI.Options
{
    // TODO implementme.
    public struct UIOptions
    {
        public static readonly UIOptions Default = new UIOptions()
        {
            WidthMode = ExpansionMode.Default,
            HeightMode = ExpansionMode.Default,
            FixedWidth = 0,
            FixedHeight = 0
        };

        public ExpansionMode WidthMode;
        public ExpansionMode HeightMode;

        public int FixedWidth;
        public int FixedHeight;
    }

    public enum ExpansionMode
    {
        Default,
        Expand,
        Fixed
    }
}
