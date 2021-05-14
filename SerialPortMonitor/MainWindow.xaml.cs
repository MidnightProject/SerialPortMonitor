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


        public MainWindow()
        {
            Ports = new ObservableCollection<Port>();

            InitializeNotifications();

            UpdateView();

            this.DataContext = this;
            InitializeComponent();

            this.Hide();
            notifyIcon.Icon = new Icon(Application.GetResourceStream(new Uri("/Images/monitor.ico", UriKind.Relative)).Stream);

            //Ports.Add("COM1");

            //Ports.UpdateOpen("COM1", true);

            //Ports.Remove("COM1");

            UpdatePorts();


            timerUsbRemoved = new DispatcherTimer();
            timerUsbRemoved.Tick += timerUsbRemoved_Tick;
            timerUsbRemoved.Interval = TimeSpan.FromSeconds(5);

            timerUsbAdded = new DispatcherTimer();
            timerUsbAdded.Tick += timerUsbAdded_Tick;
            timerUsbAdded.Interval = TimeSpan.FromSeconds(5);
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

                        systemSerialPorts.Add(new Port() { Name = name, Description = description });

                        /*
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Hardware\DeviceMap\SerialComm");
                        foreach (string value in key.GetValueNames())
                        {
                            if (key.GetValue(value).ToString() == name)
                            {
                                var v = value;
                            }
                        }
                        */
                    }
                }
            }

            return systemSerialPorts;
        }

        private void RemovePorts(List<Port> systemSerialPorts)
        {
            if (systemSerialPorts != null && Ports != null)
            {
                foreach (Port portToRemove in Ports)
                {
                    var name = systemSerialPorts.FirstOrDefault(port => port.Name == portToRemove.Name);
                    if (name == null)
                    {
                        //Ports.RemovePort(portToRemove.Name);
                    }
                }

                for (int i = 0; i < Ports.Count; i++)
                {
                    var name = systemSerialPorts.FirstOrDefault(port => port.Name == Ports[i].Name);
                    if (name == null)
                    {
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
                        Ports.Add(portToAdd.Name, portToAdd.Description);
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
