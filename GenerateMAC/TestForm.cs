using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using HZ.Common;

namespace GenerateMAC
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }

        StartUp start = new StartUp();
        private void button1_Click(object sender, EventArgs e)
        {
            start.Start();
            this.btnStart.Enabled = false;
            this.btnStop.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            start.Stop();
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            XElement ele = XElement.Load("config/log4.config");
            var a = ele.Element("appender").Element("layout").Attribute("type");
            MessageBox.Show(a.GetXmlPath());
        }
    }
}
