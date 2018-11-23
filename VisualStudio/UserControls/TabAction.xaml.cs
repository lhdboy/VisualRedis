using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace VisualRedis.UserControls
{
    /// <summary>
    /// TabAction.xaml 的交互逻辑
    /// </summary>
    public partial class TabAction : UserControl
    {
        public int TabActionIndex { get; set; }

        public object TabItemObj { get; set; }

        public delegate void CloseClick(object index);

        public event CloseClick OnCloseClick;

        public TabAction()
        {
            InitializeComponent();
        }

        public TabAction(string title, object tabItemObj) : this()
        {
            TabTitle.Text = title;
            TabItemObj = tabItemObj;
        }

        private void TabClose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseClick?.Invoke(TabItemObj);
        }
    }
}
