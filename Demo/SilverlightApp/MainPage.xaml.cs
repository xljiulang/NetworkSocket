using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightApp
{
    public partial class MainPage : UserControl
    {
        private FastServer server = new FastServer();

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.server.Connect(IPAddress.Loopback, 4502).ContinueWith((r) =>
            {
                this.Dispatcher.BeginInvoke(() => this.LabelMsg.Content = "连接到服务器" + (r.Result ? "成功" : "失败"));
            });
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = new Models.User { Account = this.TextAccount.Text, Password = this.TextPassword.Text };
            this.server.Login(user, false).ContinueWith((r) =>
            {
                this.Dispatcher.BeginInvoke(() => this.LabelMsg.Content = "登录" + (r.Result ? "成功" : "失败"));
            });
        }
    }
}
