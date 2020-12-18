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

using VisualRedis.Models;
using VisualRedis.UserControls;
using VisualRedis.UserWindows;
using VisualRedis.Utils;

namespace VisualRedis
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IList<Connection> _connections;

        //已连接的redis
        private readonly List<ConnectionMultiplexer> connectionMultiplexersList;

        public MainWindow()
        {
            InitializeComponent();

            //初始化连接信息
            connectionMultiplexersList = new List<ConnectionMultiplexer>();

            _connections = ConfigUtil.Get();

            InitRedisList();

            //初始化起始页
            OpenStartPage();
        }

        private void OpenStartPage()
        {
            StartPage startPage = new StartPage(_connections);
            startPage.OnConnectionListUpdate += StartPage_OnConnectionListUpdate;

            AddTab(startPage, "起始页");
        }

        private void StartPage_OnConnectionListUpdate(IList<Connection> connections)
        {
            //清空标签页
            valueTabControl.Items.Clear();
            _connections = connections;
            OpenStartPage();

            //清空树
            if (connectionMultiplexersList != null && connectionMultiplexersList.Count > 0)
            {
                dbTreeView.Items.Clear();
                foreach (var item in connectionMultiplexersList)
                {
                    if (item != null)
                        item.Close();
                }

                connectionMultiplexersList.Clear();
            }

            //重新再次加载树
            InitRedisList();
        }

        private void InitRedisList()
        {
            foreach (Connection item in _connections)
            {
                int index = dbTreeView.Items.Count;

                TreeViewItem treeViewItem = new TreeViewItem();
                treeViewItem.Header = new TextBlock() { Text = item.ConnectionName, Padding = new Thickness(0, 6, 0, 6) };
                treeViewItem.ContextMenu = CreateServerMenu(treeViewItem, index);
                treeViewItem.Expanded += (s, e) =>
                {
                    if (s.Equals(treeViewItem) && treeViewItem.Items != null && treeViewItem.Items.Count <= 0)
                        OpenConnectAction(treeViewItem, index);
                };

                connectionMultiplexersList.Add(null);
                dbTreeView.Items.Add(treeViewItem);
            }
        }

        private TabItem AddTab(object tabItemObj, string title)
        {
            TabItem tabItem = new TabItem();

            TabAction tabAction = new TabAction(title, tabItem);
            tabAction.TabActionIndex = valueTabControl.Items.Count;
            tabAction.OnCloseClick += TabAction_OnCloseClick;

            tabItem.Content = tabItemObj;
            tabItem.Header = tabAction;

            valueTabControl.Items.Insert(0, tabItem);
            valueTabControl.SelectedIndex = 0;

            return tabItem;
        }

        private void TabAction_OnCloseClick(object item)
        {
            valueTabControl.Items.Remove(item);
        }

        private ContextMenu CreateServerMenu(TreeViewItem treeViewItem, int index)
        {
            MenuItem connectMenu = new MenuItem();
            connectMenu.Header = "连接";
            connectMenu.Click += (s, e) => OpenConnectAction(treeViewItem, index);

            MenuItem disconnectMenu = new MenuItem();
            disconnectMenu.Header = "断开连接";
            disconnectMenu.Click += (s, e) =>
            {
                if (connectionMultiplexersList.Count > index && connectionMultiplexersList[index] != null)
                {
                    var conn = connectionMultiplexersList[index];
                    conn.Close();
                    conn.Dispose();

                    connectionMultiplexersList[index] = null;

                    treeViewItem.Items.Clear();
                }
                else
                {
                    MessageBox.Show("你还没有连接！");
                }
            };

            MenuItem testMenu = new MenuItem();
            testMenu.Header = "Ping测试";
            testMenu.Click += (s, e) =>
            {
                if (connectionMultiplexersList.Count > index && connectionMultiplexersList[index] != null)
                {
                    double testResult = RedisUtil.Ping(connectionMultiplexersList[index]);

                    if (testResult == -1)
                    {
                        MessageBox.Show("这个Redis没用");
                    }
                    else if (testResult > 0)
                    {
                        MessageBox.Show($"连接平均耗时:{testResult}ms");
                    }
                }
                else
                {
                    MessageBox.Show("你还没有连接！");
                }
            };

            MenuItem subMenu = new MenuItem();
            subMenu.Header = "订阅器";
            subMenu.Click += (s, e) =>
            {
                if (connectionMultiplexersList.Count > index && connectionMultiplexersList[index] != null)
                {
                    SubscribeWindow subscribeWindow = new SubscribeWindow(connectionMultiplexersList[index]);
                    subscribeWindow.Show();
                }
                else
                {
                    MessageBox.Show("你还没有连接！");
                }
            };

            MenuItem refershMenu = new MenuItem();
            refershMenu.Header = "刷新";
            refershMenu.Click += (s, e) =>
            {
                if (connectionMultiplexersList.Count > index && connectionMultiplexersList[index] != null)
                {
                    //清空标签页
                    valueTabControl.Items.Clear();
                    OpenStartPage();
                    treeViewItem.Items.Clear();
                    OpenDbTree(treeViewItem, connectionMultiplexersList[index], index);
                }
                else
                {
                    MessageBox.Show("你还没有连接！");
                }
            };

            MenuItem deleteMenu = new MenuItem();
            deleteMenu.Header = "删除";
            deleteMenu.Click += (s, e) =>
            {
                if (MessageBox.Show("删除改连接后，连接列表将会重新加载，请问需要继续吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _connections.RemoveAt(index);
                    ConfigUtil.Update(_connections);

                    StartPage_OnConnectionListUpdate(ConfigUtil.Get());
                }
            };

            MenuItem infoMenu = new MenuItem();
            infoMenu.Header = "属性";
            infoMenu.Click += (s, e) =>
            {
                if (connectionMultiplexersList.Count > index && connectionMultiplexersList[index] != null)
                {
                    ClientInfoWindow clientInfoWindow = new ClientInfoWindow(connectionMultiplexersList[index]);
                    clientInfoWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("你还没有连接！");
                }
            };

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(connectMenu);
            contextMenu.Items.Add(disconnectMenu);
            contextMenu.Items.Add(subMenu);
            contextMenu.Items.Add(testMenu);
            contextMenu.Items.Add(refershMenu);
            contextMenu.Items.Add(deleteMenu);
            contextMenu.Items.Add(infoMenu);

            return contextMenu;
        }

        private ContextMenu CreateDbMenu(TreeViewItem treeViewItem, int connectIndex, int dbIndex)
        {
            MenuItem expandMenu = new MenuItem();
            expandMenu.Header = "展开";
            expandMenu.Click += (s, e) => OpenDbAction(treeViewItem, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);

            MenuItem copyMenu = new MenuItem();
            copyMenu.Header = "复制db名";
            copyMenu.Click += (s, e) =>
            {
                Clipboard.SetText($"db{dbIndex}");
            };

            MenuItem createMenu = new MenuItem();
            createMenu.Header = "新建key";
            createMenu.Click += (s, e) =>
            {
                CreateKeyWindows createKeyWindows = new CreateKeyWindows(connectionMultiplexersList[connectIndex], dbIndex);
                createKeyWindows.OnRefershPage += () => OpenDbAction(treeViewItem, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                createKeyWindows.ShowDialog();
            };

            MenuItem refershMenu = new MenuItem();
            refershMenu.Header = "刷新";
            refershMenu.Click += (s, e) =>
            {
                OpenDbAction(treeViewItem, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
            };

            MenuItem flushMenu = new MenuItem();
            flushMenu.Header = "清空";
            flushMenu.Click += (s, e) =>
            {
                if (MessageBox.Show("老哥，你确定要清空这个db么？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var server = GetConnectionServer(connectionMultiplexersList[connectIndex]);

                    server.FlushDatabase(dbIndex);

                    MessageBox.Show("清空成功");

                    OpenDbAction(treeViewItem, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                }
            };

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(expandMenu);
            contextMenu.Items.Add(copyMenu);
            contextMenu.Items.Add(createMenu);
            contextMenu.Items.Add(refershMenu);
            contextMenu.Items.Add(flushMenu);

            return contextMenu;
        }

        private ContextMenu CreateKeyMenu(TreeViewItem treeViewItem, int connectIndex, int dbIndex, string keyName)
        {
            MenuItem openMenu = new MenuItem();
            openMenu.Header = "打开";
            openMenu.Click += (s, e) =>
            {
                var connection = connectionMultiplexersList[connectIndex];

                var keyType = connection.GetDatabase(dbIndex).KeyType(keyName);

                switch (keyType)
                {
                    case RedisType.String:
                        StringPage stringPage = new StringPage(connection, dbIndex, keyName);
                        var stringTab = AddTab(stringPage, $"{keyType}：{keyName}");

                        stringPage.OnClosePage += () => valueTabControl.Items.Remove(stringTab);
                        stringPage.OnRefershPage += () => OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                        break;
                    case RedisType.List:
                        ListPage listPage = new ListPage(connection, dbIndex, keyName);
                        var listTab = AddTab(listPage, $"{keyType}：{keyName}");

                        listPage.OnClosePage += () => valueTabControl.Items.Remove(listTab);
                        listPage.OnRefershPage += () => OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                        break;
                    case RedisType.Hash:
                        HashPage hashPage = new HashPage(connection, dbIndex, keyName);
                        var hashTab = AddTab(hashPage, $"{keyType}：{keyName}");

                        hashPage.OnClosePage += () => valueTabControl.Items.Remove(hashTab);
                        hashPage.OnRefershPage += () => OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                        break;
                    default:
                        MessageBox.Show("不支持打开此类型");
                        break;
                }
            };

            MenuItem copyMenu = new MenuItem();
            copyMenu.Header = "复制key名";
            copyMenu.Click += (s, e) => Clipboard.SetText(keyName);

            MenuItem deleteMenu = new MenuItem();
            deleteMenu.Header = "删除";
            deleteMenu.Click += (s, e) =>
            {
                if (MessageBox.Show("你确定要删除这个key么？", "等一下", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    connectionMultiplexersList[connectIndex].GetDatabase(dbIndex).KeyDelete(keyName);

                    //刷新
                    OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);

                    MessageBox.Show("删除成功");
                }
            };

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(openMenu);
            contextMenu.Items.Add(copyMenu);
            contextMenu.Items.Add(deleteMenu);

            return contextMenu;
        }

        #region 双击共用方法
        private void OpenConnectAction(TreeViewItem treeViewItem, int connectIndex)
        {
            if (connectionMultiplexersList.Count > connectIndex && connectionMultiplexersList[connectIndex] == null)
            {
                connectionMultiplexersList[connectIndex] = ConnectionMultiplexer.Connect(_connections[connectIndex].ConnectionString);

                OpenDbTree(treeViewItem, connectionMultiplexersList[connectIndex], connectIndex);
            }
            else
            {
                MessageBox.Show("已经连接了啊！");
            }
        }

        private void OpenDbAction(TreeViewItem treeViewItem, ConnectionMultiplexer connectionMultiplexer, int connectIndex, int dbIndex)
        {
            treeViewItem.Items.Clear();

            var server = GetConnectionServer(connectionMultiplexer).Keys(dbIndex).ToList();

            for (int i = 0; i < server.Count; i++)
            {
                string keyName = server[i].ToString();

                TreeViewItem keyTreeViewItem = new TreeViewItem();
                keyTreeViewItem.Header = new TextBlock() { Text = $"{connectionMultiplexer.GetDatabase(dbIndex).KeyType(server[i])}：{keyName}", Padding = new Thickness(0, 6, 0, 6) };
                keyTreeViewItem.ContextMenu = CreateKeyMenu(keyTreeViewItem, connectIndex, dbIndex, server[i]);
                keyTreeViewItem.Expanded += (s, e) =>
                 {
                     if (s.Equals(keyTreeViewItem))
                     {
                         var connection = connectionMultiplexersList[connectIndex];

                         var keyType = connection.GetDatabase(dbIndex).KeyType(keyName);

                         switch (keyType)
                         {
                             case RedisType.String:
                                 StringPage stringPage = new StringPage(connection, dbIndex, keyName);
                                 var stringTab = AddTab(stringPage, $"{keyType}：{keyName}");

                                 stringPage.OnClosePage += () => valueTabControl.Items.Remove(stringTab);
                                 stringPage.OnRefershPage += () => OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                                 break;
                             case RedisType.List:
                                 ListPage listPage = new ListPage(connection, dbIndex, keyName);
                                 var listTab = AddTab(listPage, $"{keyType}：{keyName}");

                                 listPage.OnClosePage += () => valueTabControl.Items.Remove(listTab);
                                 listPage.OnRefershPage += () => OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                                 break;
                             case RedisType.Hash:
                                 HashPage hashPage = new HashPage(connection, dbIndex, keyName);
                                 var hashTab = AddTab(hashPage, $"{keyType}：{keyName}");

                                 hashPage.OnClosePage += () => valueTabControl.Items.Remove(hashTab);
                                 hashPage.OnRefershPage += () => OpenDbAction((TreeViewItem)treeViewItem.Parent, connectionMultiplexersList[connectIndex], connectIndex, dbIndex);
                                 break;
                             default:
                                 MessageBox.Show("不支持打开此类型");
                                 break;
                         }
                     }
                 };

                treeViewItem.Items.Add(keyTreeViewItem);
            }

            treeViewItem.IsExpanded = true;
        }

        private void OpenDbTree(TreeViewItem treeViewItem, ConnectionMultiplexer connection, int connectIndex)
        {
            var server = GetConnectionServer(connection);

            int dbCount = server.DatabaseCount == 0 ? 1 : server.DatabaseCount;

            for (int i = 0; i < dbCount; i++)
            {
                int dbIndex = i;

                TreeViewItem dbTreeViewItem = new TreeViewItem();
                dbTreeViewItem.Header = new TextBlock() { Text = $"db{dbIndex}（key：{server.DatabaseSize(dbIndex)}）", Padding = new Thickness(0, 6, 0, 6) };
                dbTreeViewItem.ContextMenu = CreateDbMenu(dbTreeViewItem, connectIndex, dbIndex);
                dbTreeViewItem.Expanded += (s, e) =>
                {
                    if (s.Equals(dbTreeViewItem) && dbTreeViewItem.Items != null && dbTreeViewItem.Items.Count <= 0)
                        OpenDbAction(dbTreeViewItem, connection, connectIndex, dbIndex);
                };

                treeViewItem.Items.Add(dbTreeViewItem);
            }

            treeViewItem.IsExpanded = true;
        }
        #endregion

        private IServer GetConnectionServer(ConnectionMultiplexer connectionMultiplexer)
        {
            foreach (var ep in connectionMultiplexer.GetEndPoints())
                return connectionMultiplexer.GetServer(ep);

            throw new NullReferenceException();
        }
    }
}
