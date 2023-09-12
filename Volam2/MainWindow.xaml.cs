using LibraryHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Volam2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
            var listCMC = new List<CMC_Info>();
            listCMC.Add(new CMC_Info() { CMC_Index = INFO_VL2.CumMayChu.HCM, CMC_NAME = "TP Hồ Chí Minh"});
            listCMC.Add(new CMC_Info() { CMC_Index = INFO_VL2.CumMayChu.HN, CMC_NAME = "Hà Nội" });
            listCMC.Add(new CMC_Info() { CMC_Index = INFO_VL2.CumMayChu.DT, CMC_NAME = "Đấu trường" });
            txt_CMC.ItemsSource = listCMC;
            txt_CMC.DisplayMemberPath = "CMC_NAME";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txt_server.SelectedItem is ServerInfo server && txt_CMC.SelectedItem is CMC_Info cmc)
            {
                // Định danh tiến trình đích bằng ProcessID
                var processId = Process.GetProcessesByName("so2game").First();
                // Mở tiến trình đích
                IntPtr hProcess = MemoryHelper.GetHandleProcess(processId.Id);

                INFO_VL2.Call_StartGame(hProcess);
                INFO_VL2.Call_SelectServer(hProcess, cmc, server);
            }
            else
            {
                MessageBox.Show("Chưa chọn server");
            }
            
        }

        private void txt_CMC_DropDownClosed(object sender, EventArgs e)
        {
            txt_server.ItemsSource = null;
            if (txt_CMC.SelectedItem is CMC_Info infocmc)
            {
                var listServer = INFO_VL2.ListServer(infocmc.CMC_Index);
                txt_server.ItemsSource = listServer;
                txt_server.DisplayMemberPath = "ServerName";
            }
        }
    }

    
}

