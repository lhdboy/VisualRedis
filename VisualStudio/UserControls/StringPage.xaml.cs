using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
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
using VisualRedis.Utils;

namespace VisualRedis.UserControls
{
    /// <summary>
    /// StringPage.xaml 的交互逻辑
    /// </summary>
    public partial class StringPage : UserControl
    {
        public delegate void ClosePage();

        public event ClosePage OnClosePage;

        public delegate void RefershPage();

        public event RefershPage OnRefershPage;

        private readonly ConnectionMultiplexer _connection;
        private readonly int _dbIndex;
        private readonly string _keyName;

        public StringPage()
        {
            InitializeComponent();
        }

        public StringPage(ConnectionMultiplexer connection, int dbIndex, string keyName) : this()
        {
            _connection = connection;
            _dbIndex = dbIndex;
            _keyName = keyName;

            SetStringValue();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_keyName);
            MessageBox.Show("复制成功");
        }

        private void Refersh_Click(object sender, RoutedEventArgs e)
        {
            SetStringValue();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("大哥，你确定要删除这个key么？", "等一下", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _connection.GetDatabase(_dbIndex).KeyDelete(_keyName);

                MessageBox.Show("删除成功");
                OnRefershPage?.Invoke();
                OnClosePage?.Invoke();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _connection.GetDatabase(_dbIndex).StringSet(_keyName, stringValue.Text);
            MessageBox.Show("保存成功");
        }

        private void Timeout_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(TimeoutInput.Text, out long idle) && idle > 0)
            {
                _connection.GetDatabase(_dbIndex).KeyExpire(_keyName, TimeSpan.FromMilliseconds(idle));
                SetStringValue();
                MessageBox.Show("设置成功");
            }
            else
            {
                MessageBox.Show("毫秒格式不正确");
            }
        }

        private void SetStringValue()
        {
            stringKey.Text = _keyName;
            stringValue.Text = RedisUtil.TryGetValueJsonFormat(_connection.GetDatabase(_dbIndex).StringGet(_keyName));

            viewDbIndex.Text = _dbIndex.ToString();
            viewSize.Text = Encoding.UTF8.GetBytes(stringValue.Text).Length.ToString();

            var timeout = _connection.GetDatabase(_dbIndex).KeyIdleTime(_keyName);
            if (timeout != null)
            {
                viewTimeout.Text = timeout.Value.TotalMilliseconds.ToString();
                TimeoutInput.Text = viewTimeout.Text;
            }
            else
            {
                viewTimeout.Text = "-1（永久）";
                TimeoutInput.Text = "-1";
            }
        }
    }
}