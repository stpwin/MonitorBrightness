using System;
using System.Drawing;
using System.Windows.Forms;
using Bunifu.Framework.UI;
using System.Runtime.InteropServices;
using Gma.System.MouseKeyHook;

namespace MonitorBrightness
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);


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
                //sliderControl.MouseWheel += SliderControl_MouseWheel;
                sliderControl.MouseEnter += SliderControl_MouseEnter;
                sliderControl.ValueChanged += SliderControl_ValueChanged;
                flowLayoutPanel1.Controls.Add(sliderControl);
                //sliderControl.Focus();
                //sliderControl.Select();
                h += 25;
            }
            this.Size = new Size(this.Size.Width, this.Size.Height + h);
            Point newLocation = new Point(screen.Width - this.Size.Width, screen.Height - this.Size.Height);
            Location = newLocation;
            flowLayoutPanel1.Select();
            
            Subscribe(Hook.GlobalEvents());
        }

        private void SliderControl_MouseEnter(object sender, EventArgs e)
        {
            focusControl = (BunifuSlider)sender;
        }

        private IKeyboardMouseEvents m_Events;

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            m_Events.MouseWheel += M_GlobalHook_MouseWheel;
        }


        BunifuSlider focusControl;
        private void M_GlobalHook_MouseWheel(object sender, MouseEventArgs e)
        {
            //Console.WriteLine(e.Delta);
            if (flowLayoutPanel1.Controls.Count > 0)
            {
                if (focusControl != null)
                {
                    SliderControl_MouseWheel(focusControl, e);
                } else
                {
                    focusControl = (BunifuSlider)flowLayoutPanel1.Controls[0];
                }
                
                //MouseWheelRedirector.Attach((BunifuSlider)flowLayoutPanel1.Controls[0]);
                //().Focus();
            }
        }

        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseWheel -= M_GlobalHook_MouseWheel;
            m_Events.Dispose();
            m_Events = null;
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
            if (timer2.Enabled) timer2.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GetForegroundWindow() != Handle)
            {
                Unsubscribe();
                Hide();
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Unsubscribe();
            Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
                Subscribe(Hook.GlobalEvents());
                SetForegroundWindow(Handle);
                //BringToFront();
                //BringToFront();
                //Focus();
                //Activate();

            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Hide();
            Unsubscribe();
            timer2.Stop();
        }
    }
}
