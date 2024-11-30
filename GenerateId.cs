using System;
using System.Data;
using MySql.Data.MySqlClient;
using BookLendingSystem.tool;

namespace BookLendingSystem.tool
{
    public class GenerateId
    {
        // ID生成类
        public static string staffId = "10000";  // 职员编号
        public static string userId = "1000000"; // 用户编号
        public static string bookId = "A10000";  // 图书编号
        public static string borrowId = "AA10000"; // 借阅编号

        // 获取下一个职员编号
        public static string GenerateStaffId()
        {
            string tempStaffId = (int.Parse(staffId) + 1).ToString(); // 初始编号为10001
            using (MySqlConnection conn = DBUtil.GetConnection()) // 获取数据库连接
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM staff", conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == tempStaffId) // 编号重复，编号+1
                        {
                            tempStaffId = (int.Parse(tempStaffId) + 1).ToString();
                        }
                        else
                        {
                            break;
                        }
                    }
                    reader.Close();
                }
            }
            return tempStaffId;
        }

        // 获取下一个用户编号
        public static string GenerateUserId()
        {
            string tempUserId = (int.Parse(userId) + 1).ToString(); // 初始编号为1000001
            using (MySqlConnection conn = DBUtil.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM user", conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == tempUserId) // 编号重复，编号+1
                        {
                            tempUserId = (int.Parse(tempUserId) + 1).ToString();
                        }
                        else
                        {
                            break;
                        }
                    }
                    reader.Close();
                }
            }
            return tempUserId;
        }

        // 获取下一个图书编号
        public static string GenerateBookId()
        {
            char head = bookId[0];
            string tail = bookId.Substring(1);
            tail = (int.Parse(tail) + 1).ToString();
            string tempBookId = head + tail; // 初始编号为A10001

            using (MySqlConnection conn = DBUtil.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM book", conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == tempBookId) // 编号重复，编号+1
                        {
                            if (int.Parse(tail) == 99999) // 数字编号满，字母加1
                            {
                                if (head < 'Z') // 字母未满
                                {
                                    head++;
                                    tail = "10000";
                                }
                                else
                                {
                                    return ""; // 字母已满，无法生成新的编号
                                }
                            }
                            else
                            {
                                tail = (int.Parse(tail) + 1).ToString();
                            }
                            tempBookId = head + tail;
                        }
                        else
                        {
                            break;
                        }
                    }
                    reader.Close();
                }
            }
            return tempBookId;
        }

        // 获取下一个借阅编号
        public static string GenerateBorrowId()
        {
            char headFirst = borrowId[0];
            char headSecond = borrowId[1];
            string tail = borrowId.Substring(2);
            tail = (int.Parse(tail) + 1).ToString();
            string tempBorrowId = headFirst + headSecond + tail; // 初始编号为AA10001

            using (MySqlConnection conn = DBUtil.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM borrow", conn))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == tempBorrowId) // 编号重复，编号+1
                        {
                            if (int.Parse(tail) == 99999) // 数字编号满，字母加1
                            {
                                if (headSecond < 'Z') // 第二个字母未满
                                {
                                    headSecond++;
                                    tail = "10000";
                                }
                                else if (headFirst < 'Z') // 第一个字母未满
                                {
                                    headFirst++;
                                    headSecond = 'A';
                                    tail = "10000";
                                }
                                else
                                {
                                    return ""; // 字母已满，无法生成新的编号
                                }
                            }
                            else
                            {
                                tail = (int.Parse(tail) + 1).ToString();
                            }
                            tempBorrowId = headFirst + headSecond + tail;
                        }
                        else
                        {
                            break;
                        }
                    }
                    reader.Close();
                }
            }
            return tempBorrowId;
        }
    }
}
