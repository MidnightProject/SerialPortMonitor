using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SerialPortMonitor.Model
{
    public static class PortExtensions
    {
        public static ObservableCollection<Port> Add(this ObservableCollection<Port> list, string name)
        {
            list.Add(new Port() { Name = name, Description = name });
            return list;
        }

        public static ObservableCollection<Port> Add(this ObservableCollection<Port> list, string name, string description, string device)
        {
            list.Add(new Port() { Name = name, Description = description, Device = device});
            return list;
        }

        public static ObservableCollection<Port> Add(this ObservableCollection<Port> list, string name, string description, string device, Boolean open)
        {
            list.Add(new Port() { Name = name, Description = description, Device = device, Open = open });
            return list;
        }

        public static ObservableCollection<Port> Remove(this ObservableCollection<Port> list, string name)
        {
            var portToRemove = list.SingleOrDefault(port => port.Name == name);
            if (portToRemove != null)
            {
                list.Remove(portToRemove);
            }

            return list;
        }

        public static ObservableCollection<Port> UpdateOpen(this ObservableCollection<Port> list, string name, Boolean open)
        {
            var portToUpdate = list.FirstOrDefault(port => port.Name == name);
            if (portToUpdate != null)
            {
                portToUpdate.Open = open;
            }
            return list;
        }
    }
}
