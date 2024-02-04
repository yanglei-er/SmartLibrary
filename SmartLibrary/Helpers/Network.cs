using System.Runtime.InteropServices;

namespace SmartLibrary.Helpers
{
    public sealed partial class Network
    {
        [LibraryImport("sensapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsNetworkAlive(out int connectionDescription);

        public static bool IsInternetConnected()
        {
            return IsNetworkAlive(out _);
        }
    }
}
