using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using HZ.Common;
using System.IO;

namespace GenerateMAC
{
    class StartUp
    {
        /// <summary>
        /// 程序是否运行中
        /// </summary>
        private bool IsRun = false;
        /// <summary>
        /// 轨迹信息，为了避免同一小时内在多个场所存在数据
        /// </summary>
        private List<TraceLog> TraceLogs = new List<TraceLog>();

        #region 公共方法
        public void Start()
        {
            try
            {
                IsRun = true;
                new Thread(new ThreadStart(Action)).Start();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("服务器启动失败", ex);
            }
        }

        public void Stop()
        {
            IsRun = false;
        }

        /// <summary>
        /// 暂停指定秒
        /// </summary>
        /// <param name="second"></param>
        private void Wait(int second)
        {
            int i = 0;
            while (IsRun)
            {
                if (i > second)
                {
                    break;
                }
                i++;
                Thread.Sleep(1000);
            }
        }
        #endregion

        private void Action()
        {
            while (IsRun)
            {
                try
                {
                    //先生成数据,再写入文件
                    foreach (var item in SysConfig.Install.LayoutItems)
                    {
                        DataTable dt = GetData(item);
                        WriteFile(dt, item.Key);
                    }

                    Wait(600);
                }
                catch (Exception ex)
                {
                    LogHelper.Log.Error("服务运行发生无法识别的异常", ex);
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        private DataTable GetData(LayoutItem layoutItem)
        {
            try
            {
                DataTable dt = new DataTable();

                #region 配置列
                foreach (var item in layoutItem.DataItems)
                {
                    dt.Columns.Add(item.Name);
                }
                #endregion

                #region 组装数据
                foreach (var siteId in SysConfig.Install.SiteIds)
                {
                    int dataLenth = new Random().Next(200, 400);//200-400条数据
                    int allCount = GetTotalCount(layoutItem, siteId); //数据总条数
                    string[] indexs = new string[dataLenth];

                    if (allCount == 0)
                        continue;

                    for (int i = 0; i < indexs.Length; i++)
                    {
                        indexs[i] = new Random(Guid.NewGuid().GetHashCode()).Next(1, allCount).ToString();
                    }

                    DataTable table = GetDbData(siteId, indexs, layoutItem);
                    foreach (DataRow row in table.Rows)
                    {
                        string[] data = new string[layoutItem.DataItems.Count];
                        for (int i = 0; i < layoutItem.DataItems.Count; i++)
                        {
                            var dataItem = layoutItem.DataItems[i];
                            if (string.IsNullOrEmpty(dataItem.Format))
                            {
                                data[i] = dataItem.Default;
                            }
                            else
                            {
                                if (dataItem.Format.IndexOf("@@") > -1)
                                {
                                    var tenMin = DateTime.Now.AddSeconds(new Random(Guid.NewGuid().GetHashCode()).Next(0, 600) * -1);

                                    //类似于时间等的替换
                                    data[i] = dataItem.Format
                                        .Replace("@@currTime", Helper.GetTimeToSeconds() + "")
                                        .Replace("@@tenMin", Helper.GetTimeToSeconds(tenMin) + "")
                                        .Replace("@@id",CheckSumId.GetIdInt64()+"");
                                }
                                else
                                {
                                    if (table.Columns.Contains(dataItem.Format))
                                    {
                                        data[i] = row[dataItem.Format].ToString();
                                    }
                                    else
                                    {
                                        LogHelper.Log.Error("key:" + layoutItem.Key + "获取的数据中不包含列[" + dataItem.Format + "],只能返回默认值");
                                        data[i] = dataItem.Default;
                                    }
                                }
                            }
                        }

                        dt.Rows.Add(data);
                    }
                }
                #endregion

                //清理除去1小时内在其它地方采集到的数据
                var clearTrace = this.TraceLogs.Where(f => f.DetectTime < Helper.GetTimeToSeconds(DateTime.Now.AddHours(-1)));
                foreach (var item in clearTrace)
                {
                    TraceLogs.Remove(item);
                }
                if (layoutItem.Key == "HotspotInfoLog")
                {
                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                    {
                        var siteId = dt.Rows[i]["site_id"].ToString();
                        var mac = dt.Rows[i]["hotspot_mac"].ToString();
                        var detect_time = dt.Rows[i]["detect_time"].GetInt32();

                        if (TraceLogs.Count(f => f.MAC == mac && f.SiteId != siteId) > 0)
                        {
                            LogHelper.Log.DebugFormat("MAC:{0},siteId:{1}已经出现过,直接删除", mac, siteId);
                            dt.Rows.RemoveAt(i);
                        }
                        TraceLogs.Add(new TraceLog()
                        {
                            MAC=mac,
                            SiteId=siteId,
                            DetectTime=detect_time
                        });
                    }
                }
                else if (layoutItem.Key == "WifiTerminalInfoLog")
                {
                    for (int i = dt.Rows.Count - 1; i >= 0; i--)
                    {
                        var siteId = dt.Rows[i]["site_id"].ToString();
                        var mac = dt.Rows[i]["terminal_mac"].ToString();
                        var detect_time = dt.Rows[i]["detect_time"].GetInt32();

                        if (TraceLogs.Count(f => f.MAC == mac && f.SiteId != siteId) > 0)
                        {
                            LogHelper.Log.DebugFormat("MAC:{0},siteId:{1}已经出现过,直接删除", mac, siteId);
                            dt.Rows.RemoveAt(i);
                        }
                        TraceLogs.Add(new TraceLog()
                        {
                            MAC = mac,
                            SiteId = siteId,
                            DetectTime = detect_time
                        });
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("获取数据组装DataTable出错", ex);
                return new DataTable();
            }

        }

        /// <summary>
        /// 写入文件
        /// 文件名格式:20170621150450_WifiTerminalInfoLog_000001_000061.gz
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="key"></param>
        private void WriteFile(DataTable dt, string key)
        {
            try
            {
                #region 创建目录
                if (!Directory.Exists(SysConfig.Install.OutDir))
                    Directory.CreateDirectory(SysConfig.Install.OutDir);
                #endregion

                if (!dt.IsNullOrEmpty())
                {
                    string time = DateTime.Now.ToString("yyyyMMddHHmmss");
                    int index = 1;
                    DataTable tempTable = dt.Clone();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        tempTable.Rows.Add(dt.Rows[i].ItemArray);
                        if (tempTable.Rows.Count >= 5000)
                        {
                            string fileName = string.Format("{0}_{1}_{2}_{3}.gz", time, key, index.ToString("d6"), tempTable.Rows.Count.ToString("d6"));
                            WriteDataFile(tempTable, Path.Combine(SysConfig.Install.OutDir, fileName));
                            //清空
                            tempTable.Clear();
                            index++;
                        }
                    }

                    if (tempTable.Rows.Count > 0)
                    {
                        string fileName = string.Format("{0}_{1}_{2}_{3}.gz", time, key, index.ToString("d6"), tempTable.Rows.Count.ToString("d6"));
                        WriteDataFile(tempTable, Path.Combine(SysConfig.Install.OutDir, fileName));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("将数据写入文件出错", ex);
            }
        }

        /// <summary>
        /// 将DataTable写入
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="url"></param>
        private void WriteDataFile(DataTable dt, string url)
        {
            string content = StringManager.DataTable2String(dt);
            var bs = GZipManager.WriteGzip(content);
            File.WriteAllBytes(url, bs);
        }

        /// <summary>
        /// 获取待插入的DataTable数据
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="indexs"></param>
        /// <param name="layoutItem"></param>
        /// <returns></returns>
        private DataTable GetDbData(string siteId, string[] indexs, LayoutItem layoutItem)
        {
            string exeSql = string.Empty;
            try
            {
                exeSql = @"select * from(
select *,ROW_NUMBER() over(order by " + layoutItem.Orderby + ") num_ from (select * from ( " + layoutItem.Sql + " ) ss where site_id='" + siteId + "') sm) m where num_ in(" + string.Join(",", indexs) + ")";
                return SqlDBHelper.GetTable(exeSql, CommandType.Text, null);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("数据库访问出错:" + exeSql, ex);
                return new DataTable();
            }
        }

        /// <summary>
        /// 总条数
        /// </summary>
        /// <param name="layoutItem"></param>
        /// <param name="site_id"></param>
        /// <returns></returns>
        private int GetTotalCount(LayoutItem layoutItem, string site_id)
        {
            string sql = string.Empty;
            try
            {
                sql = string.Format("select count(1) from (select * from ({0}) ss where site_id='{1}') sm", layoutItem.Sql, site_id);
                return SqlDBHelper.ExecuteScalar(sql, CommandType.Text, null).GetInt32();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("数据库访问出错:" + sql, ex);
                return 0;
            }
        }

    }
}
