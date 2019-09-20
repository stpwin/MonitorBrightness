using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Bunifu.Framework.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gma.System.MouseKeyHook;

namespace MonitorBrightness
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        private readonly BrightnessControl _brightnessControl = new BrightnessControl();
        private IKeyboardMouseEvents _mEvents;
        private BunifuSlider _focusControl;
        private int _current;
        private int[] lastValues;
        private int[] newValues;
        private bool[] valuesChanged;
        private uint monitorCount;

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private async void SetMonitorBrightness(int value, int monitor)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                await Task.Run(() => _brightnessControl.SetBrightness((short)value, monitor));
                Console.WriteLine(value);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public Form1()
        {
            InitializeComponent();
            monitorCount = _brightnessControl.GetMonitors();
            var screen = Screen.PrimaryScreen.WorkingArea;
            var h = 0;
            lastValues = new int[monitorCount];
            newValues = new int[monitorCount];
            valuesChanged = new bool[monitorCount];
            for (var i = 0; i < monitorCount; i++)
            {
                var brightnessInfo = _brightnessControl.GetBrightnessCapabilities(i);
                lastValues[i] = brightnessInfo.current;
                newValues[i] = brightnessInfo.current;
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
                sliderControl.MouseEnter += SliderControl_MouseEnter;
                sliderControl.ValueChanged += SliderControl_ValueChanged;
                flowLayoutPanel1.Controls.Add(sliderControl);
                h += 25;
            }
            Size = new Size(this.Size.Width, this.Size.Height + h);
            var newLocation = new Point(screen.Width - this.Size.Width, screen.Height - this.Size.Height);
            Location = newLocation;
            flowLayoutPanel1.Select();
            
            Subscribe(Hook.GlobalEvents());
        }

        private void SliderControl_MouseEnter(object sender, EventArgs e)
        {
            _focusControl = (BunifuSlider)sender;
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            _mEvents = events;
            _mEvents.MouseWheel += M_GlobalHook_MouseWheel;
        }

        private void M_GlobalHook_MouseWheel(object sender, MouseEventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count <= 0) return;
            if (_focusControl != null)
            {
                SliderControl_MouseWheel(_focusControl, e);
            } else
            {
                _focusControl = (BunifuSlider)flowLayoutPanel1.Controls[0];
            }
        }

        private void Unsubscribe()
        {
            if (_mEvents == null) return;
            _mEvents.MouseWheel -= M_GlobalHook_MouseWheel;
            _mEvents.Dispose();
            _mEvents = null;
        }

        private void SliderControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!(sender is BunifuSlider ctl)) return;
            var value = ctl.Value + (5 * e.Delta / 120);
            if (value > ctl.MaximumValue) value = ctl.MaximumValue;
            if (value < 0) value = 0;
            if (ctl.Value == value) return;
            ctl.Value = value;
            //Console.WriteLine(value);
            SliderControl_ValueChanged(sender, null);

        }

        private int step = 5;
        private void SliderControl_ValueChanged(object sender, EventArgs e)
        {
            timerHide.Stop();
            var ct = (BunifuSlider)sender;
            if (_current == ct.Value) return;
            _current = ct.Value;
            var curMonitor = (int) ct.Tag;
            newValues[curMonitor] = _current;
            valuesChanged[curMonitor] = true;

            while (lastValues[curMonitor] != _current)
            {
                if (lastValues[curMonitor] > _current)
                {
                    lastValues[curMonitor] -= step;
                    if (lastValues[curMonitor] < _current) lastValues[curMonitor] = _current;
                }
                else if (lastValues[curMonitor] < _current)
                {
                    lastValues[curMonitor] += step;
                    if (lastValues[curMonitor] > _current) lastValues[curMonitor] = _current;
                }
                SetMonitorBrightness(lastValues[curMonitor], curMonitor);
            }

        }

        private bool lastForground = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            bool isForground = GetForegroundWindow() == Handle;
            if (isForground != lastForground)
            {
                lastForground = isForground;
                if (isForground == false)
                {
                    Hide();
                    Unsubscribe();
                    //Console.WriteLine($"Hide {DateTime.Now}");
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Unsubscribe();
            Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Show();
            Subscribe(Hook.GlobalEvents());
            SetForegroundWindow(Handle);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Hide();
            Unsubscribe();
            timerHide.Stop();
        }

    }
}
