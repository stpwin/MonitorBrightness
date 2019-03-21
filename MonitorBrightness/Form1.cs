using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunifu.Framework.UI;

namespace MonitorBrightness
{
    public partial class Form1 : Form
    {

        BrightnessControl brightnessControl = new BrightnessControl();

        public Form1()
        {
            InitializeComponent();
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            

            int h = 0;
            for(int i = 0; i < 3; i++)
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
                sliderControl.ValueChanged += SliderControl_ValueChanged;
                flowLayoutPanel1.Controls.Add(sliderControl);
                h += 25;
            }
            this.Size = new Size(this.Size.Width, this.Size.Height + h);
            Point newLocation = new Point(screen.Width - this.Size.Width, screen.Height - this.Size.Height);
            Location = newLocation;
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
                //Console.WriteLine(ct.Value);
            }
            
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Hide();
        }
    }
}
