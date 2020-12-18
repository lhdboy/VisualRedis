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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VisualRedis.UserWindows;
using VisualRedis.Utils;

namespace VisualRedis.UserControls
{
    /// <summary>
    /// ListPage.xaml 的交互逻辑
    /// </summary>
    public partial class ListPage : UserControl
    {
        public delegate void ClosePage();

        public event ClosePage OnClosePage;

        public delegate void RefershPage();

        public event RefershPage OnRefershPage;

        private List<RedisValue> FieIdList;

        private readonly ConnectionMultiplexer _connection;
        private readonly int _dbIndex;
        private readonly string _keyName;

        public ListPage()
        {
            InitializeComponent();
        }

        public ListPage(ConnectionMultiplexer connection, int dbIndex, string keyName) : this()
        {
            _connection = connection;
            _dbIndex = dbIndex;
            _keyName = keyName;

            SetListValue();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_keyName);
            MessageBox.Show("复制成功");
        }

        private void Refersh_Click(object sender, RoutedEventArgs e)
        {
            listValue.Text = "";
            SetListValue();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("你确定要删除这个key么？", "等一下", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _connection.GetDatabase(_dbIndex).KeyDelete(_keyName);

                MessageBox.Show("删除成功");
                OnRefershPage?.Invoke();
                OnClosePage?.Invoke();
            }
        }

        private void Timeout_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(TimeoutInput.Text, out long idle) && idle > 0)
            {
                _connection.GetDatabase(_dbIndex).KeyExpire(_keyName, TimeSpan.FromMilliseconds(idle));
                SetListValue();
                MessageBox.Show("设置成功");
            }
            else
            {
                MessageBox.Show("毫秒格式不正确");
            }
        }

        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            CreateKeyWindows createKeyWindows = new CreateKeyWindows(_connection, _dbIndex, listKey.Text, 0);
            createKeyWindows.OnRefershPage += SetListValue;
            createKeyWindows.ShowDialog();
        }

        private void DeleteLine_Click(object sender, RoutedEventArgs e)
        {
            if (FieIdListView.SelectedIndex >= 0 && FieIdListView.SelectedItem != null)
            {
                if (MessageBox.Show("你确定要删除这行么？", "等一下", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _connection.GetDatabase(_dbIndex).ListRemove(_keyName, FieIdListView.SelectedItem.ToString());

                    MessageBox.Show("删除成功");
                    SetListValue();
                }
            }
            else
            {
                MessageBox.Show("请选择一行以删除");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (FieIdListView.SelectedIndex >= 0)
            {
                _connection.GetDatabase(_dbIndex).ListSetByIndex(_keyName, FieIdListView.SelectedIndex, listValue.Text);
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("请选择一个以保存");
            }
        }

        private void FieIdListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieIdListView.SelectedIndex >= 0)
            {
                listValue.Text = RedisUtil.TryGetValueJsonFormat(_connection.GetDatabase(_dbIndex).ListGetByIndex(_keyName, FieIdListView.SelectedIndex));
                viewSize.Text = Encoding.UTF8.GetBytes(listValue.Text).Length.ToString();
            }
        }

        private void ValueView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(listValue.Text);
            MessageBox.Show("复制成功");
        }

        private void SetListValue()
        {
            listKey.Text = _keyName;
            FieIdList = _connection.GetDatabase(_dbIndex).ListRange(_keyName).ToList();

            FieIdListView.Items.Clear();

            foreach (var item in FieIdList)
            {
                FieIdListView.Items.Add(item.ToString());
            }

            viewDbIndex.Text = _dbIndex.ToString();
            viewListCount.Text = FieIdList.Count.ToString();

            var timeout = _connection.GetDatabase(_dbIndex).KeyTimeToLive(_keyName);
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

        private void InsertBottom_Click(object sender, RoutedEventArgs e)
        {
            CreateKeyWindows createKeyWindows = new CreateKeyWindows(_connection, _dbIndex, listKey.Text, 1);
            createKeyWindows.OnRefershPage += SetListValue;
            createKeyWindows.ShowDialog();
        }
    }
}
