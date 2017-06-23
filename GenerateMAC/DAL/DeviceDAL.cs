using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using HZ.Common;

namespace GenerateMAC.DAL
{
    /// <summary>
    /// 数据访问层
    /// </summary>
    public class DeviceDAL
    {
        /// <summary>
        /// 根据场所编码查询设备信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public DataTable GetDeviceInfo(string siteId)
        {
            try
            {
                string sql = "select * from wifi.Device_Info where Site_Id='" + siteId + "'";
                return SqlDBHelper.GetTable(sql, CommandType.Text, null);
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("数据库访问失败", ex);
                return new DataTable();
            }
        }

        /// <summary>
        /// 获得终端MAC总数量
        /// </summary>
        /// <returns></returns>
        public int GetWifiTerminalInfoLogCount()
        {
            try
            {
                string sql = "select COUNT(1) from wifi.Terminal_mac";
                return SqlDBHelper.ExecuteScalar(sql, CommandType.Text, null).GetInt32();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("数据库访问失败", ex);
                return 0;
            }
        }

        /// <summary>
        /// 获取热点MAC总数量
        /// </summary>
        /// <returns></returns>
        public int GetHotspotInfoLog()
        {
            try
            {
                string sql = "select COUNT(1) from wifi.Hotspot";
                return SqlDBHelper.ExecuteScalar(sql, CommandType.Text, null).GetInt32();
            }
            catch (Exception ex)
            {
                LogHelper.Log.Error("数据库访问失败", ex);
                return 0;
            }
        }
    }
}
