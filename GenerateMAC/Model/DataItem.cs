using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateMAC
{
    /// <summary>
    /// 数据列项的集合
    /// </summary>
    public class DataItem
    {
        /// <summary>
        /// gz文件中列的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string Default { get; set; }
        /// <summary>
        /// 格式化的列
        /// </summary>
        public string Format { get; set; }
    }

}
