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
                sbr.AppendLine("'keyword' TEXT,");
                sbr.AppendLine("'language' TEXT,");
                sbr.AppendLine("'picture' TEXT,");
                sbr.AppendLine("'shelfNumber' INTEGER NOT NULL,");
                sbr.AppendLine("'isBorrowed' INTEGER NOT NULL DEFAULT 0");
                sbr.AppendLine(");");
                await ExecuteNonQueryAsync(sbr.ToString());
            }
        }

        public SQLiteConnection GetSQLiteConnection()
        {
            string connStr = string.Format("Data Source={0}", DataSource);
            var con = new SQLiteConnection(connStr);
            return con;
        }

        private static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string sqlStr, params SQLiteParameter[]? p)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
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

        private static async Task PrepareCommandAsync(SQLiteCommand cmd, SQLiteConnection conn, string sqlStr, params SQLiteParameter[]? p)
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

        public DataTable ExecuteDataTable(string cmdText, params SQLiteParameter[]? data)
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

        public async ValueTask<DataTable> ExecuteDataTableAsync(string cmdText, params SQLiteParameter[]? data)
        {
            var dt = new DataTable();
            using (SQLiteConnection connection = GetSQLiteConnection())
            {
                var command = new SQLiteCommand();
                await PrepareCommandAsync(command, connection, cmdText, data);
                DbDataReader reader = await command.ExecuteReaderAsync();
                dt.Load(reader);
            }
            return dt;
        }

        public int ExecuteNonQuery(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var command = new SQLiteCommand();
            PrepareCommand(command, connection, cmdText, data);
            return command.ExecuteNonQuery();
        }

        public async ValueTask<int> ExecuteNonQueryAsync(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            SQLiteCommand command = new();
            await PrepareCommandAsync(command, connection, cmdText, data);
            return await command.ExecuteNonQueryAsync();
        }

        public SQLiteDataReader ExecuteReader(string cmdText, params SQLiteParameter[]? data)
        {
            var command = new SQLiteCommand();
            using SQLiteConnection connection = GetSQLiteConnection();
            PrepareCommand(command, connection, cmdText, data);
            SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        public async ValueTask<DbDataReader> ExecuteReaderAsync(string cmdText, params SQLiteParameter[]? data)
        {
            var command = new SQLiteCommand();
            using SQLiteConnection connection = GetSQLiteConnection();
            await PrepareCommandAsync(command, connection, cmdText, data);
            DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader;
        }

        public object ExecuteScalar(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, cmdText, data);
            return cmd.ExecuteScalar();
        }

        public async ValueTask<object?> ExecuteScalarAsync(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var cmd = new SQLiteCommand();
            await PrepareCommandAsync(cmd, connection, cmdText, data);
            return await cmd.ExecuteScalarAsync();
        }

        public async ValueTask<DataTable> ExecutePagerAsync(int pageIndex, int pageSize)
        {
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT * FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            sbr.AppendLine((pageIndex - 1 * pageSize).ToString());
            return await ExecuteDataTableAsync(sbr.ToString());
        }

        public async ValueTask<DataTable> ExecutePagerSimpleAsync(int pageIndex, int pageSize)
        {
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            sbr.AppendLine((pageIndex - 1 * pageSize).ToString());
            return await ExecuteDataTableAsync(sbr.ToString());
        }

        public async void ResetDataBassAsync()
        {
            using SQLiteConnection conn = GetSQLiteConnection();
            var cmd = new SQLiteCommand();

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = "vacuum";
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            await cmd.ExecuteNonQueryAsync();
        }

        public async ValueTask<int> GetRecordCountAsync()
        {
            object? result = await ExecuteScalarAsync("SELECT count(shelfNumber) FROM main");
            return Convert.ToInt32(result);
        }

        public async void DelBookAsync(string isbn)
        {
            await ExecuteNonQueryAsync($"DELETE FROM main WHERE isbn = {isbn}");
            string localFilePath = @".\pictures\" + isbn + ".jpg";
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }

        public async ValueTask<bool> ExistsAsync(string isbn)
        {
            object? result = await ExecuteScalarAsync($"SELECT COUNT(*) FROM main WHERE isbn = {isbn}", null);
            if (Convert.ToInt32(result) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void UpdateSimpleAsync(string isbn, string bookName, string? author, long shelfNumber, bool isBorrowed)
        {
            string sql = $"UPDATE main SET bookName = '{bookName}', author = '{author}', shelfNumber = @shelfNumber, isBorrowed = @isBorrowed WHERE isbn = '{isbn}'";
            SQLiteParameter[] parameters = [
                new SQLiteParameter("@shelfNumber", shelfNumber),
                new SQLiteParameter("@isBorrowed", isBorrowed)];
            await ExecuteNonQueryAsync(sql, parameters);
        }

        public async void UpdateAsync(BookInfo book)
        {
            string sqlStr = $"UPDATE main SET bookName = '{book.BookName}', author = '{book.Author}', press = '{book.Press}', pressDate = '{book.PressDate}', pressPlace = '{book.PressPlace}', price = '{book.Price}', clcName = '{book.ClcName}', bookDesc = '{book.BookDesc}', pages = '{book.Pages}', keyword = '{book.Keyword}', language = '{book.Language}', picture = '{book.Picture}', shelfNumber = @shelfNumber, isBorrowed = @isBorrowed WHERE isbn = '{book.Isbn}'";
            SQLiteParameter[] parameters = [
                        new SQLiteParameter("@shelfNumber", book.ShelfNumber),
                        new SQLiteParameter("@isBorrowed", book.IsBorrowed),
                    ];
            await ExecuteNonQueryAsync(sqlStr, parameters);
        }

        public async ValueTask<int[]> MergeDatabaseAsync(string newDbPath)
        {
            int mergedCount = 0;
            int repeatedCount = 0;

            using SQLiteConnection oldDbConnection = GetSQLiteConnection();
            if (oldDbConnection.State != ConnectionState.Open)
            {
                await oldDbConnection.OpenAsync();
            }

            using SQLiteConnection newDbConnection = new()
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
                    string sqlStr = $"INSERT INTO main VALUES ('{isbn}','{reader.GetString(1)}','{reader.GetString(2)}','{reader.GetString(3)}','{reader.GetString(4)}','{reader.GetString(5)}','{reader.GetString(6)}','{reader.GetString(7)}','{reader.GetString(8)}','{reader.GetString(9)}','{reader.GetString(10)}','{reader.GetValue(11)}','{reader.GetValue(12)}',@shelfNumber,@isBorrowed)";

                    SQLiteParameter[] parameters = [
                        new SQLiteParameter("@shelfNumber", reader.GetInt64(13)),
                        new SQLiteParameter("@isBorrowed", reader.GetInt64(14)),
                    ];
                    await PrepareCommandAsync(command, oldDbConnection, sqlStr, parameters);
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
            return [mergedCount, repeatedCount];
        }

        public async ValueTask<DataTable> GetBookList(int pageIndex, int pageSize)
        {
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT isbn,bookName,author,press,picture FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            sbr.AppendLine((pageIndex - 1 * pageSize).ToString());
            return await ExecuteDataTableAsync(sbr.ToString());
        }

        public async ValueTask<BookInfo> GetOneBookInfoAsync(string isbn)
        {
            BookInfo book = new();
            string sql = $"SELECT * FROM main WHERE isbn = {isbn}";

            using SQLiteConnection DbConnection = GetSQLiteConnection();
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
                book.Keyword = reader.GetString(10);
                book.Language = reader.GetString(11);
                book.Picture = reader.GetString(12);
                book.ShelfNumber = reader.GetInt64(13);
                book.IsBorrowed = Convert.ToBoolean(reader.GetInt64(14));
            }
            command.Dispose();
            reader.Dispose();
            return book;
        }

        public async void AddBookAsync(BookInfo book)
        {
            string sqlStr = $"INSERT INTO main VALUES ('{book.Isbn}','{book.BookName}','{book.Author}','{book.Press}','{book.PressDate}','{book.PressPlace}','{book.Price}','{book.ClcName}','{book.BookDesc}','{book.Pages}','{book.Keyword}','{book.Language}','{book.Picture}',@shelfNumber,@isBorrowed)";
            SQLiteParameter[] parameters = [
                        new SQLiteParameter("@shelfNumber", book.ShelfNumber),
                        new SQLiteParameter("@isBorrowed", book.IsBorrowed),
                    ];
            await ExecuteNonQueryAsync(sqlStr, parameters);
        }

        public async ValueTask<DataTable> AutoSuggestByStringAsync(string str)
        {
            string sql = $"SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main WHERE bookName LIKE '%{str}%' OR author LIKE '%{str}%'";
            return await ExecuteDataTableAsync(sql);
        }

        public async ValueTask<DataTable> AutoSuggestByNumAsync(int num)
        {
            string sql = $"SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main WHERE isbn = {num} OR shelfNumber = {num}";
            return await ExecuteDataTableAsync(sql);
        }

        public async ValueTask<DataTable> AutoSuggestBookShelfInfoByStringAsync(string str)
        {
            string sql = $"SELECT isbn,bookName,author,press,picture FROM main WHERE bookName LIKE '%{str}%' OR author LIKE '%{str}%'";
            return await ExecuteDataTableAsync(sql);
        }

        public async ValueTask<DataTable> AutoSuggestBookShelfInfoByNumAsync(int num)
        {
            string sql = $"SELECT isbn,bookName,author,press,picture FROM main WHERE shelfNumber = {num}";
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
