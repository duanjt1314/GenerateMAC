using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace GenerateMAC
{
    public class SqlDBHelper
    {
        /// <summary>
        /// 获得数据库连接字符串
        /// </summary>
        public static String connStr = string.Empty;

        /// <summary>
        /// 提取Command公共的方法
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="comm">Command对象</param>
        /// <param name="tran">事务</param>
        /// <param name="cmdText">数据库执行语句</param>
        /// <param name="cmdType">指定执行语句的类型（sql语句或存储过程）</param>
        /// <param name="parm">参数</param>
        private static void PreparedExecute(SqlConnection conn, SqlCommand comm, SqlTransaction tran, string cmdText, CommandType cmdType, SqlParameter[] parm)
        {
            //打开连接
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            //为Command指定参数
            comm.Connection = conn;
            comm.CommandText = cmdText;
            comm.CommandType = cmdType;

            //指定事务
            if (tran != null)
            {
                comm.Transaction = tran;
            }

            //指定参数
            if (parm != null)
            {
                comm.Parameters.AddRange(parm);
            }
        }

        /// <summary>
        /// 查询数据库的数据
        /// </summary>
        /// <param name="cmdText">数据库执行语句</param>
        /// <param name="cmdType">指定执行语句的类型（sql语句或存储过程）</param>
        /// <param name="parm">参数</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string cmdText, CommandType cmdType, params SqlParameter[] parm)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand comm = new SqlCommand();

            try
            {
                PreparedExecute(conn, comm, null, cmdText, cmdType, parm);
                SqlDataReader sr = comm.ExecuteReader();
                return sr;
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// 执行更新语句（无事务）
        /// </summary>
        /// <param name="cmdText">数据库执行语句</param>
        /// <param name="cmdType">指定执行语句的类型（sql语句或存储过程）</param>
        /// <param name="parm">参数</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string cmdText, CommandType cmdType, params SqlParameter[] parm)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand comm = new SqlCommand();
                PreparedExecute(conn, comm, null, cmdText, cmdType, parm);
                int num = comm.ExecuteNonQuery();
                return num;
            }
        }

        /// <summary>
        /// 根据传入的SQL语句或存储过程返回第一行第一列的数据
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="cmdType"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string cmdText, CommandType cmdType, params SqlParameter[] parm)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand comm = new SqlCommand();
                PreparedExecute(conn, comm, null, cmdText, cmdType, parm);
                object obj = comm.ExecuteScalar();
                return obj;
            }
        }

        /// <summary>
        /// 执行更新语句（有事务）
        /// </summary>
        /// <param name="tran">事务</param>
        /// <param name="cmdText">数据库执行语句</param>
        /// <param name="cmdType">指定执行语句的类型（sql语句或存储过程）</param>
        /// <param name="parm">参数</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(SqlTransaction tran, string cmdText, CommandType cmdType, params SqlParameter[] parm)
        {
            SqlCommand comm = new SqlCommand();
            PreparedExecute(tran.Connection, comm, tran, cmdText, cmdType, parm);
            int num = comm.ExecuteNonQuery();
            return num;
        }



        /// <summary>
        /// 根据传入的SQL语句或存储过程返回DataTable
        /// </summary>
        /// <param name="cmdText">Sql语句或存储过程</param>
        /// <param name="cmdType">指定是sql语句还是存储过程</param>
        /// <param name="parm">参数</param>
        /// <returns></returns>
        public static DataTable GetTable(string cmdText, CommandType cmdType, params SqlParameter[] parm)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                DataSet ds = new DataSet();
                conn.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(cmdText, conn);
                adapter.SelectCommand.CommandType = cmdType;
                if (parm != null)
                {
                    adapter.SelectCommand.Parameters.AddRange(parm);
                }

                adapter.Fill(ds);
                return ds.Tables[0];
            }
        }

        /// <summary>
        /// 分页查询数据并返回DataTable的公共方法
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="field">需要查询的字段</param>
        /// <param name="pageSize">每页显示数据的条数</param>
        /// <param name="start">排除的数据量</param>
        /// <param name="sqlWhere">where条件</param>
        /// <param name="sortName">排序名称</param>
        /// <param name="sortOrder">排序方式</param>
        /// <returns></returns>
        public static DataTable GetTable(String tableName, String field, int pageSize, int start, String sqlWhere, String sortName, String sortOrder, out Int32 total)
        {
            String sql = String.Format("select {0} from(select *,ROW_NUMBER() over(order by {1} {2}) rowid from {3} where {4}) a where rowid>{5} and rowid<={6}",
                field, sortName, sortOrder, tableName, sqlWhere, start, pageSize + start);
            DataTable dt = GetTable(sql, CommandType.Text, null);

            sql = "select count(1) from " + tableName + " where " + sqlWhere;
            total = Convert.ToInt32(SqlDBHelper.ExecuteScalar(sql, CommandType.Text, null));

            return dt;
        }

        /// <summary>
        /// 同时执行多条sql语句的增删改操作
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool ExecuteSql(List<SqlHashTable> list)
        {
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                tran = conn.BeginTransaction();
                foreach (SqlHashTable hash in list)
                {
                    SqlCommand comm = new SqlCommand(hash.Sql, conn);
                    if (hash.Par != null)
                    {
                        comm.Parameters.AddRange(hash.Par);
                    }
                    comm.Transaction = tran;
                    comm.ExecuteNonQuery();
                }
                tran.Commit();
                conn.Close();
                return true;
            }
            catch
            {
                if (tran != null)
                    tran.Rollback();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
                return false;
            }
        }

        /// <summary>
        /// 同时执行多条sql语句的增删改操作
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool ExecuteSql(List<String> list)
        {
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                tran = conn.BeginTransaction();
                foreach (String sql in list)
                {
                    SqlCommand comm = new SqlCommand(sql, conn);

                    comm.Transaction = tran;
                    comm.ExecuteNonQuery();
                }
                tran.Commit();
                conn.Close();
                return true;
            }
            catch
            {
                if (tran != null)
                    tran.Rollback();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
                return false;
            }
        }

    }
    public class SqlHashTable
    {
        public SqlHashTable()
        {

        }

        public SqlHashTable(string sql, SqlParameter[] par)
        {
            this.sql = sql;
            this.par = par;
        }

        private string sql;
        /// <summary>
        /// SQL语句
        /// </summary>
        public string Sql
        {
            get { return sql; }
            set { sql = value; }
        }
        private SqlParameter[] par;
        /// <summary>
        /// SQL语句的参数数组
        /// </summary>
        public SqlParameter[] Par
        {
            get { return par; }
            set { par = value; }
        }
    }
}

