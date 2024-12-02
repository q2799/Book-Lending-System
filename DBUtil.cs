using System;
using System.IO;
using MySql.Data.MySqlClient;

namespace BookLendingSystem
{
    public class DBUtil
    {
        // 数据库连接对象
        public static MySqlConnection db = null;
        public static string database = null;
        private static string host = null;
        private static string port = null;
        private static string user = null;
        private static string password = null;
        private static string filePath = @"../../db.config.txt";

        // 静态方法，用于从文件导入配置
        public static void ImportFromTxt()
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                foreach (string line in lines)
                {
                    string[] splitValues = line.Trim().Split('=');
                    string key = splitValues[0].Trim();
                    string value = splitValues[1].Trim();

                    if (key == "host")
                    {
                        host = value;
                    }
                    else if (key == "port")
                    {
                        port = value;
                    }
                    else if (key == "user")
                    {
                        user = value;
                    }
                    else if (key == "password")
                    {
                        password = value;
                    }
                    else if (key == "database")
                    {
                        database = value;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error importing from file: {e.Message}");
            }
        }

        // 获取数据库连接对象
        public static MySqlConnection GetConnection()
        {
            ImportFromTxt();
            try
            {
                db = new MySqlConnection($"server={host};port={port};user={user};password={password};database={database}");
                db.Open();
                return db;
            }
            catch (MySqlException e)
            {
                Console.WriteLine($"Error connecting to database: {e.Message}");
                return null;
            }
        }

        // 关闭数据库连接
        public static void Close(MySqlConnection connection)
        {
            if (connection != null)
            {
                connection.Close();
            }
        }
    }
}
