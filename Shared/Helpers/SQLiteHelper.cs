using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace Shared.Helpers
{
    public class SQLiteHelper
    {
        protected string DataSource = string.Empty;

        protected SQLiteHelper(string filename)
        {
            DataSource = @".\database\" + filename;
        }

        protected SQLiteConnection GetSQLiteConnection()
        {
            string connStr = string.Format("Data Source={0}", DataSource);
            var con = new SQLiteConnection(connStr);
            return con;
        }

        protected static void PrepareCommand(SQLiteCommand cmd, SQLiteConnection conn, string sqlStr, params SQLiteParameter[]? p)
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

        protected static async Task PrepareCommandAsync(SQLiteCommand cmd, SQLiteConnection conn, string sqlStr, params SQLiteParameter[]? p)
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

        protected DataTable ExecuteDataTable(string cmdText, params SQLiteParameter[]? data)
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

        protected async ValueTask<DataTable> ExecuteDataTableAsync(string cmdText, params SQLiteParameter[]? data)
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

        protected int ExecuteNonQuery(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var command = new SQLiteCommand();
            PrepareCommand(command, connection, cmdText, data);
            return command.ExecuteNonQuery();
        }

        protected async ValueTask<int> ExecuteNonQueryAsync(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            SQLiteCommand command = new();
            await PrepareCommandAsync(command, connection, cmdText, data);
            return await command.ExecuteNonQueryAsync();
        }

        protected SQLiteDataReader ExecuteReader(string cmdText, params SQLiteParameter[]? data)
        {
            var command = new SQLiteCommand();
            using SQLiteConnection connection = GetSQLiteConnection();
            PrepareCommand(command, connection, cmdText, data);
            SQLiteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader;
        }

        protected async ValueTask<DbDataReader> ExecuteReaderAsync(string cmdText, params SQLiteParameter[]? data)
        {
            var command = new SQLiteCommand();
            using SQLiteConnection connection = GetSQLiteConnection();
            await PrepareCommandAsync(command, connection, cmdText, data);
            DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            return reader;
        }

        protected object ExecuteScalar(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var cmd = new SQLiteCommand();
            PrepareCommand(cmd, connection, cmdText, data);
            return cmd.ExecuteScalar();
        }

        protected async ValueTask<object?> ExecuteScalarAsync(string cmdText, params SQLiteParameter[]? data)
        {
            using SQLiteConnection connection = GetSQLiteConnection();
            var cmd = new SQLiteCommand();
            await PrepareCommandAsync(cmd, connection, cmdText, data);
            return await cmd.ExecuteScalarAsync();
        }

        protected async void ResetDataBassAsync()
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

    }
}
