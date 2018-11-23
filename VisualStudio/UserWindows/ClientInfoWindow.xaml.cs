using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VisualRedis.Utils;

namespace VisualRedis.UserWindows
{
    /// <summary>
    /// ClientInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientInfoWindow : Window
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public ClientInfoWindow()
        {
            InitializeComponent();
        }

        public ClientInfoWindow(ConnectionMultiplexer connectionMultiplexer) : this()
        {
            _connectionMultiplexer = connectionMultiplexer;

            LoadInfo();
        }

        private void LoadInfo()
        {
            var server = RedisUtil.GetServer(_connectionMultiplexer);

            //服务器信息
            var info = server.Info();
            foreach (var serverInfo in info)
            {
                clientProperty.Items.Add(new TextBlock() { Text = $"【{serverInfo.Key}】" });
                foreach (var serverInfoData in serverInfo)
                {
                    clientProperty.Items.Add(new TextBlock() { Text = $"{serverInfoData.Key}：{serverInfoData.Value}" });
                }
            }

            //客户端连接列表
            var list = server.ClientList();
            clientDataGrid.ItemsSource = list;
        }
    }
}
