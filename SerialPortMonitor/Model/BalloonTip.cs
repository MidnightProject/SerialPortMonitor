namespace SerialPortMonitor.Model
{
    public enum Status
    {
        Added,
        Removed,
    }

    public class BalloonTip
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Status PortStatus { get; set; }

        public string Title()
        {
            switch (PortStatus)
            {
                case Status.Added:
                    return Name + " Added"; 
                case Status.Removed:
                    return Name + " Removed";
                default:
                    return "Error";
            }
        }

        public string Message()
        {
            return Name + " - " + Description;
        }
    }
}
