using LibraryHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Volam2
{
    public class INFO_VL2
    {
        public static void Call_StartGame(IntPtr hProcess)
        {
            //Phải mov eax, đại chỉ trước khi Call eax là để xác định địa chỉ đích một cách rõ ràng cho lệnh call. Gọi ngay có thể khong chạy được
            byte[] byteASM = new byte[] { 0xB8, 0xC0, 0x4E, 0x66,0x00,//mov eax, 0x00664EC0
                                         0xFF,0xD0, //call eax
                                         0xB8, 0xD0, 0xE3, 0x66,0x00,//mov eax, 0x0066E3D0
                                         0xFF,0xD0, //call eax
                                         0xC3 }; //res
            MemoryHelper.Call_Function(hProcess, byteASM);
        }

        public static void Call_SelectServer(IntPtr hProcess, CMC_Info cmc, ServerInfo server)
        {
            byte[] CodeASM = new byte[]{
                                                0xB9, 0x58, 0xBD, 0xD3, 0x00, // mov ecx,00D3BD58 //Name server cần thay thế
                                                0xBF, 0x04, 0x00, 0x00, 0x00, // MOV EDI, 4 cần thay thế
                                                0xBA, 0x01, 0x00, 0x00, 0x00, // MOV EDX, 1 cần thay thế
                                                0x51,                         // PUSH ECX
                                                0x57,                         // PUSH EDI
                                                0x52,                         // PUSH EDX                                            
                                                0xB8, 0x00, 0xD1, 0x66, 0x00, // mov eax, 0066D100
                                                0xFF,0xD0, //call eax
                                                0xB8, 0xD0, 0xF9, 0x66, 0x00, // mov eax, 0066F9D0
                                                0xFF,0xD0, //call eax
                                                0xB8, 0x10, 0xD0, 0x66, 0x00, // mov eax, 0066D010
                                                0xFF,0xD0, //call eax
                                                0xC3                          // RET
                                            };

            // Sử dụng mã hóa Windows-1258 để chuyển đổi chuỗi TCVN3 thành mảng byte
            string tcvn3_CMC = FontsHelper.UnicodeToTCVN3(cmc.CMC_NAME); // Chuỗi TCVN3 bạn muốn chuyển đổi
            string tcvn3_Server = FontsHelper.UnicodeToTCVN3(server.ServerName); // Chuỗi TCVN3 bạn muốn chuyển đổi

            // Chuyển chuỗi tcvn3 về byte 
            Encoding tcvn3Encoding = Encoding.GetEncoding("windows-1258");
            byte[] byte_CMC = tcvn3Encoding.GetBytes(tcvn3_CMC);
            byte[] byte_Server = tcvn3Encoding.GetBytes(tcvn3_Server);

            //Ghi giá trị cụm máy chủ vào "so2game.exe"+00941794
            IntPtr hCmc = MemoryHelper.Read_Offset(hProcess, (IntPtr)0x00941794, new int[] { 0x890 });
            MemoryHelper.Write_Byte(hProcess, hCmc, byte_CMC);

            //Ghi giá trị server vào 0x00D3BD58
            IntPtr hServer = (IntPtr)0x00D3BD58;
            MemoryHelper.Write_Byte(hProcess, hServer, byte_Server);
            //Ghi vào code asm
            CodeASM[6] = server.ServerIndex;
            CodeASM[11] = server.CmcIndex;

            MemoryHelper.Call_Function(hProcess, CodeASM);

            //Giải phóng bộ nhớ cấp 
            //MemoryHelper.VirtualFree(hProcess, hAlloc);
        }
        public static IntPtr AbsAddress(Process process, int AddressPoint)
        {
            IntPtr AbsoluteAddress = IntPtr.Add(process.MainModule.BaseAddress, AddressPoint);
            return AbsoluteAddress;
        }
        public static void LoginGame(Process process,IntPtr hProcess, string username, string password)
        {
            var x = AbsAddress(process,0x942E14);
            IntPtr ptrUser = MemoryHelper.Read_Offset(hProcess, AbsAddress(process, 0x0095370C), new int[] { 0x2E8 });
            byte[] byteUser = Encoding.ASCII.GetBytes(username);

            //Call TabUser
            IntPtr ptrTabUser = MemoryHelper.Read_Offset(hProcess, AbsAddress(process, 0x9536F8), new int[] { 0X68, 0x1CC });
            byte[] hTabUser = BitConverter.GetBytes((uint)ptrTabUser);
            var byteTabUser = new byte[] { 0x68, hTabUser[0], hTabUser[1], hTabUser[2], hTabUser[3],  // push hTabUser
                                          0xB8, 0xF0, 0x20, 0x64, 0x00,                               // mov eax, 006420F0
                                          0xFF,0xD0,                                                  // call eax
                                          0x83, 0xC4, 0x04,                                           // add esp, 04
                                          0xC3                                                        // RET
                                        };
            MemoryHelper.Call_Function(hProcess, byteTabUser);
            MemoryHelper.PossKey(process, username,10);

            Thread.Sleep(200);

            //Call TabPass
            IntPtr ptrTabPass = MemoryHelper.Read_Offset(hProcess, AbsAddress(process, 0x9536F8), new int[] { 0X68, 0x308 });
            byte[] hTabPass = BitConverter.GetBytes((uint)ptrTabPass);
            var byteTabPass = new byte[] { 0x68, hTabPass[0], hTabPass[1], hTabPass[2], hTabPass[3],  // push hTabPass
                                          0xB8, 0xF0, 0x20, 0x64, 0x00,                               // mov eax, 006420F0
                                          0xFF,0xD0,                                                  // call eax
                                          0x83, 0xC4, 0x04,                                           // add esp, 04
                                          0xC3                                                        // RET
                                        };
            MemoryHelper.Call_Function(hProcess, byteTabPass);        
            MemoryHelper.PossKey(process, password,10);//Write Pass

            //Send Enter
            MemoryHelper.PossEnter(process);

            //Đồng ý với điều khoản sử dụng
            //IntPtr hAccept = MemoryHelper.Read_Offset(hProcess, AbsAddress(process, 0x00944AD4), new int[] { 0x204,0x638});

            Thread.Sleep(5000);
            //An phan dong y voi dieu khoan su dung
            IntPtr hAccept = AbsAddress(process, 0x0094F424);
            MemoryHelper.Write_Uint(hProcess,hAccept, 0);
        }

        public static void SelectFigure(IntPtr hProcess,int idFigure = 0)
        {
            byte index = 0x0;
            switch (idFigure)
            {
                case 0:
                    index = 0x00;
                    break;
                case 1:
                    index = 0x01;
                    break;
                case 2:
                    index = 0x02;
                    break;
                default:
                    index = 0x0;
                    break;
            }

            byte[] hSelectFigure = new byte[]
            {
                0x68,index, 0x00,0x00,0x00,
                0xB9, 0x88, 0xBA, 0xD3, 0x00, //mov ecx,00D3BA88
                0xB8, 0x40, 0x22, 0x63, 0x00, //move eax,00632240
                0xFF,0xD0,                      //Call eax
                0xC3
            };
            MemoryHelper.Call_Function(hProcess, hSelectFigure);
        }

        public static List<ServerInfo> ListServer(CumMayChu cmc)
        {
            var myList = new List<ServerInfo>();
            switch (cmc)
            {
                case CumMayChu.HCM:
                    myList.Add(new ServerInfo() { ServerName = "Bạch Hổ", ServerCode = 1751365954, ServerIndex = 0x0, CmcIndex = 0x0 });
                    myList.Add(new ServerInfo() { ServerName = "Phục Hổ", ServerCode = 1676961872, ServerIndex = 0x1, CmcIndex = 0x0 });
                    myList.Add(new ServerInfo() { ServerName = "Quán Hổ", ServerCode = 1857582417, ServerIndex = 0x2, CmcIndex = 0x0 });
                    myList.Add(new ServerInfo() { ServerName = "Lăng Hổ", ServerCode = 1735305292, ServerIndex = 0x3, CmcIndex = 0x0 });
                    break;
                case CumMayChu.HN:
                    myList.Add(new ServerInfo() { ServerName = "Tàng Long", ServerCode = 1735308628, ServerIndex = 0, CmcIndex = 0x1 });
                    myList.Add(new ServerInfo() { ServerName = "Thiên Long", ServerCode = 2859034708, ServerIndex = 1, CmcIndex = 0x1 });
                    myList.Add(new ServerInfo() { ServerName = "Linh Bảo Sơn", ServerCode = 1752066380, ServerIndex = 2, CmcIndex = 0x1 });
                    myList.Add(new ServerInfo() { ServerName = "Châu Long", ServerCode = 1974036547, ServerIndex = 3, CmcIndex = 0x1 });
                    myList.Add(new ServerInfo() { ServerName = "Bá Long", ServerCode = 1277212738, ServerIndex = 4, CmcIndex = 0x1 });
                    break;
                default:
                    break;
            }
            return myList;
        }

        public enum CumMayChu
        {
            HCM = 0,
            HN = 1,
            DT = 2
        }
    }
    public class CMC_Info
    {
        private string cMC_NAME;
        private INFO_VL2.CumMayChu cMC_Index;

        public string CMC_NAME { get => cMC_NAME; set => cMC_NAME = value; }
        public INFO_VL2.CumMayChu CMC_Index { get => cMC_Index; set => cMC_Index = value; }
    }
    public class ServerInfo
    {
        private string serverName;
        private uint serverCode;
        private byte serverIndex;
        private byte cmcIndex;
        public string ServerName { get => serverName; set => serverName = value; }
        public uint ServerCode { get => serverCode; set => serverCode = value; }
        public byte ServerIndex { get => serverIndex; set => serverIndex = value; }
        public byte CmcIndex { get => cmcIndex; set => cmcIndex = value; }
    }
}
