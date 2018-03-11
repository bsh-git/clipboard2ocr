using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace clipboard2ocr
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

		public void ShowSettingsDialog(Form1 parent)
		{
			Console.WriteLine("Showing dialog");
            textBox1.Text = parent.ApiKey;
			Console.WriteLine("current apikey={0}", parent.ApiKey);
			if (this.ShowDialog() == DialogResult.OK)
			{
                parent.UpdateApiKey(textBox1.Text);
			}
		}
    }
}
