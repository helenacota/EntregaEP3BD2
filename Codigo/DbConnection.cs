using Npgsql;
using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace MisConsultas
{
    public class DbConnection : IDisposable
    {
        public string search_path = "SET search_path TO MisPeliculas;";
        public NpgsqlConnection Connection { get; set; }

        public DbConnection()
        {
            var config = ReadConfig("dbconfig.txt");
            string connectionString = $"Server={config["Server"]};Port={config["Port"]};Database={config["Database"]};User Id={config["User Id"]};Password={config["Password"]};";
            Connection = new NpgsqlConnection(connectionString);
            Connection.Open();
        }

        private Dictionary<string, string> ReadConfig(string filePath)
        {
            var config = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    config[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return config;
        }

        public NpgsqlCommand execCommand(string sSql)
        {
            NpgsqlCommand command = new NpgsqlCommand(sSql, this.Connection);
            command.ExecuteNonQuery();
            return command;
        }

        public NpgsqlDataReader getReader(string sSql)
        {
            NpgsqlCommand command = new NpgsqlCommand(sSql, this.Connection);
            NpgsqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        public DataTable getDataTable(string sSql)
        {
            DataTable dataTable = new DataTable();

            using (NpgsqlCommand command = new NpgsqlCommand(sSql, this.Connection))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(dataTable);
            }

            return dataTable;
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}