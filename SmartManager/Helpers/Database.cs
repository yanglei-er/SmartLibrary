using Shared.Helpers;
using Shared.Models;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace SmartManager.Helpers
{
    public class Database : SQLiteHelper
    {
        private static readonly Dictionary<string, Database> DataBaceList = [];

        private Database(string filename) : base(filename)
        {
            DataSource = @".\database\" + filename;
        }

        public static Database GetDatabase(string filename)
        {
            if (DataBaceList.TryGetValue(filename, out Database? value))
            {
                return value;
            }
            else
            {
                Database db = new(filename);
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
                sbr.AppendLine("'name' TEXT PRIMARY KEY NOT NULL,");
                sbr.AppendLine("'sex' TEXT,");
                sbr.AppendLine("'age' TEXT,");
                sbr.AppendLine("'joinTime',");
                sbr.AppendLine("'feature' BLOB NOT NULL");
                sbr.AppendLine(");");
                await ExecuteNonQueryAsync(sbr.ToString());
            }
        }

        public async ValueTask<int> GetRecordCountAsync()
        {
            object? result = await ExecuteScalarAsync("SELECT count(id) FROM main");
            return Convert.ToInt32(result);
        }

        public async ValueTask<DataTable> ExecutePagerSimpleAsync(int pageIndex, int pageSize)
        {
            StringBuilder sbr = new();
            sbr.AppendLine("SELECT isbn,bookName,author,shelfNumber,isBorrowed FROM main LIMIT ");
            sbr.AppendLine(pageSize.ToString());
            sbr.AppendLine(" OFFSET ");
            int OffsetIndex = (pageIndex - 1) * pageSize;
            sbr.AppendLine(OffsetIndex.ToString());
            return await ExecuteDataTableAsync(sbr.ToString());
        }

        public async void AddUserAsync(User user)
        {
            string sqlStr = $"INSERT INTO main VALUES ('{user.Name}','{user.Sex}','{user.Age}','{user.JoinTime}','{user.Feature}')";
            await ExecuteNonQueryAsync(sqlStr);
        }
    }
}
