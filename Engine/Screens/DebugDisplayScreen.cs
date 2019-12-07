using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GeonBit.UI;
using GeonBit.UI.Entities;
using System;

namespace Engine.Screens
{
    internal class DebugDisplayScreen : GameScreen
    {
        private SpriteFont Font { get { return LoadingScreen.font; } }

        private Panel MainPanel;
        private Label ThreadPendingLabel;
        private Paragraph[] ThreadLabels;
        private ProgressBar[] ThreadBars;

        public DebugDisplayScreen() : base("Debug Display")
        {
            Visible = false;
        }

        public override void LoadContent(JContent content)
        {
            MainPanel = new Panel(new Vector2(400, 320), PanelSkin.Simple, Anchor.BottomRight, new Vector2(10, 10));
            MainPanel.Draggable = true;
            PanelTabs tabs = new PanelTabs(PanelSkin.None);
            MainPanel.AddChild(tabs);
            var threadTab = tabs.AddTab("Pathing");
            threadTab.button.MaxSize = new Vector2(500, 32);
            var threadPanel = threadTab.panel;
            threadPanel.AddChild(new CheckBox("Draw Paths")
            {
                OnValueChange = (e) =>
                {
                    JEngine.Entities.DrawPathfindingDebug = (e as CheckBox).Checked;
                },
                Checked = false
            });
            threadPanel.AddChild(ThreadPendingLabel = new Label("Pending: 0"));
            threadPanel.AdjustHeightAutomatically = true;
            ThreadLabels = new Paragraph[JEngine.PathfindingThreadCount];
            ThreadBars = new ProgressBar[JEngine.PathfindingThreadCount];
            for (int i = 0; i < JEngine.PathfindingThreadCount; i++)
            {
                threadPanel.AddChild(ThreadLabels[i] = new Paragraph($"{i}: 0%, Avg: 0.0ms, Max: 0.0ms") { Padding = new Vector2(0, 0), Scale = 0.5f});
                threadPanel.AddChild(ThreadBars[i] = new ProgressBar(0, 100) { Value = 40, Size = new Vector2(-1, 24), Padding = Vector2.Zero });
            }

            UserInterface.Active.AddEntity(MainPanel);
        }

        public override void Update()
        {
            if (Input.KeyDown(Keys.F1))
                Visible = !Visible;

            MainPanel.Visible = Visible;

            ThreadPendingLabel.Text = $"Pending: {JEngine.Pathfinding.PendingCount}, Req. per second: {JEngine.Pathfinding.ProcessedLastSecond}";
            var stats = JEngine.Pathfinding.Statistics;
            for (int i = 0; i < stats.Length; i++)
            {
                var stat = stats[i];

                ThreadLabels[i].Text = $"{i}: {stat.AverageUsage * 100f:F1}%, Avg: {stat.MeanProcessTime:F2}ms, Max: {stat.MaxProcessTime:F2}ms, Prc: {stat.ProcessedLastSecond}";
                ThreadBars[i].Value = (int)Math.Round(stat.AverageUsage * 100f);
            }
        }

        public override void DrawUI(SpriteBatch spr)
        {
            int y = 140;

            for(int i = 0; i < Debug.DebugTexts.Count; i++)
            {
                string text = Debug.DebugTexts[i];
                var size = Font.MeasureString(text);

                int x = Screen.Width - ((int)size.X + 5);
                spr.DrawString(Font, text, new Vector2(x, y), Color.Black);
                y += (int)size.Y;
            }
        }
    }
}
