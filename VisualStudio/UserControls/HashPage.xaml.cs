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
    /// HashPage.xaml 的交互逻辑
    /// </summary>
    public partial class HashPage : UserControl
    {
        public delegate void ClosePage();

        public event ClosePage OnClosePage;

        public delegate void RefershPage();

        public event RefershPage OnRefershPage;

        private readonly ConnectionMultiplexer _connection;
        private readonly int _dbIndex;
        private readonly string _keyName;

        private List<RedisValue> FieIdList;

        public HashPage()
        {
            InitializeComponent();
        }

        public HashPage(ConnectionMultiplexer connection, int dbIndex, string keyName) : this()
        {
            _connection = connection;
            _dbIndex = dbIndex;
            _keyName = keyName;

            SetHashValue();
        }

        private void Timeout_Click(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(TimeoutInput.Text, out long idle) && idle > 0)
            {
                _connection.GetDatabase(_dbIndex).KeyExpire(_keyName, TimeSpan.FromMilliseconds(idle));
                SetHashValue();
                MessageBox.Show("设置成功");
            }
            else
            {
                MessageBox.Show("毫秒格式不正确");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(hashFieldValue.Text))
            {
                _connection.GetDatabase(_dbIndex).HashSet(_keyName, hashFieldValue.Text, hashValue.Text);
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("请选择一个FieId以保存");
            }
        }

        private void Refersh_Click(object sender, RoutedEventArgs e)
        {
            hashFieldValue.Text = "";
            hashValue.Text = "";
            viewSize.Text = "";
            SetHashValue();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_keyName);
            MessageBox.Show("复制成功");
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

        private void FieldView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(hashFieldValue.Text);
            MessageBox.Show("复制成功");
        }

        private void ValueView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(hashValue.Text);
            MessageBox.Show("复制成功");
        }

        private void SetHashValue()
        {
            hashKey.Text = _keyName;
            FieIdList = _connection.GetDatabase(_dbIndex).HashKeys(_keyName).ToList();

            FieIdListView.Items.Clear();

            foreach (var item in FieIdList)
            {
                FieIdListView.Items.Add(item.ToString());
            }

            viewDbIndex.Text = _dbIndex.ToString();
            viewDataKeyCount.Text = FieIdList.Count.ToString();

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

        private void FieIdListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FieIdListView.SelectedIndex >= 0)
            {
                hashFieldValue.Text = FieIdList[FieIdListView.SelectedIndex].ToString();
                hashValue.Text = RedisUtil.TryGetValueJsonFormat(_connection.GetDatabase(_dbIndex).HashGet(_keyName, FieIdList[FieIdListView.SelectedIndex]));
                viewSize.Text = Encoding.UTF8.GetBytes(hashValue.Text).Length.ToString();
            }
        }

        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            CreateKeyWindows createKeyWindows = new CreateKeyWindows(_connection, _dbIndex, hashKey.Text);
            createKeyWindows.OnRefershPage += SetHashValue;
            createKeyWindows.ShowDialog();
        }

        private void DeleteLine_Click(object sender, RoutedEventArgs e)
        {
            if (FieIdListView.SelectedIndex >= 0)
            {
                if (MessageBox.Show("大哥，你确定要删除这行么？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _connection.GetDatabase(_dbIndex).HashDelete(_keyName, FieIdList[FieIdListView.SelectedIndex].ToString());
                    MessageBox.Show("删除成功");
                    SetHashValue();
                }
            }
            else
            {
                MessageBox.Show("请选择一个FieId以删除");
            }
        }
    }
}
