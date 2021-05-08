using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
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

        private void UpdatePortsList()
        {
            OnPropertyChanged("Ports");
        }

        private DeviceNotifier notifier = new DeviceNotifier();
        private void InitializeNotifications()
        {
            notifier.StartInsertUSBHandler();
            notifier._NotifyUsbAdded += Notifier__NotifyUsbAdded;

            notifier.StartRemoveUSBHandler();
            notifier._NotifyUsbRemoved += Notifier__NotifyUsbRemoved;
        }

        private void Notifier__NotifyUsbRemoved(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Notifier__NotifyUsbAdded(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public MainWindow()
        {
            Ports = new ObservableCollection<Port>();

            InitializeNotifications();

            UpdatePortsList();

            this.DataContext = this;
            InitializeComponent();

            this.Hide();
            notifyIcon.Icon = new Icon(Application.GetResourceStream(new Uri("/Images/monitor.ico", UriKind.Relative)).Stream);

            Ports.Add("COM1");
            UpdatePortsList();

            Ports.UpdateOpen("COM1", true);

            //Ports.Remove("COM1");
            //UpdatePortsList();




        }
    }


}
