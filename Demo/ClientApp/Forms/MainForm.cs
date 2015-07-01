using Models;
using NetworkSocket.Fast;
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
            this.btn_Pass.Click += btn_Pass_Click;
            this.Load += MainForm_Load;
        }


        private async void MainForm_Load(object sender, EventArgs e)
        {            
            var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 1380);
            var state = await RemoteServer.Instance.Connect(endPoint);
            var version = state ? await RemoteServer.Instance.GetVersion() : "未知";

            this.Text = "连接" + (state ? "成功" : "失败") + " 服务版本：" + version;
            this.btn_Login.Enabled = this.btn_Pass.Enabled = state;
        }

        private void btn_Pass_Click(object sender, EventArgs e)
        {
            this.Hide();
            new SumForm().ShowDialog();
            this.Close();
        }

        private async void btn_Login_Click(object sender, EventArgs e)
        {
            var user = new User
            {
                Account = this.textBox_Account.Text,
                Password = this.textBox_Password.Text
            };

            try
            {
                var state = await RemoteServer.Instance.Login(user, false);
                if (state == false)
                {
                    MessageBox.Show("账号或密码错误...", "系统提示");
                }
                else
                {
                    this.Hide();
                    new SumForm().ShowDialog();
                    this.Close();
                }
            }
            catch (TimeoutException ex)
            {
                this.Text = ex.Message;
            }
            catch (RemoteException ex)
            {
                MessageBox.Show(ex.Message, "远程服务器异常");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "未分类的异常");
            }
        }

    }
}
