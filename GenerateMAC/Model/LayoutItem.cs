using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateMAC
{
    public class LayoutItem
    {
        /// <summary>
        /// 输出GZ文件的键，来源于文件名
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 执行的sql
        /// </summary>
        public string Sql { get; set; }
        /// <summary>
        /// 排序列
        /// </summary>
        public string Orderby { get; set; }
        /// <summary>
        /// 输出项的集合
        /// </summary>
        public List<DataItem> DataItems { get; set; }
    }
}
