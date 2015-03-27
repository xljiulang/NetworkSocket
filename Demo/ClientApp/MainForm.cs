using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class MainForm : Form
    {
        private FastServer myServer = new FastServer();

        public MainForm()
        {
            InitializeComponent();

            this.btn_Login.Click += btn_Login_Click;
            this.Load += MainForm_Load;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 4502);
            var state = await this.myServer.Connect(endPoint);

            this.Text = "连接到服务器" + (state ? "成功" : "失败");
            this.btn_Login.Enabled = state;
        }

        private async void btn_Login_Click(object sender, EventArgs e)
        {
            var user = new Models.User { Account = this.textBox_Account.Text, Password = this.textBox_Password.Text };
            var state = await this.myServer.Login(user, false);

            if (state == false)
            {
                MessageBox.Show("登录" + (state ? "成功" : "失败"));
            }
            else
            {
                this.Hide();
                new SumForm(this.myServer).ShowDialog();
                this.Close();
            }
        }

    }
}
