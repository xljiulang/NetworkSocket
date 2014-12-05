using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp
{
    public partial class SumForm : Form
    {
        private FastServer myServer;

        public SumForm(FastServer server)
        {
            InitializeComponent();

            this.myServer = server;
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

            var sum = await this.myServer.GetSun(x, y, z);
            MessageBox.Show("服务器返回：" + sum.ToString());
        }       
    }
}
