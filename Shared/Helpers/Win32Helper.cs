using System.Runtime.InteropServices;
using System.Text;

namespace Shared.Helpers
{
    public partial class Win32Helper
    {
        // 声明常量
        public const int WM_COPYDATA = 0x004A;

        // 定义 COPYDATASTRUCT 结构
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        /// <summary>
        /// 发送字符串消息
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="message"></param>
        public static void SendMessageString(IntPtr hWnd, string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            byte[] messageBytes = Encoding.Unicode.GetBytes(message + '\0'); // 添加终止符

            COPYDATASTRUCT cds = new()
            {
                dwData = IntPtr.Zero,
                cbData = messageBytes.Length
            };
            cds.lpData = Marshal.AllocHGlobal(cds.cbData);
            Marshal.Copy(messageBytes, 0, cds.lpData, cds.cbData);
            try
            {
                SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            finally
            {
                //释放分配的内存，即使发生异常也不会泄漏资源
                Marshal.FreeHGlobal(cds.lpData);
            }
        }
    }
}
