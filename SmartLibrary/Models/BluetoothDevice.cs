namespace SmartLibrary.Models
{
    public record BluetoothDevice
    {
        public string Name { get; init; }
        public string Address { get; init; }
        public string ClassOfDevice { get; init; }
        public bool IsAuthenticated { get; init; }

        public BluetoothDevice(string name, string address, string classOfDevice, bool isAuthenticated)
        {
            Name = name;
            Address = address;
            ClassOfDevice = classOfDevice;
            IsAuthenticated = isAuthenticated;
        }
    }
}