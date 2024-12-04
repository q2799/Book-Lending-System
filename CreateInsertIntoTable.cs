using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookLendingSystem
{
    internal class CreateInsertIntoTable : NewForm
    {
        protected List<List<string>> information; // 登录信息
        protected List<List<string>> data; // 存储单元格数据

        public CreateInsertIntoTable() : base()
        {
            this.information = new List<List<string>>();
            this.data = new List<List<string>>();
            this.Text = "增加";
        }

        public CreateInsertIntoTable(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null,
            int row = 0, int col = 0, int flag = 0, List<List<string>> information = null)
            : base(dbConnection, tableName, attributes, row, col, flag)
        {
            this.information = information;
            this.data = new List<List<string>>();
            this.Text = "增加";
            button1.Click -= base.button1_Click;
            button1.Click += button1_Click;
            SetItem();
            FillNewRow(dataGridView1); // 设置初始行
            dataGridView1.RowsAdded += new DataGridViewRowsAddedEventHandler(dataGridView1_RowsAdded);
        }

        protected void FillNewRow(DataGridView dataGridView, string id = null, int count = 0)
        {
            DataGridViewRow newRow = dataGridView.Rows[dataGridView.Rows.Count - 1];
            if (tableName == "staff")
            {
                if ((id != null) && (count > 0)) // 职员编号
                    newRow.Cells[0].Value = GenerateId.GenerateStaffId(id, count);
                else
                    newRow.Cells[0].Value = GenerateId.GenerateStaffId();
                newRow.Cells[1].Value = "staff" + newRow.Cells[0].Value.ToString(); // 用户名
                newRow.Cells[2].Value = Config.InitialPassword(); // 密码
                newRow.Cells[5].Value = Config.InitialDate(); // 入职时间
                newRow.Cells[7].Value = 0; // 权限
                dataGridView.Columns[0].ReadOnly = true;
            }
            else if (tableName == "user")
            {
                if ((id != null) && (count > 0))
                    newRow.Cells[0].Value = GenerateId.GenerateUserId(id, count); // 用户编号
                else
                    newRow.Cells[0].Value = GenerateId.GenerateUserId();
                newRow.Cells[1].Value = "user" + newRow.Cells[0].Value.ToString(); // 用户名
                newRow.Cells[2].Value = Config.InitialPassword(); // 密码
                newRow.Cells[5].Value = Config.InitialDate(); // 注册时间
                newRow.Cells[6].Value = Config.InitialNumberOfBooksAvailableBorrowing(); // 可借书总数
                newRow.Cells[7].Value = Config.InitialNumberOfBooksBorrowing(); // 已借书数量
                newRow.Cells[8].Value = Config.InitialOverdue(); // 逾期次数
                dataGridView.Columns[0].ReadOnly = true;
            }
            else if (tableName == "book")
            {
                if ((id != null) && (count > 0))
                    newRow.Cells[0].Value = GenerateId.GenerateBookId(id, count); // 图书编号
                else
                    newRow.Cells[0].Value = GenerateId.GenerateBookId();
                newRow.Cells[5].Value = Config.InitialDate(); // 出版时间
                newRow.Cells[7].Value = Config.InitialBorrowed(); // 借阅状态
                dataGridView.Columns[0].ReadOnly = true;
            }
            else if (tableName == "borrow")
            {
                if ((id != null) && (count > 0))
                    newRow.Cells[0].Value = GenerateId.GenerateBorrowId(id, count); // 借阅编号
                else
                    newRow.Cells[0].Value = GenerateId.GenerateBorrowId();
                newRow.Cells[1].Value = information[0][0]; // 职员编号
                newRow.Cells[4].Value = Config.InitialDate(); // 借阅时间
                newRow.Cells[5].Value = Config.InitialReturnDate(); // 归还时间
                newRow.Cells[6].Value = Config.BorrowedDate(Config.InitialReturnDate()); // 已借阅时间（日）
                newRow.Cells[7].Value = Config.InitialOverdue(); // 逾期状态
                newRow.Cells[8].Value = Config.InitialOverdueDate(); // 逾期时间（日）
                newRow.Cells[9].Value = Config.InitialOverdueCost(); // 逾期费用（元）
                dataGridView.Columns[0].ReadOnly = true;
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //Console.WriteLine(e.RowIndex);
            // 检查是否是新行
            if (e.RowIndex >= dataGridView1.Rows.Count - 1 && dataGridView1.Rows[e.RowIndex].IsNewRow)
            {
                //Console.WriteLine(dataGridView1.Rows[e.RowIndex - 1].Cells[0].Value?.ToString());
                FillNewRow(dataGridView1, dataGridView1.Rows[e.RowIndex - 1].Cells[0].Value?.ToString(), 1);
            }
        }

        protected bool CheckIfExists(MySqlConnection dbConnection, string idValue, string nameValue) // 查重
        {
            if (tableName == "staff" || tableName == "user")
            {
                string sql = $"SELECT COUNT(*) FROM {tableName} WHERE {tableName}Id = @Id AND {tableName}Name = @Name";
                using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", idValue);
                    cmd.Parameters.AddWithValue("@Name", nameValue);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            else if (tableName == "book" || tableName == "borrow")
            {
                string sql = $"SELECT COUNT(*) FROM {tableName} WHERE {tableName}Id = @Id";
                using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                {
                    cmd.Parameters.AddWithValue("@Id", idValue);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            return false;
        }

        protected bool RowDataIsNull(DataGridViewRow row)
        {
            if (row.IsNewRow)
                return true;
            else
            {
                for (int j = 0; j < attributes.Count; j++)
                    if (row.Cells[j].Value == null)
                            return true;
            }
            return false;
        }

        protected bool UserCanBorrowBook(string userId) // 用户可以借书
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

                        if (reader["numberOfBooksAvailableBorrowing"] != null && reader["numberOfBooksBorrowing"] != null)
                        {
                            int numberOfBooksAvailableBorrowing = Convert.ToInt32(reader["numberOfBooksAvailableBorrowing"]);
                            int numberOfBooksBorrowing = Convert.ToInt32(reader["numberOfBooksBorrowing"]);
                            if (numberOfBooksAvailableBorrowing > numberOfBooksBorrowing)
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
                Console.WriteLine($"UserCanBorrowBook: {ex.Message}");
            }
            return false;
        }

        protected bool BookCanBorrow(string bookId) // 图书可以被借阅
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
                            if (borrowed == 0)
                            {
                                reader.Close();
                                return true;
                            }
                            else if (borrowed == 1)
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
                Console.WriteLine($"BookCanBorrow: {ex.Message}");
            }
            return false;
        }

        protected void UserBorrowBook(string userId, string bookId) // 用户借书
        {
            string updateUserSql = "UPDATE user SET numberOfBooksBorrowing = numberOfBooksBorrowing + 1 WHERE userId = @UserId";
            string updateBookSql = "UPDATE book SET borrowed = 1 WHERE bookId = @BookId";
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
                Console.WriteLine($"UserBorrowBook: {ex.Message}");
            }
        }

        protected void InsertIntoTable()
        {
            string sql1 = string.Join(",", attributes);
            string sql2 = string.Join(",", attributes.ConvertAll(attr => $"@{attr}"));
            string sql = $"INSERT INTO {tableName} ({sql1}) VALUES ({sql2})";
            int count = 0;
            MySqlTransaction transaction = null;

            try
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (RowDataIsNull(row)) continue; // 跳过无效行

                    string idValue = row.Cells[0].Value?.ToString();
                    string nameValue = row.Cells[1].Value?.ToString();
                    if (tableName == "staff")
                    {
                        int thisFlag = 0, insertFlag = 0;
                        if (information[0][7] == "1" || information[0][7] == "True")
                            thisFlag = 1;
                        if (row.Cells[7].Value?.ToString() == "1" || row.Cells[7].Value?.ToString() == "True")
                            insertFlag = 1;
                        if (thisFlag < insertFlag) // 不允许增加上一级权限
                            continue;
                    }
                    if (tableName == "borrow")
                    {
                        if (!UserCanBorrowBook(row.Cells[2].Value.ToString())) // 无法借书
                            continue;
                        if (!BookCanBorrow(row.Cells[3].Value.ToString())) // 无法借书
                            continue;
                    }
                    bool exists = CheckIfExists(dbConnection, idValue, nameValue); // id查重
                    if (!exists)
                    {
                        transaction = dbConnection.BeginTransaction();
                        using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                        {
                            cmd.Transaction = transaction;
                            for (int j = 0; j < attributes.Count; j++)
                            {
                                if (attributes[j] == "password" && row.Cells[j].Value != null)
                                    row.Cells[j].Value = Config.ChangePassword(row.Cells[j].Value.ToString());
                                cmd.Parameters.AddWithValue($"@{attributes[j]}", row.Cells[j].Value);
                            }
                            int affectRows = cmd.ExecuteNonQuery();
                            if (affectRows > 0) // 插入成功
                            {
                                count += affectRows;
                                if (tableName == "borrow")
                                    UserBorrowBook(row.Cells[2].Value.ToString(), row.Cells[3].Value.ToString()); // 借书
                            }
                            transaction.Commit();
                        }
                    }
                }
                MessageBox.Show($"{count}条数据增加成功！");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();  // 回滚事务
                }
                Console.WriteLine($"InsertIntoTable: {ex.Message}");
                MessageBox.Show($"InsertIntoTable: {ex.Message}");
            }
        }

        protected new void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定执行该操作吗？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                InsertIntoTable();
            }
        }
    }
}
