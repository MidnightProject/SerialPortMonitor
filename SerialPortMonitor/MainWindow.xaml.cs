using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using SerialPortMonitor.Model;
using SerialPortMonitor.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SerialPortMonitor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<Port> Ports { get; set; }

        private void UpdateView()
        {
            OnPropertyChanged("Ports");
        }

        private DeviceNotifier notifier = new DeviceNotifier();
        private void InitializeNotifications()
        {
            notifier.StartInsertUSBHandler();
            notifier._NotifyUsbAdded += Notifier_NotifyUsbAdded;

            notifier.StartRemoveUSBHandler();
            notifier._NotifyUsbRemoved += Notifier_NotifyUsbRemoved;
        }

        private void Notifier_NotifyUsbRemoved(object sender, EventArgs e)
        {
            timerUsbRemoved.Start();
        }

        private void Notifier_NotifyUsbAdded(object sender, EventArgs e)
        {
            timerUsbAdded.Start();
        }

        private DispatcherTimer timerUsbRemoved;
        private void timerUsbRemoved_Tick(object sender, EventArgs e)
        {
            timerUsbRemoved.Stop();

            RemovePorts(GetSystemSerialPorts());
        }

        private DispatcherTimer timerUsbAdded;
        private void timerUsbAdded_Tick(object sender, EventArgs e)
        {
            timerUsbAdded.Stop();

            AddPorts(GetSystemSerialPorts());
        }

        private DispatcherTimer timerUpdateOpen;
        private void timerUpdateOpen_Tick(object sender, EventArgs e)
        {
            UpdateOpenAsync();
        }

        private DispatcherTimer timerUpdateApplication;
        private void timerUpdateApplication_Tick(object sender, EventArgs e)
        {
            timerUpdateApplication.Stop();
            UpdateApplicationAsync();
        }

        private List<BalloonTip> balloonTips;

        private DispatcherTimer timerBalloonTips;
        private void timerBalloonTips_Tick(object sender, EventArgs e)
        {
            timerBalloonTips.Stop();

            switch (balloonTips.Count)
            {
                case 0:
                    return;
                case 1:
                    balloonTips.RemoveAt(0);
                    return;
                default:
                    balloonTips.RemoveAt(0);
                    notifyIcon.ShowBalloonTip(balloonTips[0].Title(), balloonTips[0].Message(), BalloonIcon.Info);
                    timerBalloonTips.Start();
                    return;
            }
        }

        private void ShowBallonTips(BalloonTip balloonTip)
        {
            balloonTips.Add(balloonTip);

            if (balloonTips.Count == 1)
            {
                notifyIcon.ShowBalloonTip(balloonTips[0].Title(), balloonTips[0].Message(), BalloonIcon.Info);
                timerBalloonTips.Start();
            }
        }


        public MainWindow()
        {
            Ports = new ObservableCollection<Port>();
            balloonTips = new List<BalloonTip>();

            InitializeNotifications();

            UpdateView();

            this.DataContext = this;
            InitializeComponent();

            this.Hide();
            notifyIcon.Icon = new Icon(Application.GetResourceStream(new Uri("/Images/monitor.ico", UriKind.Relative)).Stream);

            timerUsbRemoved = new DispatcherTimer();
            timerUsbRemoved.Tick += timerUsbRemoved_Tick;
            timerUsbRemoved.Interval = TimeSpan.FromSeconds(5);

            timerUsbAdded = new DispatcherTimer();
            timerUsbAdded.Tick += timerUsbAdded_Tick;
            timerUsbAdded.Interval = TimeSpan.FromSeconds(5);

            timerBalloonTips = new DispatcherTimer();
            timerBalloonTips.Tick += timerBalloonTips_Tick;
            timerBalloonTips.Interval = TimeSpan.FromSeconds(5);

            timerUpdateOpen = new DispatcherTimer();
            timerUpdateOpen.Tick += timerUpdateOpen_Tick;
            timerUpdateOpen.Interval = TimeSpan.FromSeconds(5);

            UpdatePorts();
        }


        private List<Port> GetSystemSerialPorts()
        {
            List<Port> systemSerialPorts = new List<Port>();

            ManagementClass processClass = new ManagementClass("Win32_PnPEntity");
            ManagementObjectCollection portsWin32 = processClass.GetInstances();

            foreach (string name in System.IO.Ports.SerialPort.GetPortNames())
            {
                foreach (ManagementObject property in portsWin32)
                {
                    if (property.GetPropertyValue("Name") != null
                        && property.GetPropertyValue("Name").ToString().Contains(name))
                    {
                        string description = property.GetPropertyValue("Description").ToString();
                        string device = String.Empty;

                        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Hardware\DeviceMap\SerialComm");
                        foreach (string value in key.GetValueNames())
                        {
                            if (key.GetValue(value).ToString() == name)
                            {
                                device = value.ToString();
                            }
                        }

                        systemSerialPorts.Add(new Port() { Name = name, Description = description, Device = device });
                    }
                }
            }

            return systemSerialPorts;
        }

        private void RemovePorts(List<Port> systemSerialPorts)
        {
            if (systemSerialPorts != null && Ports != null)
            {
                for (int i = 0; i < Ports.Count; i++)
                {
                    var name = systemSerialPorts.FirstOrDefault(port => port.Name == Ports[i].Name);
                    if (name == null)
                    {
                        ShowBallonTips(new BalloonTip() { Name = Ports[i].Name, Description = Ports[i].Description, PortStatus = Status.Removed });
                        Ports.Remove(Ports[i].Name);
                    }
                }
            }

            UpdateView();
        }

        private void AddPorts(List<Port> systemSerialPorts)
        {
            if (systemSerialPorts != null)
            {
                foreach (Port portToAdd in systemSerialPorts)
                {
                    var name = Ports.FirstOrDefault(port => port.Name == portToAdd.Name);
                    if (name == null)
                    {
                        ShowBallonTips(new BalloonTip() { Name = portToAdd.Name, Description = portToAdd.Description, PortStatus = Status.Added });
                        Ports.Add(portToAdd.Name, portToAdd.Description, portToAdd.Device);
                    }
                }
            }

            UpdateView();
        }

        private void UpdatePorts()
        {
            Ports.Clear();

            AddPorts(GetSystemSerialPorts());
            SelectedIndexFromPortsList = -1;

            UpdateOpenAsync();
        }

        private async Task UpdateOpenAsync()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\mode.com",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            await proc.WaitForExitAsync();

            var output = proc.StandardOutput.ReadToEnd();

            foreach (Port port in Ports)
            {
                if (output.Contains(port.Name + ':'))
                {
                    int index = Ports.IndexOf(Ports.Where(n => n.Name == port.Name).FirstOrDefault());
                    if (index != -1 && index == SelectedIndexFromPortsList)
                    {
                        SelectedIndexFromPortsList = -1;
                    }

                    port.Open = false;
                }
                else
                {
                    port.Open = true;
                }
            }
        }

        private void TrayPopup_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateOpenAsync();
            timerUpdateOpen.Start();
        }

        private void TrayPopup_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            timerUpdateOpen.Stop();
        }

        private int OldSelectedIndexFromPortsList;
        private int selectedIndexFromPortsList;
        public int SelectedIndexFromPortsList
        {
            get
            {
                return selectedIndexFromPortsList;
            }

            set
            {
                selectedIndexFromPortsList = value;
                OnPropertyChanged("SelectedIndexFromPortsList");
            }
        }

        private void TrayPopup_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectedIndexFromPortsList = -1;
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedIndexFromPortsList != -1)
            {
                if (OldSelectedIndexFromPortsList == SelectedIndexFromPortsList)
                {
                    SelectedIndexFromPortsList = -1;
                }
                else
                {
                    if ( String.IsNullOrEmpty(Ports[SelectedIndexFromPortsList].ApplicationName) )
                    {
                        UpdateApplicationAsync();
                    }
                }

                OldSelectedIndexFromPortsList = SelectedIndexFromPortsList;
            }
        }

        private async Task UpdateApplicationAsync()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, @"\handle.exe"),
                    Arguments = "-a " + Ports[SelectedIndexFromPortsList].Device,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            await proc.WaitForExitAsync();

            var output = proc.StandardOutput.ReadToEnd();

            string patternName = @".+(?=pid:)";
            Regex rgName = new Regex(patternName);
            MatchCollection matchedName = rgName.Matches(output.Replace(" ", ""));

            if (matchedName.Count != 0)
            {
                Ports[SelectedIndexFromPortsList].ApplicationName = matchedName[0].Value;

                string patternPID = @"(?<=pid: )(\d+)";
                Regex rgPID = new Regex(patternPID);
                MatchCollection matchedPID = rgPID.Matches(output);

                if (matchedPID.Count != 0)
                {
                    Ports[SelectedIndexFromPortsList].PID = matchedPID[0].Value;

                    proc = Process.GetProcessById( int.Parse(matchedPID[0].Value));
                    string path = proc.GetPathToApp();

                    Ports[SelectedIndexFromPortsList].ApplicationPath = path;

                    Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                    using (Bitmap bmp = icon.ToBitmap())
                    {
                        var stream = new MemoryStream();
                        bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        Ports[SelectedIndexFromPortsList].ApplicationIcon = BitmapFrame.Create(stream);
                    }
                }
                else
                {
                    Ports[SelectedIndexFromPortsList].PID = String.Empty;
                    Ports[SelectedIndexFromPortsList].ApplicationPath = String.Empty;
                }
            }
            else
            {
                Ports[SelectedIndexFromPortsList].ApplicationName = "File Not Found";
                Ports[SelectedIndexFromPortsList].PID = String.Empty;
                Ports[SelectedIndexFromPortsList].ApplicationPath = String.Empty;
            } 
        }
    } 
}
