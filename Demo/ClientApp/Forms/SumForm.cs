using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp.Forms
{
    public partial class SumForm : Form
    {
        public SumForm()
        {
            InitializeComponent();
            this.button_Sum.Click += button_Sum_Click;
        }

        private async void button_Sum_Click(object sender, EventArgs e)
        {
            var x = 0;
            int y = 0;
            int z = 0;

            int.TryParse(this.textBox1.Text, out x);
            int.TryParse(this.textBox2.Text, out y);
            int.TryParse(this.textBox3.Text, out z);

            try
            {
                var sum = await RemoteServer.Instance.GetSun(x, y, z);
                MessageBox.Show(string.Format("{0} + {1} + {2} = {3}", x, y, z, sum), "计算结果");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统出错了");
            }
        }
    }
}
