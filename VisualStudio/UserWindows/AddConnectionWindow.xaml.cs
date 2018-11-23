using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VisualRedis.Models;
using VisualRedis.Utils;
using StackExchange.Redis;

namespace VisualRedis.UserWindows
{
    /// <summary>
    /// AddConnectionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddConnectionWindow : Window
    {
        public delegate void ConnectionListUpdate(IList<Connection> connections);

        public event ConnectionListUpdate OnConnectionListUpdate;

        private readonly IList<Connection> _connections;

        public AddConnectionWindow()
        {
            InitializeComponent();
        }

        public AddConnectionWindow(IList<Connection> connections) : this()
        {
            _connections = connections;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save.IsEnabled = false;
            Test.IsEnabled = false;

            if (CheckInput()) return;

            if (TestConnection() <= 0)
            {
                MessageBox.Show("这个Redis连接无效");

                Save.IsEnabled = true;
                Test.IsEnabled = true;
                return;
            }

            if (!connStr.Text.Contains("allowAdmin"))
                connStr.Text += ",allowAdmin=true";

            _connections.Add(new Connection() { ConnectionName = connName.Text, ConnectionString = connStr.Text });

            ConfigUtil.Update(_connections);

            OnConnectionListUpdate?.Invoke(_connections);

            Save.IsEnabled = true;
            Test.IsEnabled = true;

            this.Close();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            Save.IsEnabled = false;
            Test.IsEnabled = false;

            double testResult = TestConnection();

            if (testResult == -1)
            {
                MessageBox.Show("这个Redis没用");
            }
            else if (testResult > 0)
            {
                MessageBox.Show($"这个Redis可以使用，连接平均耗时:{testResult}ms");
            }

            Save.IsEnabled = true;
            Test.IsEnabled = true;
        }

        private double TestConnection()
        {
            if (CheckInput()) return -2;

            try
            {
                ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(connStr.Text);

                return RedisUtil.Ping(connectionMultiplexer);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private bool CheckInput()
        {
            if (string.IsNullOrWhiteSpace(connName.Text) || string.IsNullOrWhiteSpace(connStr.Text))
            {
                MessageBox.Show("连接名称和连接字符串不能为空");

                Save.IsEnabled = true;
                Test.IsEnabled = true;

                return true;
            }

            return false;
        }
    }
}
