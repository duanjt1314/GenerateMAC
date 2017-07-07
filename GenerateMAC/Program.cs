using HZ.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace GenerateMAC
{
    static class Program
    {
        private static readonly string ServiceName = "GenerateMACService";

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main1()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestForm());
        }

        static void Main(string[] args)
        {
            //设置应用程序处理异常方式：ThreadException处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //处理非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (args != null && args.Length > 0)
            {
                if (args[0] == "-service")
                {//启动服务
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
					{
						new GenerateMACService()
					};
                    ServiceBase.Run(ServicesToRun);
                    return;
                }
                else if (args[0] == "-install")
                {//安装服务
                    try
                    {
                        using (AssemblyInstaller assemblyInstaller = new AssemblyInstaller())
                        {
                            assemblyInstaller.UseNewContext = true;
                            assemblyInstaller.Path = Assembly.GetExecutingAssembly().Location;
                            assemblyInstaller.Install(null);
                            assemblyInstaller.Commit(null);
                        }
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        RegistryKey regkey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Services\" + ServiceName, true);
                        object obj = regkey.GetValue("ImagePath");
                        if (obj != null)
                        {
                            regkey.SetValue("ImagePath", obj.ToString() + " -service");
                            regkey.Flush();
                        }
                        regkey.Close();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    return;
                }
                else if (args[0] == "-uninstall")
                {//卸载服务
                    using (AssemblyInstaller assemblyInstaller = new AssemblyInstaller())
                    {
                        assemblyInstaller.UseNewContext = true;
                        assemblyInstaller.Path = Assembly.GetExecutingAssembly().Location;
                        assemblyInstaller.Uninstall(null);
                    }

                    return;
                }
            }
            //应用程序方式启动, 启动窗体
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormServiceControl(ServiceName, "MAC定时生成程序", Application.ExecutablePath));
        }
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            // Config.Instance.LogWriter.WriteErrorLog(e.ToString());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Config.Instance.LogWriter.WriteErrorLog(e.ToString());
        }
    }
}
