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

namespace VisualRedis.UserWindows
{
    /// <summary>
    /// SubscribeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SubscribeWindow : Window
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public SubscribeWindow()
        {
            InitializeComponent();
        }

        public SubscribeWindow(ConnectionMultiplexer connectionMultiplexer) : this()
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        private void SubBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SubBtn.Content.ToString() == "订阅")
            {
                if (string.IsNullOrWhiteSpace(subName.Text))
                {
                    MessageBox.Show("名称不能为空");
                    return;
                }

                SubBtn.Content = "停止";
                _connectionMultiplexer.GetSubscriber().SubscribeAsync(subName.Text, Sub);
                subName.IsEnabled = false;
            }
            else
            {
                SubBtn.Content = "订阅";
                _connectionMultiplexer.GetSubscriber().Unsubscribe(subName.Text);
                subName.IsEnabled = true;
            }
        }

        private void Sub(RedisChannel channel, RedisValue redisValue)
        {
            this.Dispatcher.Invoke(() => subMsg.Text += $"{redisValue}\r\n");
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            subMsg.Text = "";
        }
    }
}
