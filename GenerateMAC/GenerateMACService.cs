using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace GenerateMAC
{
    public partial class GenerateMACService : ServiceBase
    {
        public GenerateMACService()
        {
            InitializeComponent();
        }

        StartUp start = new StartUp();

        protected override void OnStart(string[] args)
        {
            start.Start();
        }

        protected override void OnStop()
        {
            start.Stop();
        }
    }
}
