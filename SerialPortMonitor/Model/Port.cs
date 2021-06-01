using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace SerialPortMonitor.Model
{
    public class Port : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string name;
        public string Name
        {
            get
            {
                return name;
            }

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
            get
            {
                return open;
            }

            set
            {
                if (value != open)
                {
                    open = value;
                    OnPropertyChanged();

                    ApplicationName = String.Empty;
                }
            }
        }

        public string Device { get; set; }

        private string applicationName;
        public string ApplicationName
        {
            get
            {
                return applicationName;
            }

            set
            {
                if (value != applicationName)
                {
                    applicationName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string applicationPath;
        public string ApplicationPath
        {
            get
            {
                return applicationPath;
            }

            set
            {
                if (value != applicationPath)
                {
                    applicationPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource applicationIcon;
        public ImageSource ApplicationIcon
        {
            get
            {
                return applicationIcon;
            }

            set
            {
                if (value != applicationIcon)
                {
                    applicationIcon = value;
                    OnPropertyChanged();
                }
            }
        }

        private string pid;
        public string PID
        {
            get
            {
                return pid;
            }

            set
            {
                if (value != pid)
                {
                    pid = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
