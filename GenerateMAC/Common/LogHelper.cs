using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenerateMAC
{
    public class LogHelper
    {
        private static ILog log = null;
        public static ILog Log
        {
            get
            {
                if (log == null)
                {
                    //log4.config表示log4的配置文件
                    string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "log4.config");
                    log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(fileName));
                    //Log4Name表示配置文件中的日志名称
                    log = LogManager.GetLogger("GenerateMAC");
                }
                return log;
            }
        }
    }
}
