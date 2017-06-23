using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HZ.Common;

namespace GenerateMAC
{
    /// <summary>
    /// 系统配置类
    /// </summary>
    class SysConfig
    {
        #region 属性
        /// <summary>
        /// 文件输出目录
        /// </summary>
        public string OutDir { get; private set; }
        /// <summary>
        /// 场所编码的集合
        /// </summary>
        public List<string> SiteIds { get; private set; }
        /// <summary>
        /// 需要转出的配置
        /// </summary>
        public List<LayoutItem> LayoutItems { get; private set; }
        #endregion

        private static SysConfig _install = null;

        public static SysConfig Install
        {
            get
            {
                if (_install == null)
                {
                    _install = new SysConfig();
                    _install.LoadConfig();
                    _install.LoadSites();
                    _install.LoadLayout();
                }
                return _install;
            }
        }

        /// <summary>
        /// 加载系统配置xml文件[config/config.xml]
        /// </summary>
        private void LoadConfig()
        {
            string fileName = string.Empty;
            try
            {
                fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "config.xml");
                if (!File.Exists(fileName))
                {
                    LogHelper.Log.Error("配置文件:" + fileName + "不存在,请检查");
                    return;
                }

                XElement config = XElement.Load(fileName);
                XElement db = config.GetXmlElement("db");
                var ip = db.GetXmlElement("ip").Value;
                var port = db.GetXmlElement("port").Value;
                var serviceName = db.GetXmlElement("serviceName").Value;
                var userName = db.GetXmlElement("userName").Value;
                var pwd = db.GetXmlElement("pwd").Value;

                SqlDBHelper.connStr = string.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", ip, serviceName, userName, pwd);
                this.OutDir = config.GetXmlElement("outDir").Value;

                LogHelper.Log.Info("配置文件:" + fileName + "加载成功\r\n连接字符串:" + SqlDBHelper.connStr + "\r\n输出目录:" + this.OutDir);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("加载配置文件:" + fileName + "失败,请检查", ex);
            }
        }

        /// <summary>
        /// 加载待生成的场所文件[config/sites.xml]
        /// </summary>
        private void LoadSites()
        {
            string fileName = string.Empty;
            try
            {
                this.SiteIds = new List<string>();
                fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "sites.xml");
                if (!File.Exists(fileName))
                {
                    LogHelper.Log.Error("配置文件:" + fileName + "不存在,请检查");
                    return;
                }

                XElement config = XElement.Load(fileName);
                var sites = config.Elements("site");
                foreach (var s in sites)
                {
                    this.SiteIds.Add(s.Value);
                }

                LogHelper.Log.Info("配置文件:" + fileName + "加载成功\r\n场所编码:" + this.SiteIds.GetJsonString());
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("加载配置文件:" + fileName + "失败,请检查", ex);
            }
        }

        /// <summary>
        /// 加载输出数据格式文件
        /// </summary>
        private void LoadLayout()
        {
            string path = string.Empty;
            try
            {
                this.LayoutItems = new List<LayoutItem>();
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "dataLayout");
                var files = Directory.GetFiles(path, "*.xml");

                foreach (var file in files)
                {
                    try
                    {
                        LayoutItem layoutItem = new LayoutItem()
                        {
                            DataItems = new List<DataItem>()
                        };
                        XElement data = XElement.Load(file);
                        var items = data.Elements("item");
                        foreach (var item in items)
                        {
                            DataItem dataItem = new DataItem();
                            dataItem.Name = item.GetXmlAttribute("name").Value;
                            dataItem.Default = item.GetXmlAttr("default", "");
                            dataItem.Format = item.GetXmlAttr("format", "");
                            layoutItem.DataItems.Add(dataItem);
                        }
                        var key = Path.GetFileNameWithoutExtension(file);
                        layoutItem.Key = key;
                        layoutItem.Sql = data.GetXmlAttribute("sql").Value;
                        layoutItem.Orderby = data.GetXmlAttribute("orderby").Value;
                        this.LayoutItems.Add(layoutItem);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log.Error("解析文件" + file + "失败", ex);
                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.Log.Error("加载输出配置目录:" + path + "失败,请检查", ex);
            }
        }

    }
}
