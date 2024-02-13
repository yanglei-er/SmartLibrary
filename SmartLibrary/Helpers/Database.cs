using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SmartLibrary.Helpers
{
    public class SQLiteHelper
    {
        private static readonly Dictionary<string, SQLiteHelper> DataBaceList = [];
        private readonly string DataSource = string.Empty;

        public delegate void ExecuteCompletedEventHandler(DataTable datatable);
        public event ExecuteCompletedEventHandler ExecutePagerCompleted = delegate { };
        public event ExecuteCompletedEventHandler ExecuteDataTableCompleted = delegate { };

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filename">数据库文件名</param>
        private SQLiteHelper(string filename)
        {
            DataSource = @".\database\" + filename;
        }

        public static SQLiteHelper GetDatabase(string filename)
        {
            if (DataBaceList.TryGetValue(filename, out SQLiteHelper? value))
            {
                return value;
            }
            else
            {
                SQLiteHelper db = new(filename);
                if (File.Exists(@".\database\" + filename))
                {
                    DataBaceList.Add(filename, db);
                }
                return db;
            }
        }

        /// <summary>
        /// 数据库是否连接
        /// </summary>
        public static bool IsDatabaseConnected(string filename)
        {
            if (DataBaceList.ContainsKey(filename))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 创建数据库，如果数据库文件存在则忽略此操作
        /// </summary>
        public void CreateDataBase()
        {
            string? path = Path.GetDirectoryName(DataSource);
            if ((!string.IsNullOrWhiteSpace(path)) && (!Directory.Exists(path))) Directory.CreateDirectory(path);
            if (!File.Exists(DataSource)) SQLiteConnection.CreateFile(DataSource);
        }

        /// <summary>
        /// 获得连接对象
        /// </summary>
        /// <returns>SQLiteConnection</returns>       
        public SQLiteConnection GetSQLiteConnection()
        {
            string connStr = string.Format("Data Source={0}", DataSource);
            var con = new SQLiteConnection(connStr);
            return con;
        }

        /// <summary>
        /// 准备操作命令参数
        /// </summary>
        /// <param name="cmd">SQLiteCommand</param>
        /// <param name="conn">SQLiteConnection</param>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string cmdText, Dictionary<String, String>? data)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            if (data != null && data.Count >= 1)
            {
                foreach (KeyValuePair<String, String> val in data)
                {
                    cmd.Parameters.AddWithValue(val.Key, val.Value);
                }
            }
        }

        /// <summary>
        /// 查询，返回DataSet
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataset(string cmdText, Dictionary<string, string>? data)
        {
            var ds = new DataSet();
            using (SQLiteConnection connection = GetSQLiteConnection())
            {
                var command = new SQLiteCommand();
                PrepareCommand(command, connection, cmdText, data);
                var da = new SQLiteDataAdapter(command);
                da.Fill(ds);
            }
            return ds;
        }

        /// <summary>
        /// 查询，返回DataTable
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteDataTable(string cmdText, Dictionary<string, string>? data)
        {
            var dt = new DataTable();
            using (SQLiteConnection connection = GetSQLiteConnection())
            {
                var command = new SQLiteCommand();
                PrepareCommand(command, connection, cmdText, data);
                SQLiteDataReader reader = command.ExecuteReader();
                dt.Load(reader);
            }
            return dt;
        }

        private void ExecuteDataTableAction(string cmdText, Dictionary<string, string>? data)
        {
            ExecuteDataTableCompleted.Invoke(ExecuteDataTable(cmdText, data));
        }

        public void ExecuteDataTableAsync(string cmdText, Dictionary<string, string>? data)
        {
            Task.Run(() => ExecuteDataTableAction(cmdText, data));
        }

        /// <summary>
        /// 返回一行数据
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataRow</returns>
        public DataRow? ExecuteDataRow(string cmdText, Dictionary<string, string>? data)
        {
            DataSet ds = ExecuteDataset(cmdText, data);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                return ds.Tables[0].Rows[0];
            return null;
        }

        /// <summary>
        /// 执行数据库操作
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string cmdText, Dictionary<string, string>? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var command = new SQLiteCommand();
            PrepareCommand(command, connection, cmdText, data);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 返回SqlDataReader对象
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>SQLiteDataReader</returns>
        public SQLiteDataReader ExecuteReader(string cmdText, Dictionary<string, string>? data)
        {
            var command = new SQLiteCommand();
            SQLiteConnection connection = GetSQLiteConnection();
            try
            {
                PrepareCommand(command, connection, cmdText, data);
                SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch
            {
                connection.Close();
                command.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 返回结果集中的第一行第一列，忽略其他行或列
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>object</returns>
        public object ExecuteScalar(string cmdText, Dictionary<string, string>? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, cmdText, data);
            return cmd.ExecuteScalar();
        }

        private void ExecutePagerAction(int pageIndex, int pageSize)
        {
            //pageIndex 页码
            //pageSize 每页条数
            //OFFSET 代表从第几条记录的后面开始查询
            //LIMIT 查询多少条结果
            //SELECT* FROM 表名 LIMIT pageSize OFFSET(pageIndex* pageSize);
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT * FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            sbr.AppendLine((pageIndex * pageSize).ToString());
            ExecutePagerCompleted.Invoke(ExecuteDataTable(sbr.ToString(), null));
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageIndex">页牵引</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>DataSet</returns>
        public void ExecutePager(int pageIndex, int pageSize)
        {
            Task.Run(() => ExecutePagerAction(pageIndex - 1, pageSize));
        }

        /// <summary>
        /// 重新组织数据库：VACUUM 将会从头重新组织数据库
        /// </summary>
        public void ResetDataBass()
        {
            using SQLiteConnection conn = GetSQLiteConnection();
            var cmd = new SQLiteCommand();

            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = "vacuum";
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 查询总记录数
        /// </summary>
        public int GetRecordCount()
        {
            string? command = ExecuteScalar("SELECT count(shelfNumber) FROM main", null).ToString();
            if (command != null) { return int.Parse(command); }
            else { return 0; }
        }

        /// <summary>
        /// 更新数据库
        /// </summary>
        public void UpdateDatabase(DataTable table)
        {
            SQLiteConnection conn = GetSQLiteConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            SQLiteDataAdapter SD = new()
            {
                SelectCommand = new SQLiteCommand("select * from main", conn)
            };
            SQLiteCommandBuilder SC = new(SD);
            SD.UpdateCommand = SC.GetUpdateCommand();
            SD.Update(table); //更新数据源与UI
            conn.Close(); //关闭数据库连接
        }

        /// <summary>
        /// 删除书籍
        /// </summary>
        public void DelBook(string isbn)
        {
            StringBuilder sbr = new();
            sbr.Append("DELETE FROM main WHERE isbn =  ");
            sbr.Append(isbn);
            ExecuteNonQuery(sbr.ToString(), null);
        }

        /// <summary>
        /// 书籍是否存在
        /// </summary>
        public bool Exists(string isbn)
        {
            if (ExecuteReader($"SELECT * FROM main WHERE isbn = {isbn}", null).HasRows)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 合并数据库
        /// </summary>
        public int[] MergeDatabase(string newDbPath)
        {
            int mergedCount = 0;
            int repeatedCount = 0;

            SQLiteConnection newDbConnection = new()
            {
                ConnectionString = "Data Source=" + newDbPath
            };
            newDbConnection.Open();

            SQLiteCommand selectCommand = new("SELECT * FROM main", newDbConnection);
            SQLiteDataReader reader = selectCommand.ExecuteReader();

            while (reader.Read())
            {
                string isbn = reader.GetString(0);
                if (!Exists(isbn))
                {
                    //string cmdText = "INSERT INTO main VALUES (@isbn,@bookName,@author,@press,@pressDate,@pressPlace,@price,@clcName,@bookDesc,@pages,@words,@shelfNumber,@isBorrowed,@picture)";
                    //Dictionary<string, string> data = new()
                    //{
                    //    { "@isbn", isbn },
                    //    { "@bookName", reader.GetString(1) },
                    //    { "@author", reader.GetString(2) },
                    //    { "@press", reader.GetString(3) },
                    //    { "@pressDate", reader.GetString(4) },
                    //    { "@pressPlace", reader.GetString(5) },
                    //    { "@price", reader.GetValue(6).ToString() },
                    //    { "@clcName", reader.GetString(7) },
                    //    { "@bookDesc", reader.GetString(8) },
                    //    { "@pages", reader.GetString(9) },
                    //    { "@words", reader.GetString(10) },
                    //    { "@shelfNumber", reader.GetInt64(11).ToString() },
                    //    { "@isBorrowed", reader.GetInt64(12).ToString() },
                    //    { "@picture", reader.GetValue(13).ToString() }
                    //};
                    //ExecuteNonQuery(cmdText, data);
                    mergedCount++;
                }
                else
                {
                    repeatedCount++;
                }
            }
            reader.Dispose();
            newDbConnection.Dispose();
            return [mergedCount, repeatedCount];
        }
    }
}
