using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MonitorBrightness
{
    class BrightnessControl
    {

        public static List<IntPtr> monitors = new List<IntPtr>();

        public BrightnessControl()
        {
            SetupMonitors();
        }

        public static uint monCount = 0;

        public void SetupMonitors()
        {
            monitors.Clear();
#if DEBUG
            if (NativeCalls.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callb, 0))

                Console.WriteLine("You have {0} monitors", monCount);
            else
                Console.WriteLine("An error occured while enumerating monitors");
#else
            NativeCalls.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callb, 0);
#endif
        }
        private static bool Callb(IntPtr hMonitor, IntPtr hDC, ref NativeStructures.Rect prect, int d)
        {
            //monitors.Add(hMonitor);
            int lastWin32Error;
            uint pdwNumberOfPhysicalMonitors = 0;
            bool numberOfPhysicalMonitorsFromHmonitor = NativeCalls.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref pdwNumberOfPhysicalMonitors);
            lastWin32Error = Marshal.GetLastWin32Error();

            var pPhysicalMonitorArray = new NativeStructures.PHYSICAL_MONITOR[pdwNumberOfPhysicalMonitors];
            bool physicalMonitorsFromHmonitor = NativeCalls.GetPhysicalMonitorsFromHMONITOR(hMonitor, pdwNumberOfPhysicalMonitors, pPhysicalMonitorArray);
            lastWin32Error = Marshal.GetLastWin32Error();

            monitors.Add(pPhysicalMonitorArray[0].hPhysicalMonitor);

            //Console.WriteLine($"Handle: 0x{hMonitor:X}, Num: {pdwNumberOfPhysicalMonitors}, Physical: {pPhysicalMonitorArray[0].hPhysicalMonitor}");

            //GetMonitorCapabilities((int)monCount);
            return ++monCount > 0;
        }

        //private static void GetMonitorCapabilities(int monitorNumber)
        //{
        //    uint pdwMonitorCapabilities = 0u;
        //    uint pdwSupportedColorTemperatures = 0u;
        //    var monitorCapabilities = NativeCalls.GetMonitorCapabilities(monitors[monitorNumber], ref pdwMonitorCapabilities, ref pdwSupportedColorTemperatures);
        //    Debug.WriteLine(pdwMonitorCapabilities);
        //    Debug.WriteLine(pdwSupportedColorTemperatures);
        //    int lastWin32Error = Marshal.GetLastWin32Error();
        //    NativeStructures.MC_DISPLAY_TECHNOLOGY_TYPE type = NativeStructures.MC_DISPLAY_TECHNOLOGY_TYPE.MC_SHADOW_MASK_CATHODE_RAY_TUBE;
        //    var monitorTechnologyType = NativeCalls.GetMonitorTechnologyType(monitors[monitorNumber], ref type);
        //    Debug.WriteLine(type);
        //    lastWin32Error = Marshal.GetLastWin32Error();
        //}

        public bool SetBrightness(short brightness, int monitorNumber)
        {
            var brightnessWasSet = NativeCalls.SetMonitorBrightness(monitors[monitorNumber], brightness);
            //if (brightnessWasSet)
            //    Debug.WriteLine("Brightness set to " + (short)brightness);
            int lastWin32Error = Marshal.GetLastWin32Error();
            return brightnessWasSet;
        }

        public BrightnessInfo GetBrightnessCapabilities(int monitorNumber)
        {
            short current = -1, minimum = -1, maximum = -1;
            bool getBrightness = NativeCalls.GetMonitorBrightness(monitors[monitorNumber], ref minimum,ref current,ref maximum);
            int lastWin32Error = Marshal.GetLastWin32Error();
            return new BrightnessInfo { minimum = minimum, maximum = maximum, current = current};
        }

        //public void DestroyMonitors(uint pdwNumberOfPhysicalMonitors, NativeStructures.PHYSICAL_MONITOR[] pPhysicalMonitorArray)
        //{
        //    var destroyPhysicalMonitors = NativeCalls.DestroyPhysicalMonitors(pdwNumberOfPhysicalMonitors, pPhysicalMonitorArray);
        //    int lastWin32Error = Marshal.GetLastWin32Error();
        //}

        public uint GetMonitors()
        {
            return monCount;
        }
    }
}
