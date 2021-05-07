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

        public MainWindow()
        {
            Ports = new ObservableCollection<Port>();
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
