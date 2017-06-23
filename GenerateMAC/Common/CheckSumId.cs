using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateMAC
{
    public static class CheckSumId
    {
        /// <summary>
        /// 生成ID  2015-12-26 李霓修改
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string GetId(DateTime dt, bool isMax = false, params string[] inputs)
        {
            TimeSpan ts = dt - new DateTime(2000, 1, 1);
            if (inputs != null && inputs.Length > 0)
            {
                return string.Format("{0}{1}", Convert.ToInt32(ts.TotalSeconds), CheckSum(inputs) & 262143 | 262144);
            }
            else
            {
                if (!isMax)
                    return string.Format("{0}{1}", Convert.ToInt32(ts.TotalSeconds), "000000");
                else
                    return string.Format("{0}{1}", Convert.ToInt32(ts.TotalSeconds), "999999");
            }
        }

        public static string GetId(DateTime dt, params string[] inputs)
        {
            TimeSpan ts = dt - new DateTime(2000, 1, 1);
            if (inputs != null && inputs.Length > 0)
            {
                return string.Format("{0}{1}", Convert.ToInt32(ts.TotalSeconds), CheckSum(inputs) & 262143 | 262144);
            }
            else
            {
                return string.Format("{0}{1}", Convert.ToInt32(ts.TotalSeconds), "000000");
            }
        }

        public static long GetIdInt64(DateTime dt, params string[] inputs)
        {
            TimeSpan ts = dt - new DateTime(2000, 1, 1);
            return Convert.ToInt64(string.Format("{0}{1}", Convert.ToInt32(ts.TotalSeconds), CheckSum(inputs) & 262143 | 262144));
        }

        public static long GetIdInt64()
        {
            return GetIdInt64(DateTime.Now, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// 计算校验和
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        private static int CheckSum(params string[] inputs)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            if (inputs.Length >= 2)
            {
                for (int i = 0, length = inputs.Length - 1; i < length; i++)
                {
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(inputs[i]);
                    md5.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                }

                byte[] bs = System.Text.Encoding.ASCII.GetBytes(inputs[inputs.Length - 1]);
                md5.TransformFinalBlock(bs, 0, bs.Length);
            }
            else if (inputs.Length == 1)
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(inputs[0]);
                md5.ComputeHash(bytes);
            }

            int result = 0;
            foreach (byte b in md5.Hash)
            {
                result = (result << 5) + result + b;
            }

            return result;
        }
        /// <summary>
        /// 解析ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DateTime Id2Time(string id)
        {

            string idstr = id;
            if (idstr.Length >= 9)
            {
                double seconds = Int64.Parse(idstr.Substring(0, 9));
                TimeSpan ts = TimeSpan.FromSeconds(seconds);
                return new DateTime(2000, 1, 1).Add(ts);
            }
            return (DateTime.Now);
        }
    }
}
