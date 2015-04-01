using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClientApp.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.btn_Login.Click += btn_Login_Click;
            this.Load += MainForm_Load;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 4502);
            var state = await RemoteServer.Instance.Connect(endPoint);
            var version = state ? await RemoteServer.Instance.GetVersion() : "未知";

            this.Text = "连接" + (state ? "成功" : "失败") + " 服务版本：" + version;
            this.btn_Login.Enabled = state;
        }

        private async void btn_Login_Click(object sender, EventArgs e)
        {
            var user = new User
            {
                Account = this.textBox_Account.Text,
                Password = this.textBox_Password.Text
            };

            var state = await RemoteServer.Instance.Login(user, false);
            if (state == false)
            {
                MessageBox.Show("登录" + (state ? "成功" : "失败"));
            }
            else
            {
                this.Hide();
                new SumForm().ShowDialog();
                this.Close();
            }
        }

    }
}
