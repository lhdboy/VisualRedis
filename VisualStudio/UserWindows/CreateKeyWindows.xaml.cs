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
    /// CreateKeyWindows.xaml 的交互逻辑
    /// </summary>
    public partial class CreateKeyWindows : Window
    {
        public delegate void RefershPage();

        public event RefershPage OnRefershPage;

        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly int _dbIndex;

        public CreateKeyWindows()
        {
            InitializeComponent();
        }

        public CreateKeyWindows(ConnectionMultiplexer connectionMultiplexer, int dbIndex) : this()
        {
            _connectionMultiplexer = connectionMultiplexer;
            _dbIndex = dbIndex;
        }

        public CreateKeyWindows(ConnectionMultiplexer connectionMultiplexer, int dbIndex, string hashKeyName) : this()
        {
            _connectionMultiplexer = connectionMultiplexer;
            _dbIndex = dbIndex;

            hashKey.Text = hashKeyName;
            CreateTab.SelectedIndex = 1;
        }

        public CreateKeyWindows(ConnectionMultiplexer connectionMultiplexer, int dbIndex, string listKeyName, int insertIndex) : this()
        {
            _connectionMultiplexer = connectionMultiplexer;
            _dbIndex = dbIndex;

            listKey.Text = listKeyName;
            CreateTab.SelectedIndex = 2;
            listLeftRight.SelectedIndex = insertIndex;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Button saveBtn = (Button)sender;
            switch (saveBtn.Name)
            {
                case "SaveString":
                    if (string.IsNullOrWhiteSpace(stringKey.Text) || string.IsNullOrWhiteSpace(stringValue.Text))
                    {
                        MessageBox.Show("请输入键名或键值");
                        return;
                    }

                    if (int.TryParse(stringTimeSpan.Text, out int stringTimeout))
                        _connectionMultiplexer.GetDatabase(_dbIndex).StringSet(stringKey.Text, stringValue.Text, TimeSpan.FromMilliseconds(stringTimeout));
                    else
                        _connectionMultiplexer.GetDatabase(_dbIndex).StringSet(stringKey.Text, stringValue.Text);

                    break;
                case "SaveHash":
                    if (string.IsNullOrWhiteSpace(hashKey.Text) || string.IsNullOrWhiteSpace(hashDataKey.Text) || string.IsNullOrWhiteSpace(hashValue.Text))
                    {
                        MessageBox.Show("请输入Hash键名、字段名或Hash键值");
                        return;
                    }

                    _connectionMultiplexer.GetDatabase(_dbIndex).HashSet(hashKey.Text, hashDataKey.Text, hashValue.Text);
                    break;
                case "SaveList":
                    if (string.IsNullOrWhiteSpace(listKey.Text) || string.IsNullOrWhiteSpace(listValue.Text))
                    {
                        MessageBox.Show("请输入List键名或键值");
                        return;
                    }

                    if (listLeftRight.SelectedIndex == 0)
                        _connectionMultiplexer.GetDatabase(_dbIndex).ListLeftPush(listKey.Text, listValue.Text);
                    else
                        _connectionMultiplexer.GetDatabase(_dbIndex).ListRightPush(listKey.Text, listValue.Text);
                    break;
                case "SavePublish":
                    if (string.IsNullOrWhiteSpace(publishKey.Text) || string.IsNullOrWhiteSpace(publishValue.Text))
                    {
                        MessageBox.Show("请输入订阅名或值");
                        return;
                    }

                    _connectionMultiplexer.GetDatabase(_dbIndex).Publish(publishKey.Text, publishValue.Text);
                    break;
                default:
                    return;
            }

            MessageBox.Show("添加成功");

            OnRefershPage?.Invoke();

            Close();
        }
    }
}
