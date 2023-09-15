using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibraryHelper
{
    /// <summary>
    /// [DllImport("kernel32.dll")]: Đây là một thuộc tính (attribute) trong C# để chỉ ra rằng chúng ta đang import một hàm từ thư viện kernel32.dll.
    /// Chú ý: 'extern' chỉ ra rằng hàm này không được triển khai trong mã C# mà thay vào đó nó sẽ được tìm và sử dụng từ một thư viện bên ngoài (DLL).
    /// Chú ý: [Out] khác hoàn toàn out trong c#.
    /// IntPtr là kiểu dữ liệu được sử dụng để đại diện cho con trỏ trong C#
    /// </summary>
    public class MemoryHelper
    {

        #region PRIVATE USE WIN API
        // Định nghĩa hằng số cho quyền truy cập vào quy trình
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFFu;//0x1F0FFF
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int VK_RETURN = 0x0D;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwsize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwsize, int dwFreeType);
        /// <summary>
        /// Import hàm OpenProcess từ thư viện kernel32.dll
        /// </summary>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="bInheritHandle"></param>
        /// <param name="dwProcessId"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        /// <summary>
        /// Hàm ReadProcessMemory sử dụng để đọc thông tin từ bộ nhớ memory. Là một hàm trong kernel32.dll WIN API
        /// </summary>
        /// <param name="hProcess">Con trỏ tới tiến trình mà chúng ta muốn đọc bộ nhớ từ.</param>
        /// <param name="lpBaseAddress">Con trỏ tới địa chỉ bắt đầu của vùng bộ nhớ mà bạn muốn đọc.</param>
        /// <param name="lpBuffer">Mảng byte được sử dụng để lưu trữ dữ liệu được đọc từ bộ nhớ của tiến trình mục tiêu.</param>
        /// <param name="dwSize">Xác định kích thước của vùng bộ nhớ bạn muốn đọc (số byte).</param>
        /// <param name="lpNumberOfBytesRead">Được sử dụng để lưu trữ số byte thực sự đã được đọc từ bộ nhớ của tiến trình mục tiêu sau khi hàm thực hiện xong.</param>
        /// <returns>Trả về true nếu nó thực hiện thành công và false nếu không.</returns>

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out]byte[] lpBuffer, int dwSize, [Out] IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, [Out] IntPtr lpNumberOfBytesWritten);
        
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Public
        public static void SendEnter(IntPtr hProcess)
        {
            SetActiveWindow(hProcess);
            // Gửi sự kiện KeyDown và KeyUp cho phím Enter
            SendMessage(hProcess, WM_KEYDOWN, (IntPtr)VK_RETURN, IntPtr.Zero);
            SendMessage(hProcess, WM_KEYUP, (IntPtr)VK_RETURN, IntPtr.Zero);
        }

        public static void PossEnter(IntPtr hProcess)
        {
            PostMessage(hProcess, WM_KEYDOWN, (IntPtr)VK_RETURN, IntPtr.Zero);
            PostMessage(hProcess, WM_KEYUP, (IntPtr)VK_RETURN, IntPtr.Zero);
        }

        public static IntPtr GetHandleProcess(int id)
        {
            var handle = OpenProcess(PROCESS_ALL_ACCESS, false, id);
            return handle;
        }

        public static IntPtr Read_IntPtr(IntPtr hProcess, IntPtr lpBaseAddress)
        {
            byte[] array = new byte[1024];
            if(ReadProcessMemory(hProcess, lpBaseAddress, array, array.Length, IntPtr.Zero))
            {
                return (IntPtr)(long)BitConverter.ToUInt32(array, 0);
            }
            else
            {
                return IntPtr.Zero;
            }
            
        }
        /// <summary>
        /// Đọc giá trị từ memory trả ra kiểu int kích thước 4 byte
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="lpBaseAddress"></param>
        /// <returns></returns>
        public static int? Read_4Byte(IntPtr hProcess, IntPtr lpBaseAddress)
        {
            byte[] array = new byte[4];
            if (ReadProcessMemory(hProcess, lpBaseAddress, array, array.Length, IntPtr.Zero))
            {
                return BitConverter.ToInt32(array, 0);
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// Đọc giá trị từ memory trả ra kiểu Float
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="lpBaseAddress"></param>
        /// <returns></returns>
        public static float? Read_Float(IntPtr hProcess, IntPtr lpBaseAddress)
        {
            byte[] array = new byte[24];
            if (ReadProcessMemory(hProcess, lpBaseAddress, array, array.Length, IntPtr.Zero))
            {
                return BitConverter.ToSingle(array, 0);
            }
            else
            {
                return null;
            }
        }
        public static IntPtr Read_Offset(IntPtr hProcess, IntPtr baseAddress, int[] pointer)
        {
            IntPtr offset = IntPtr.Zero;
            foreach (int item in pointer)
            {
                if (offset == IntPtr.Zero)
                {
                    offset = IntPtr.Add(Read_IntPtr(hProcess, baseAddress), item);
                }
                else
                {
                    offset = IntPtr.Add(Read_IntPtr(hProcess,  offset),item);
                }
            }
            return offset;
        }
        public static bool Write_Float(IntPtr hProcess, IntPtr lpBaseAddress, float value)
        {
            
            float? current = MemoryHelper.Read_Float(hProcess, lpBaseAddress);
            if (current is float fl) value += fl;
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteProcessMemory(hProcess, lpBaseAddress, bytes, bytes.Length, IntPtr.Zero);
        }
        public static bool Write_Uint(IntPtr hProcess, IntPtr lpBaseAddress, uint value)
        {

            //float? current = MemoryHelper.Read_Float(hProcess, lpBaseAddress);
            //if (current is float fl) value += fl;
            byte[] bytes = BitConverter.GetBytes(value);
            return WriteProcessMemory(hProcess, lpBaseAddress, bytes, bytes.Length, IntPtr.Zero);
        }

        public static bool Write_Byte(IntPtr hProcess, IntPtr lpBaseAddress, byte[] value)
        {
            return WriteProcessMemory(hProcess, lpBaseAddress, value, value.Length, IntPtr.Zero);
        }
        public static bool Call_Function(IntPtr hProcess, byte[] byteASM)
        {
            //Khởi tạo bộ nhớ chưa được cấp phát và gán mã máy vào đó
            IntPtr alloc = VirtualAllocEx(hProcess, IntPtr.Zero, byteASM.Length, 0x3000, 0x40);
            //Ghi địa chỉ hàm cần gọi vào bộ nhớ cấp phát
            WriteProcessMemory(hProcess, alloc, byteASM, byteASM.Length, IntPtr.Zero);
            //Khởi tạo một remote thread để gọi hàm
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, alloc, IntPtr.Zero, 0, IntPtr.Zero);
            //Khởi động remoter thread
            int threadResult = WaitForSingleObject(hThread, 5000);
            //Giải phóng bộ nhớ cấp 
            VirtualFreeEx(hProcess, alloc, 0, 0x8000);
            return threadResult == 0; 
        }

        public static IntPtr AllocMemory_Write(IntPtr hProcess, uint value)
        {
            // Chuyển đổi uint thành mảng byte
            byte[] byteArray = BitConverter.GetBytes(value);
            //Khởi tạo bộ nhớ chưa được cấp phát và gán mã máy vào đó
            IntPtr alloc = VirtualAllocEx(hProcess, IntPtr.Zero, byteArray.Length, 0x3000, 0x40);
            //Ghi địa chỉ hàm cần gọi vào bộ nhớ cấp phát
            WriteProcessMemory(hProcess, alloc, byteArray, byteArray.Length, IntPtr.Zero);
            return alloc;
        }

        public static void VirtualFree(IntPtr hProcess, IntPtr alloc) 
        {
            VirtualFreeEx(hProcess, alloc, 0, 0x8000);
        }
        #endregion

    }
}
