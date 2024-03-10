using SmartLibrary.Models;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SmartLibrary.Helpers
{
    public class SQLiteHelper
    {
        private static readonly Dictionary<string, SQLiteHelper> DataBaceList = [];
        private readonly string DataSource = string.Empty;

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
        public async Task CreateDataBaseAsync()
        {
            string? path = Path.GetDirectoryName(DataSource);
            if ((!string.IsNullOrWhiteSpace(path)) && (!Directory.Exists(path)))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(DataSource))
            {
                SQLiteConnection.CreateFile(DataSource);
                StringBuilder sbr = new();
                sbr.AppendLine("CREATE TABLE IF NOT EXISTS 'main' (");
                sbr.AppendLine("'isbn' TEXT PRIMARY KEY NOT NULL,");
                sbr.AppendLine("'bookName' TEXT NOT NULL,");
                sbr.AppendLine("'author' TEXT NOT NULL,");
                sbr.AppendLine("'press' TEXT,");
                sbr.AppendLine("'pressDate' TEXT,");
                sbr.AppendLine("'pressPlace' TEXT,");
                sbr.AppendLine("'price' TEXT,");
                sbr.AppendLine("'clcName' TEXT,");
                sbr.AppendLine("'bookDesc' TEXT,");
                sbr.AppendLine("'pages' TEXT,");
                sbr.AppendLine("'words' TEXT,");
                sbr.AppendLine("'language' TEXT,");
                sbr.AppendLine("'picture' TEXT,");
                sbr.AppendLine("'shelfNumber' INTEGER NOT NULL,");
                sbr.AppendLine("'isBorrowed' INTEGER NOT NULL DEFAULT 0");
                sbr.AppendLine(");");
                await ExecuteNonQueryAsync(sbr.ToString());
            }
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
        /// <param name="sqlStr">Sql命令文本</param>
        /// <param name="p">参数数组</param>
        private static async void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string sqlStr, params SQLiteParameter[]? p)
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = sqlStr;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            if (p != null && p.Length >= 1)
            {
                foreach (SQLiteParameter parm in p)
                {
                    cmd.Parameters.AddWithValue(parm.ParameterName, parm.Value);
                }
            }
        }

        /// <summary>
        /// 查询，返回DataSet
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataset(string cmdText, params SQLiteParameter[]? data)
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
        public async Task<DataTable> ExecuteDataTableAsync(string cmdText, params SQLiteParameter[]? data)
        {
            var dt = new DataTable();
            using (SQLiteConnection connection = GetSQLiteConnection())
            {
                var command = new SQLiteCommand();
                PrepareCommand(command, connection, cmdText, data);
                DbDataReader reader = await command.ExecuteReaderAsync();
                dt.Load(reader);
            }
            return dt;
        }

        /// <summary>
        /// 返回一行数据
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">参数数组</param>
        /// <returns>DataRow</returns>
        public DataRow? ExecuteDataRow(string cmdText, params SQLiteParameter[]? data)
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
        public async Task<int> ExecuteNonQueryAsync(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var command = new SQLiteCommand();
            PrepareCommand(command, connection, cmdText, data);
            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 返回SqlDataReader对象
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>SQLiteDataReader</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(string cmdText, params SQLiteParameter[]? data)
        {
            var command = new SQLiteCommand();
            using SQLiteConnection connection = GetSQLiteConnection();
            PrepareCommand(command, connection, cmdText, data);
            DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader;
        }

        /// <summary>
        /// 返回结果集中的第一行第一列，忽略其他行或列
        /// </summary>
        /// <param name="cmdText">Sql命令文本</param>
        /// <param name="data">传入的参数</param>
        /// <returns>object</returns>
        public async Task<object?> ExecuteScalarAsync(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, cmdText, data);
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<DataTable> ExecutePagerAsync(int pageIndex, int pageSize)
        {
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT * FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            sbr.AppendLine((pageIndex - 1 * pageSize).ToString());
            return await ExecuteDataTableAsync(sbr.ToString());
        }

        public async Task<DataTable> ExecutePagerSimple(int pageIndex, int pageSize)
        {
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            sbr.AppendLine((pageIndex - 1 * pageSize).ToString());
            return await ExecuteDataTableAsync(sbr.ToString());
        }

        /// <summary>
        /// 重新组织数据库：VACUUM 将会从头重新组织数据库
        /// </summary>
        public async void ResetDataBassAsync()
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
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 查询总记录数
        /// </summary>
        public async Task<int> GetRecordCountAsync()
        {
            object? result = await ExecuteScalarAsync("SELECT count(shelfNumber) FROM main");
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// 删除书籍
        /// </summary>
        public async void DelBookAsync(string isbn)
        {
            await ExecuteNonQueryAsync($"DELETE FROM main WHERE isbn = {isbn}");
            string localFilePath = @".\pictures\" + isbn + ".jpg";
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }

        /// <summary>
        /// 书籍是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string isbn)
        {
            object? result = await ExecuteScalarAsync($"SELECT COUNT(*) FROM main WHERE isbn = {isbn}", null);
            if (result != null)
            {
                if ((long)result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 简单更新数据库
        /// </summary>
        public async void UpdateSimpleAsync(string isbn, string bookName, string? author, long shelfNumber, bool isBorrowed)
        {
            string sql = $"UPDATE main SET bookName = '{bookName}', author = '{author}', shelfNumber = @shelfNumber, isBorrowed = @isBorrowed WHERE isbn = '{isbn}'";
            SQLiteParameter[] parameters = [
                new SQLiteParameter("@shelfNumber", shelfNumber),
                new SQLiteParameter("@isBorrowed", isBorrowed)];
            await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// 更新数据库
        /// </summary>
        public async void UpdateAsync(BookInfo book)
        {
            string sqlStr = $"UPDATE main SET bookName = '{book.BookName}', author = '{book.Author}', press = '{book.Press}', pressDate = '{book.PressDate}', pressPlace = '{book.PressPlace}', price = '{book.Price}', clcName = '{book.ClcName}', bookDesc = '{book.BookDesc}', pages = '{book.Pages}', words = '{book.Words}', language = '{book.Language}', picture = '{book.Picture}', shelfNumber = @shelfNumber, isBorrowed = @isBorrowed WHERE isbn = '{book.Isbn}'";
            SQLiteParameter[] parameters = [
                        new SQLiteParameter("@shelfNumber", book.ShelfNumber),
                        new SQLiteParameter("@isBorrowed", book.IsBorrowed),
                    ];
            await ExecuteNonQueryAsync(sqlStr, parameters);
        }

        /// <summary>
        /// 合并数据库
        /// </summary>
        public async Task<int[]> MergeDatabaseAsync(string newDbPath)
        {
            int mergedCount = 0;
            int repeatedCount = 0;

            SQLiteConnection oldDbConnection = GetSQLiteConnection();
            if (oldDbConnection.State != ConnectionState.Open)
            {
                await oldDbConnection.OpenAsync();
            }

            SQLiteConnection newDbConnection = new()
            {
                ConnectionString = "Data Source=" + newDbPath
            };
            await newDbConnection.OpenAsync();

            SQLiteCommand selectCommand = new("SELECT * FROM main", newDbConnection);
            DbDataReader reader = await selectCommand.ExecuteReaderAsync();

            SQLiteCommand command = new();
            SQLiteTransaction transaction = oldDbConnection.BeginTransaction();

            while (reader.Read())
            {
                string isbn = reader.GetString(0);
                if (!await ExistsAsync(isbn))
                {
                    //string sqlStr = "INSERT INTO main VALUES (\"@isbn\",\"@bookName\",\"@author\",\"@press\",\"@pressDate\",\"@pressPlace\",@price,\"@clcName\",\"@bookDesc\",\"@pages\",\"@words\",\"@languag\",@picture\",@shelfNumber,@isBorrowed)";
                    string sqlStr = $"INSERT INTO main VALUES ('{isbn}','{reader.GetString(1)}','{reader.GetString(2)}','{reader.GetString(3)}','{reader.GetString(4)}','{reader.GetString(5)}','{reader.GetString(6)}','{reader.GetString(7)}','{reader.GetString(8)}','{reader.GetString(9)}','{reader.GetString(10)}','{reader.GetValue(12)}','{reader.GetValue(12)}',@shelfNumber,@isBorrowed)";

                    SQLiteParameter[] parameters = [
                        new SQLiteParameter("@shelfNumber", reader.GetInt64(13)),
                        new SQLiteParameter("@isBorrowed", reader.GetInt64(14)),
                    ];
                    PrepareCommand(command, oldDbConnection, sqlStr, parameters);
                    command.Transaction = transaction;
                    await command.ExecuteNonQueryAsync();
                    mergedCount++;
                }
                else
                {
                    repeatedCount++;
                }
            }
            transaction.Commit();
            command.Dispose();
            reader.Dispose();
            oldDbConnection.Dispose();
            newDbConnection.Dispose();
            return [mergedCount, repeatedCount];
        }

        public async Task<BookInfo> GetOneBookInfoAsync(string isbn)
        {
            BookInfo book = new();
            string sql = $"SELECT * FROM main WHERE isbn = {isbn}";

            SQLiteConnection DbConnection = GetSQLiteConnection();
            if (DbConnection.State != ConnectionState.Open)
            {
                await DbConnection.OpenAsync();
            }
            SQLiteCommand command = new(sql, DbConnection);
            DbDataReader reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                book.Isbn = isbn;
                book.BookName = reader.GetString(1);
                book.Author = reader.GetString(2);
                book.Press = reader.GetString(3);
                book.PressDate = reader.GetString(4);
                book.PressPlace = reader.GetString(5);
                book.Price = reader.GetString(6);
                book.ClcName = reader.GetString(7);
                book.BookDesc = reader.GetString(8);
                book.Pages = reader.GetString(9);
                book.Words = reader.GetString(10);
                book.Language = reader.GetString(11);
                book.Picture = reader.GetString(12);
                book.ShelfNumber = reader.GetInt64(13);
                book.IsBorrowed = Convert.ToBoolean(reader.GetInt64(14));
            }
            command.Dispose();
            reader.Dispose();
            DbConnection.Dispose();
            return book;
        }

        public async void AddBookAsync(BookInfo book)
        {
            string sqlStr = $"INSERT INTO main VALUES ('{book.Isbn}','{book.BookName}','{book.Author}','{book.Press}','{book.PressDate}','{book.PressPlace}','{book.Price}','{book.ClcName}','{book.BookDesc}','{book.Pages}','{book.Words}','{book.Language}','{book.Picture}',@shelfNumber,@isBorrowed)";
            SQLiteParameter[] parameters = [
                        new SQLiteParameter("@shelfNumber", book.ShelfNumber),
                        new SQLiteParameter("@isBorrowed", book.IsBorrowed),
                    ];
            await ExecuteNonQueryAsync(sqlStr, parameters);
        }

        //public List<BookInfo> GetBookInfos(object value)
        //{
        //    List<BookInfo> books = [];
        //    string sql = string.Empty ;
        //    SQLiteParameter[] parameters = [];
        //    if (value is string s)
        //    {
        //        sql = $"SELECT * FROM main WHERE bookName LIKE '{s}' OR author LIKE '{s}'";
        //    }
        //    else if (value is int i)
        //    {
        //        sql = $"SELECT * FROM main WHERE isbn LIKE @value OR shelfNumber like @value";
        //        parameters = [new SQLiteParameter("@value", i)];
        //    }
        //    using SQLiteDataReader reader = ExecuteReader(sql, parameters);
        //    while (reader.Read())
        //    {
        //        BookInfo book = new()
        //        {
        //            Isbn = reader.GetString(0),
        //            BookName = reader.GetString(1),
        //            Author = reader.GetString(2),
        //            Press = reader.GetString(3),
        //            PressDate = reader.GetString(4),
        //            PressPlace = reader.GetString(5),
        //            Price = reader.GetFloat(6),
        //            ClcName = reader.GetString(7),
        //            BookDesc = reader.GetString(8),
        //            Pages = reader.GetString(9),
        //            Words = reader.GetString(10),
        //            ShelfNumber = reader.GetInt64(11),
        //            IsBorrowed = Convert.ToBoolean(reader.GetInt64(12)),
        //            Picture = reader.GetString(13)
        //        };
        //        books.Add(book);
        //    }
        //    return books;
        //}

        //public ObservableCollection<string> GetBookInfos(string value)
        //{
        //    ObservableCollection<string> books = [];
        //    string sql = $"SELECT * FROM main WHERE bookName LIKE '{value}' OR author LIKE '{value}'";
        //    using SQLiteDataReader reader = ExecuteReader(sql);
        //    while (reader.Read())
        //    {
        //        BookInfo book = new()
        //        {
        //            Isbn = reader.GetString(0),
        //            BookName = reader.GetString(1),
        //            Author = reader.GetString(2),
        //            Press = reader.GetString(3),
        //            PressDate = reader.GetString(4),
        //            PressPlace = reader.GetString(5),
        //            Price = reader.GetFloat(6),
        //            ClcName = reader.GetString(7),
        //            BookDesc = reader.GetString(8),
        //            Pages = reader.GetString(9),
        //            Words = reader.GetString(10),
        //            ShelfNumber = reader.GetInt64(11),
        //            IsBorrowed = Convert.ToBoolean(reader.GetInt64(12)),
        //            Picture = reader.GetString(13)
        //        };
        //        books.Add(book);
        //    }
        //    return books;
        //}

        //public List<BookInfo> GetBookInfos(int value)
        //{
        //    List<BookInfo> books = [];
        //    string sql = $"SELECT * FROM main WHERE isbn LIKE @value OR shelfNumber like @value";
        //    SQLiteParameter[] parameters = [new SQLiteParameter("@value", value)];
        //    using SQLiteDataReader reader = ExecuteReader(sql, parameters);
        //    while (reader.Read())
        //    {
        //        BookInfo book = new()
        //        {
        //            Isbn = reader.GetString(0),
        //            BookName = reader.GetString(1),
        //            Author = reader.GetString(2),
        //            Press = reader.GetString(3),
        //            PressDate = reader.GetString(4),
        //            PressPlace = reader.GetString(5),
        //            Price = reader.GetFloat(6),
        //            ClcName = reader.GetString(7),
        //            BookDesc = reader.GetString(8),
        //            Pages = reader.GetString(9),
        //            Words = reader.GetString(10),
        //            ShelfNumber = reader.GetInt64(11),
        //            IsBorrowed = Convert.ToBoolean(reader.GetInt64(12)),
        //            Picture = reader.GetString(13)
        //        };
        //        books.Add(book);
        //    }
        //    return books;
        //}

        public async Task<DataTable> AutoSuggestByStringAsync(string str)
        {
            string sql = $"SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main WHERE bookName LIKE '%{str}%' OR author LIKE '%{str}%'";
            return await ExecuteDataTableAsync(sql);
        }

        public async Task<DataTable> AutoSuggestByNumAsync(int num)
        {
            string sql = $"SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main WHERE isbn = {num} OR shelfNumber = {num}";
            return await ExecuteDataTableAsync(sql);
        }

        public async void BorrowBookAsync(string isbn)
        {
            string sql = $"UPDATE main SET isBorrowed = 1 WHERE isbn = '{isbn}'";
            await ExecuteNonQueryAsync(sql);
        }

        public async void ReturnBookAsync(string isbn)
        {
            string sql = $"UPDATE main SET isBorrowed = 0 WHERE isbn = '{isbn}'";
            await ExecuteNonQueryAsync(sql);
        }
    }

}
