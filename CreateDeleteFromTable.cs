using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace BookLendingSystem
{
    internal class CreateDeleteFromTable : NewForm
    {
        protected List<List<string>> information; // 登录信息
        protected List<List<string>> data; // 存储单元格数据

        public CreateDeleteFromTable() : base()
        {
            this.information = new List<List<string>>();
            this.data = new List<List<string>>();
            this.Text = "删除";
        }

        public CreateDeleteFromTable(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null, 
            int row = 0, int col = 0, int flag = 0, List<List<string>> information = null, List<List<string>> data = null)
            : base(dbConnection, tableName, attributes, row, col, flag)
        {
            this.information = information;
            this.data = data;
            this.Text = "删除";
            button1.Click -= base.button1_Click;
            button1.Click += button1_Click;
            SetData();
            this.dataGridView1.ReadOnly = true;
        }

        protected void SetData() // 设置表格
        {
            SetItem();
            // 显示表的内容
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    string value = data[i][j];
                    dataGridView1.Rows[i].Cells[j].Value = value;
                }
            }
        }


        protected bool UserCanReturnBook(string userId) // 用户可以还书
        {
            string sql = $"SELECT * FROM user WHERE userId = '{userId}'";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        List<string> resultList = new List<string>();
                        for (int colIndex = 0; colIndex < reader.FieldCount; colIndex++)
                            resultList.Add(reader[colIndex].ToString());

                        if (reader["NumberOfBooksBorrowing"] != null && reader["numberOfBooksBorrowing"] != null)
                        {
                            int NumberOfBooksBorrowing = Convert.ToInt32(reader["NumberOfBooksBorrowing"]);
                            if (NumberOfBooksBorrowing > 0)
                            {
                                reader.Close();
                                return true;
                            }
                            else
                            {
                                reader.Close();
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserCanReturnBook: {ex.Message}");
            }
            return false;
        }

        protected bool BookCanReturn(string bookId) // 图书可以被归还
        {
            string sql = $"SELECT * FROM book WHERE bookId = '{bookId}'";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        List<string> resultList = new List<string>();
                        for (int colIndex = 0; colIndex < reader.FieldCount; colIndex++)
                            resultList.Add(reader[colIndex].ToString());
                        if (reader["borrowed"] != null)
                        {
                            int borrowed = Convert.ToInt32(reader["borrowed"]);
                            if (borrowed == 1)
                            {
                                reader.Close();
                                return true;
                            }
                            else if (borrowed == 0)
                            {
                                reader.Close();
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BookCanReturn: {ex.Message}");
            }
            return false;
        }

        protected void UserReturnBook(string userId, string bookId) // 用户还书
        {
            string updateUserSql = "UPDATE user SET numberOfBooksBorrowing = numberOfBooksBorrowing - 1 WHERE userId = @UserId";
            string updateBookSql = "UPDATE book SET borrowed = 0 WHERE bookId = @BookId";
            //Console.WriteLine(updateUserSql);
            //Console.WriteLine(updateBookSql);
            try
            {
                using (MySqlCommand updateUserCmd = new MySqlCommand(updateUserSql, dbConnection))
                {
                    updateUserCmd.Parameters.AddWithValue("@UserId", userId);
                    updateUserCmd.ExecuteNonQuery(); // 执行更新用户操作
                }

                using (MySqlCommand updateBookCmd = new MySqlCommand(updateBookSql, dbConnection))
                {
                    updateBookCmd.Parameters.AddWithValue("@BookId", bookId);
                    updateBookCmd.ExecuteNonQuery(); // 执行更新图书操作
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserReturnBook: {ex.Message}");
            }
        }

        protected bool CancelAccount() // 注销账号
        {
            string sql = $"DELETE FROM {tableName} WHERE {tableName}Id = @id";
            MySqlTransaction transaction = null;

            try
            {
                foreach (List<string> rowData in data)
                {
                    if (rowData == null || rowData.Count == 0) continue; // 跳过无效行

                    string idValue = rowData[0];
                    string nameValue = rowData[1];
                    transaction = dbConnection.BeginTransaction();

                    using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                    {
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@id", rowData[0]);
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                }
                MessageBox.Show($"注销成功！");
                return true;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();  // 回滚事务
                }
                Console.WriteLine($"CancelAccount: {ex.Message}");
                MessageBox.Show($"CancelAccount: {ex.Message}");
                return false;
            }
        }

        internal bool CancelAccountSuccess() // 注销成功
        {
            return CancelAccount();
        }

        protected void DeleteFromTable()
        {
            string sql = $"DELETE FROM {tableName} WHERE {tableName}Id = @id";
            int count = 0;
            MySqlTransaction transaction = null;

            try
            {
                foreach (List<string> rowData in data)
                {
                    if(rowData == null || rowData.Count == 0) continue; // 跳过无效行

                    string idValue = rowData[0];
                    string nameValue = rowData[1];
                    if (tableName == "staff")
                    {
                        if (information[0][0] == rowData[0]) // 不允许删除自己
                            continue;

                        int thisFlag = 0, deleteFlag = 0;
                        if (information[0][7] == "1" || information[0][7] == "True")
                            thisFlag = 1;
                        if (rowData[7] == "1" || rowData[7] == "True")
                            deleteFlag = 1;
                        if (thisFlag < deleteFlag) // 不允许删除上一级权限
                            continue;
                    }
                    if (tableName == "borrow")
                    {
                        if (!UserCanReturnBook(rowData[2])) // 无法还书
                            continue;
                        if (!BookCanReturn(rowData[3])) // 无法还书
                            continue;
                    }
                    using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                    {
                        transaction = dbConnection.BeginTransaction();
                        cmd.Transaction = transaction;
                        cmd.Parameters.AddWithValue("@id", rowData[0]);
                        int affectRows = cmd.ExecuteNonQuery();
                        if (affectRows > 0) // 删除成功
                        {
                            count += affectRows;
                            if (tableName == "borrow")
                                UserReturnBook(rowData[2], rowData[3]); // 还书
                        }
                        transaction.Commit();
                    }
                }
                MessageBox.Show($"{count}条数据删除成功！");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();  // 回滚事务
                }
                Console.WriteLine($"DeleteFromTable: {ex.Message}");
                MessageBox.Show($"DeleteFromTable: {ex.Message}");
            }
        }


        protected new void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定执行该操作吗？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                DeleteFromTable();
            }
        }
    }
}
