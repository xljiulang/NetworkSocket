using Models;
using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using NetworkSocket.Fast;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace FastClient
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            this.Load += MainForm_Load;
            this.btn_Login.Click += btn_Login_Click;
            this.btn_Pass.Click += btn_Pass_Click;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            // 断开自动重连间隔一秒
            Client.Instance.ReconnectPeriod = TimeSpan.FromSeconds(1);

            var connected = Client.Instance.Connect("localhost", 1212) == SocketError.Success;
            var version = connected ? await Client.Instance.GetVersion() : null;

            this.Text = connected ? ("通讯库版本：" + version) : "连接服务器失败 ..";
            this.btn_Login.Enabled = this.btn_Pass.Enabled = connected;
        }


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Login_Click(object sender, EventArgs e)
        {
            try
            {
                var user = new UserInfo { Account = this.textBox_Account.Text, Password = this.textBox_Password.Text };
                var result = await Client.Instance.Login(user, false);

                if (result.State == false)
                {
                    MessageBox.Show(result.Message, "服务器提示");
                }
                else
                {
                    this.SwitchSumForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示");
            }
        }


        /// <summary>
        /// 略过登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Pass_Click(object sender, EventArgs e)
        {
            this.SwitchSumForm();
        }

        /// <summary>
        /// 转换到SumForm
        /// </summary>
        private void SwitchSumForm()
        {
            this.Hide();
            var sumForm = new FormSum();
            sumForm.ShowDialog();
            this.Close();
        }
    }
}
