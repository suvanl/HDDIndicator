using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;

namespace HDDIndicator
{
    public partial class HDDIndicator : Form
    {
        // Global variables

        NotifyIcon hddIndicatorIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddInfoWorkerThread;

        // Main winform and icon stuff (entry point)

        public HDDIndicator()
        {
            InitializeComponent();
            activeIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");
            hddIndicatorIcon = new NotifyIcon
            {
                Icon = idleIcon,
                Visible = true
            };

            MenuItem progNameMenuItem = new MenuItem("HDD Usage Indicator v1.0.0 Beta");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddIndicatorIcon.ContextMenu = contextMenu;

            quitMenuItem.Click += QuitMenuItem_Click;

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Start worker thread, which pulls HDD activity
            hddInfoWorkerThread = new Thread(new ThreadStart(HddActivityThread));
            hddInfoWorkerThread.Start();
        }

        // Context menu event handlers

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            hddInfoWorkerThread.Abort();
            hddIndicatorIcon.Dispose();
            this.Close();
        }

        // Hard drive activity threads

        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");

            try
            {
                // Main loop
                while (true)
                {
                    // Connect to the drive performance instance
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    foreach( ManagementObject obj in driveDataClassCollection )
                    {
                        // Only process the "_Total" instance, ignoring all individual instances
                        if( obj["Name"].ToString() == "_Total")
                        {
                            if ( Convert.ToUInt64(obj["DiskBytesPersec"]) > 0 )
                            {
                                // Show busy icon
                                hddIndicatorIcon.Icon = activeIcon;
                            }
                            else
                            {
                                // Show idle icon
                                hddIndicatorIcon.Icon = idleIcon;
                            }
                        }
                    }
                    
                    // Sleep for 0.1s
                    Thread.Sleep(100);
                }
            } catch (ThreadAbortException)
            {
                // Abort the thread
                driveDataClass.Dispose();
            }
        }
    }
}
