using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualRedis.Models;
using VisualRedis.UserWindows;

namespace VisualRedis.UserControls
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : UserControl
    {
        public delegate void ConnectionListUpdate(IList<Connection> connections);

        public event ConnectionListUpdate OnConnectionListUpdate;

        private readonly IList<Connection> _connections;

        public StartPage()
        {
            InitializeComponent();
        }

        public StartPage(IList<Connection> connections) : this()
        {
            _connections = connections;
        }

        private void OpenGithub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/lhdboy"));
        }

        private void CreateConnection_Click(object sender, RoutedEventArgs e)
        {
            AddConnectionWindow connectionWindow = new AddConnectionWindow(_connections);
            connectionWindow.OnConnectionListUpdate += ConnectionWindow_OnConnectionListUpdate;
            connectionWindow.ShowDialog();
        }

        private void ConnectionWindow_OnConnectionListUpdate(IList<Connection> connections)
        {
            OnConnectionListUpdate?.Invoke(connections);
        }

        private void FollowMe_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://weibo.com/930179151"));
        }
    }
}
