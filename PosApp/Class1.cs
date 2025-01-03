using MySql.Data.MySqlClient;
using System;
using System.Data;

public static class DatabaseHelper
{
    private static string connectionString = "Server=localhost;Database=pos_app;Uid=root;Pwd=;";

    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }
}
