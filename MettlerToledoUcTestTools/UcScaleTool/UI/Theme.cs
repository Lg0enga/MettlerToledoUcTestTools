using System.Drawing;
using System.Windows.Forms;

namespace UcScaleTool.UI
{
    /// <summary>
    /// Colors, fonts and control factories for a flat, touch friendly look.
    /// Works on Windows XP (falls back to Tahoma when Segoe UI isn't installed).
    /// </summary>
    internal static class Theme
    {
        public static readonly Color Header = Color.FromArgb(27, 42, 65);
        public static readonly Color NavBar = Color.FromArgb(36, 54, 79);
        public static readonly Color NavSelected = Color.FromArgb(0, 120, 212);
        public static readonly Color Background = Color.FromArgb(238, 241, 245);
        public static readonly Color Card = Color.White;
        public static readonly Color CardBorder = Color.FromArgb(218, 223, 230);
        public static readonly Color Text = Color.FromArgb(33, 37, 41);
        public static readonly Color MutedText = Color.FromArgb(108, 117, 125);

        public static readonly Color Primary = Color.FromArgb(0, 103, 177);
        public static readonly Color Success = Color.FromArgb(40, 140, 80);
        public static readonly Color Danger = Color.FromArgb(200, 55, 55);
        public static readonly Color Neutral = Color.FromArgb(84, 99, 120);
        public static readonly Color Warning = Color.FromArgb(230, 150, 30);

        public static readonly Color Display = Color.FromArgb(15, 27, 45);
        public static readonly Color DisplayText = Color.FromArgb(120, 240, 160);

        private static string _fontFamily;

        public static string FontFamily
        {
            get
            {
                if (_fontFamily == null)
                {
                    using (Font probe = new Font("Segoe UI", 10f))
                        _fontFamily = probe.Name == "Segoe UI" ? "Segoe UI" : "Tahoma";
                }
                return _fontFamily;
            }
        }

        public static Font CreateFont(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font(FontFamily, size, style);
        }

        public static Font CreateMonoFont(float size)
        {
            using (Font probe = new Font("Consolas", size))
            {
                if (probe.Name == "Consolas")
                    return new Font("Consolas", size);
            }
            return new Font("Courier New", size);
        }

        public static Button CreateButton(string text, Color color, float fontSize = 12f)
        {
            Button button = new Button();
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.15f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Font = CreateFont(fontSize, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(6);
            button.UseVisualStyleBackColor = false;
            button.EnabledChanged += delegate
            {
                button.BackColor = button.Enabled ? color : Color.FromArgb(190, 196, 204);
            };
            return button;
        }

        public static void SetButtonColor(Button button, Color color)
        {
            button.BackColor = color;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.15f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
        }

        /// <summary>A white card with a title; add content to the returned card's Body.</summary>
        public static CardPanel CreateCard(string title)
        {
            return new CardPanel(title);
        }

        public static Label CreateFieldLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Font = CreateFont(10.5f);
            label.ForeColor = Text;
            return label;
        }
    }

    internal class CardPanel : Panel
    {
        public readonly Panel Body;
        public readonly Label TitleLabel;

        public CardPanel(string title)
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Card;
            Padding = new Padding(16, 12, 16, 16);
            Margin = new Padding(8);
            DoubleBuffered = true;

            Body = new Panel();
            Body.Dock = DockStyle.Fill;
            Body.BackColor = Theme.Card;

            TitleLabel = new Label();
            TitleLabel.Text = title;
            TitleLabel.Dock = DockStyle.Top;
            TitleLabel.Height = 36;
            TitleLabel.Font = Theme.CreateFont(14f, FontStyle.Bold);
            TitleLabel.ForeColor = Theme.Text;

            Controls.Add(Body);
            Controls.Add(TitleLabel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Theme.CardBorder))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        protected override void OnResize(System.EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Invalidate();
        }
    }
}
