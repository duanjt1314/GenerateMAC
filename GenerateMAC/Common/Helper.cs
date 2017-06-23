using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateMAC
{
    class Helper
    {
        /// <summary>
        /// 将日期转换为时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimeToSeconds(DateTime time)
        {
            try
            {
                TimeSpan ts = (TimeSpan)(time - new DateTime(1970, 1, 1, 8, 0, 0));
                return Int64.Parse(Math.Ceiling(ts.TotalSeconds) + "");
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 将当前时间转换为时间戳(从数据库获取)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long GetTimeToSeconds()
        {
            try
            {
                TimeSpan ts = (TimeSpan)(DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0));
                return Int64.Parse(Math.Ceiling(ts.TotalSeconds) + "");
            }
            catch
            {
                return 0;
            }
        }
    }
}
