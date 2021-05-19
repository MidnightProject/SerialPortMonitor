using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using SerialPortMonitor.Model;


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

            if (balloonTips.Count ==  1)
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
        }
    }
}
