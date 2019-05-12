using System;
using System.Drawing;
using System.Windows.Forms;
using Bunifu.Framework.UI;
using System.Runtime.InteropServices;

namespace MonitorBrightness
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        BrightnessControl brightnessControl = new BrightnessControl();

        public Form1()
        {
            InitializeComponent();
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            
            int h = 0;
            for(int i = 0; i < brightnessControl.GetMonitors(); i++)
            {
                var brightnessInfo = brightnessControl.GetBrightnessCapabilities(i);
                var sliderControl = new BunifuSlider
                {
                    Name = $"slider{i}",
                    Tag = i,
                    IndicatorColor = Color.FromArgb(255, 26, 177, 136),
                    Size = new Size(313, 30),
                    BorderRadius = 3,
                    Value = brightnessInfo.current,
                    MaximumValue = brightnessInfo.maximum
                };
                sliderControl.MouseWheel += SliderControl_MouseWheel;
                sliderControl.ValueChanged += SliderControl_ValueChanged;
                flowLayoutPanel1.Controls.Add(sliderControl);
                h += 25;
            }
            this.Size = new Size(this.Size.Width, this.Size.Height + h);
            Point newLocation = new Point(screen.Width - this.Size.Width, screen.Height - this.Size.Height);
            Location = newLocation;
        }

        private void SliderControl_MouseWheel(object sender, MouseEventArgs e)
        {
            BunifuSlider ctl = sender as BunifuSlider;
            int value = ctl.Value + (5 * e.Delta / 120);
            if (value > ctl.MaximumValue || value < 0) return;
            ctl.Value = value;
            SliderControl_ValueChanged(sender, null);

        }

        int current = 0;
        private void SliderControl_ValueChanged(object sender, EventArgs e)
        {
            BunifuSlider ct = (BunifuSlider)sender;
            int id = (int)ct.Tag;
            if (current != ct.Value)
            {
                current = ct.Value;
                brightnessControl.SetBrightness((short)ct.Value, id);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GetForegroundWindow() != Handle)
            {
                Hide();
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                BringToFront();
                Show();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Hide();
            timer2.Stop();
        }
    }
}
