﻿using Shared.Helpers;
using Shared.Models;
using System.Data;
using System.Data.Common;
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
                sbr.AppendLine("'joinTime' TEXT,");
                sbr.AppendLine("'feature' TEXT NOT NULL,");
                sbr.AppendLine("'image' BLOB NOT NULL");
                sbr.AppendLine(");");
                await ExecuteNonQueryAsync(sbr.ToString());
            }
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

        public async ValueTask<int> GetRecordCountAsync()
        {
            object? result = await ExecuteScalarAsync("SELECT count(name) FROM main");
            return Convert.ToInt32(result);
        }

        public async void DelBookAsync(string name)
        {
            await ExecuteNonQueryAsync($"DELETE FROM main WHERE name = {name}");
        }

        public async ValueTask<bool> ExistsAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            object? result = await ExecuteScalarAsync($"SELECT COUNT(*) FROM main WHERE name = {name}", null);
            if (Convert.ToInt32(result) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void AddFaceAsync(User user)
        {
            string sqlStr = $"INSERT INTO main VALUES ('{user.Name}','{user.Sex}','{user.Age}','{user.JoinTime}','{user.Feature}',@faceImage)";
            byte[] image = ImageProcess.BitmapImageToByte(user.FaceImage);
            SQLiteParameter parameter = new("@faceImage", DbType.Binary, image.Length)
            {
                Value = image
            };
            await ExecuteNonQueryAsync(sqlStr, parameter);
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
                string name = reader.GetString(0);
                if (!await ExistsAsync(name))
                {
                    string sqlStr = $"INSERT INTO main VALUES ('{name}','{reader.GetString(1)}','{reader.GetString(2)}','{reader.GetString(3)}','{reader.GetString(4)}',@faceImage)";
                    byte[] image = (byte[])reader.GetValue(5);
                    SQLiteParameter parameter = new("@faceImage", DbType.Binary, image.Length) { Value = image };
                    await PrepareCommandAsync(command, oldDbConnection, sqlStr, parameter);
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

        public async void CleanDatabaseAsync()
        {
            using SQLiteConnection conn = GetSQLiteConnection();
            var cmd = new SQLiteCommand();

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            cmd.Parameters.Clear();
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM main;";
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 30;
            await cmd.ExecuteNonQueryAsync();

            cmd.Parameters.Clear();
            cmd.CommandText = "vacuum";
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
