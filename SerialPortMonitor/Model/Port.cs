using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SerialPortMonitor.Model
{
    public class Port : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description { get; set; }

        private Boolean open { get; set; }
        public Boolean Open
        {
            get { return open; }
            set
            {
                if (value != open)
                {
                    open = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Device { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
