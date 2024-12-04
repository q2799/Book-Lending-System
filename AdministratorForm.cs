using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace BookLendingSystem
{
    internal partial class AdministratorForm : Form
    {
        private Form LoginForm;
        private List<List<string>> information; // 登录信息
        private MySqlConnection dbConnection;
        private string tableName; // 当前表
        private List<string> attributes; // 属性
        private List<List<string>> result; // 记录
        private int row;
        private int col;
        private CreateInsertIntoTable insertIntoTable; // 增加
        private CreateDeleteFromTable deleteFromTable; // 删除
        private CreateUpdateSetTable updateSetTable; // 更新
        private CreateSelectFromTable selectFromTable; // 查询
        private int flag;

        public AdministratorForm(Form LoginForm, List<List<string>> information = null)
        {
            InitializeComponent();
            this.LoginForm = LoginForm;
            this.information = information;
            this.dbConnection = null;  // 数据库连接
            this.tableName = null;  // 当前表
            this.attributes = null;  // 属性
            this.result = null;  // 记录
            this.row = 0;  // 行
            this.col = 0;  // 列
            this.insertIntoTable = null;  // 增加
            this.deleteFromTable = null;  // 删除
            this.updateSetTable = null;  // 更新
            this.selectFromTable = null;  // 查询
            this.flag = 2;
            this.ConnectToDatabase();
            this.Text = $"管理员：{information[0][3]}（{information[0][1]}）";
        }

        protected void ConnectToDatabase() // 连接数据库
        {
            DBUtil.ImportFromTxt();
            dbConnection = DBUtil.GetConnection();

            if (dbConnection == null)
                ShowDatabaseError();
        }

        protected void CloseDatabase() { DBUtil.Close(dbConnection); } // 关闭数据库

        protected void ShowDatabaseError() // 显示数据库错误信息
        {
            MessageBox.Show("数据库连接错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected void UpdateInformation()  // 更新借阅信息的已借阅时间、逾期状态、逾期时间、逾期费用
        {
            MySqlCommand cmd = new MySqlCommand("", dbConnection);
            string sql = "SELECT * FROM borrow";
            MySqlTransaction transaction = null;
            try
            {
                transaction = dbConnection.BeginTransaction();
                cmd.Transaction = transaction;
                cmd.CommandText = sql;
                using (MySqlDataReader initialResult = cmd.ExecuteReader())
                {
                    // 遍历初始数据，并获取更新后的数据，准备更新
                    List<List<string>> updateResult = new List<List<string>>();
                    while (initialResult.Read())
                    {
                        List<string> rowResult = new List<string>();
                        rowResult.Add(initialResult[0].ToString());
                        string borrowDate = initialResult.GetDateTime(4).ToString("yyyy-MM-dd");
                        string returnDate = initialResult.GetDateTime(5).ToString("yyyy-MM-dd");
                        rowResult.Add(Config.BorrowedDate(borrowDate).ToString());
                        rowResult.Add(Config.Overdue(returnDate).ToString());
                        rowResult.Add(Config.OverdueDate(returnDate).ToString());
                        rowResult.Add(string.Format("{0:0.00}", Config.OverdueCost(Config.OverdueDate(returnDate))));
                        updateResult.Add(rowResult);
                    }
                    initialResult.Close();

                    // 更新数据
                    for (int i = 0; i < updateResult.Count; i++)
                    {
                        sql = $"UPDATE borrow SET borrowedDate = '{updateResult[i][1]}', overdue = '{updateResult[i][2]}', overdueDate = '{updateResult[i][3]}', overdueCost = '{updateResult[i][4]}'  WHERE borrowId = '{updateResult[i][0]}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();  // 回滚事务
                }
                Console.WriteLine($"UpdateInformation: {ex.Message}");
            }
        }

        protected void GetTableColumns() // 获取表的列名
        {
            MySqlCommand cmd = new MySqlCommand("", dbConnection);

            // 获取属性名
            string sqlCol = $"SHOW COLUMNS FROM {tableName}";
            cmd.CommandText = sqlCol;
            if (attributes == null)
                this.attributes = new List<string>();
            else
                this.attributes.Clear();
            try
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        attributes.Add(reader["Field"].ToString());
                    }
                    this.col = attributes.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法获取到属性名" + ex.Message);
                return;
            }
        }

        protected void GetTableData()  // 获取表的数据
        {
            if (tableName == "borrow")
                UpdateInformation();

            MySqlCommand cmd = new MySqlCommand("", dbConnection);
            string sql = $"SELECT * FROM {tableName} ORDER BY {attributes[0]} ASC";
            cmd.CommandText = sql;

            try
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (result == null)
                        this.result = new List<List<string>>();
                    else
                        this.result.Clear();
                    while (reader.Read())
                    {
                        List<string> resultList = new List<string>();
                        for (int colIndex = 0; colIndex < reader.FieldCount; colIndex++)
                        {
                            string result = "";
                            if (Config.IsDate(attributes[colIndex]))
                                result = reader.GetDateTime(colIndex).ToString("yyyy-MM-dd");
                            else
                                result = reader[colIndex].ToString();
                            resultList.Add(result);
                        }
                        result.Add(resultList);
                    }
                    this.row = this.result.Count;
                    this.col = reader.FieldCount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            cmd.Dispose();
        }

        protected void GetTableInformation() // 获取表的信息
        {
            GetTableColumns();
            GetTableData();
        }

        protected void DisplayTable(DataGridView dataGridView)  // 显示列表
        {
            if (dataGridView.RowCount > 0)
            {
                dataGridView.Rows.Clear();
            }

            DataTable dataTable = new DataTable();
            for (int j = 0; j < this.col; j++)
            {
                dataTable.Columns.Add(attributes[j]);
            }

            // 显示表的内容
            for (int i = 0; i < this.row; i++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int j = 0; j < this.col; j++)
                    dataRow[j] = result[i][j];
                dataGridView.Rows.Add(dataRow.ItemArray.Cast<object>().ToArray());
            }
        }

        protected List<List<string>> GetSelectRows(DataGridView dataGridView) // 获取选中行
        {
            DataGridViewSelectedRowCollection selectedRows = dataGridView.SelectedRows;
            List<List<string>> data = new List<List<string>>();
            foreach (DataGridViewRow row in selectedRows)
            {
                List<string> rowData = new List<string>();
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    string cellValue = row.Cells[i].Value?.ToString();
                    rowData.Add(cellValue);
                }
                data.Add(rowData);
            }
            return data;
        }

        protected void SelectItems()  // 查询
        {
            this.selectFromTable = new CreateSelectFromTable(dbConnection, tableName, attributes, 1, col, flag);
            this.selectFromTable.Show();
        }

        protected void DisplayStaffTable()  // 显示职员信息
        {
            tableName = "staff";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView1);  // 显示
        }

        protected void DisplayUserTable()  // 显示用户信息
        {
            tableName = "user";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView2);  // 显示
        }

        protected void DisplayBookTable()  // 显示图书信息
        {
            tableName = "book";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView3);  // 显示
        }

        protected void DisplayBorrowTable()  // 显示借阅信息
        {
            tableName = "borrow";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView4);  // 显示
        }

        protected void DisplayPersonalInformation()  // 显示个人信息
        {
            tableName = "staff";
            GetTableInformation();  // 从数据库中获取所需信息
            if (result == null)
                this.result = new List<List<string>>();
            else
                this.result.Clear();
            this.result.Add(this.information[0]);
            this.row = 1;
            DisplayTable(dataGridView5);
        }

        protected void InsertStaffTable() // 增加职员信息
        {
            tableName = "staff";
            GetTableColumns(); // 获取列名
            this.insertIntoTable = new CreateInsertIntoTable(dbConnection, tableName, attributes, 1, col, flag, information);
            this.insertIntoTable.Show();
        }

        protected void InsertUserTable() // 增加用户信息
        {
            tableName = "user";
            GetTableColumns(); // 获取列名
            this.insertIntoTable = new CreateInsertIntoTable(dbConnection, tableName, attributes, 1, col, flag, information);
            this.insertIntoTable.Show();
        }

        protected void InsertBookTable() // 增加图书信息
        {
            tableName = "book";
            GetTableColumns(); // 获取列名
            this.insertIntoTable = new CreateInsertIntoTable(dbConnection, tableName, attributes, 1, col, flag, information);
            this.insertIntoTable.Show();
        }

        protected void InsertBorrowTable() // 借阅图书
        {
            tableName = "borrow";
            GetTableColumns(); // 获取列名
            this.insertIntoTable = new CreateInsertIntoTable(dbConnection, tableName, attributes, 1, col, flag, information);
            this.insertIntoTable.Show();
        }

        protected void  DeleteStaffTable() // 删除职员信息
        {
            tableName = "staff";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView1);
            if (data != null && data.Count > 0)
            {
                this.deleteFromTable = new CreateDeleteFromTable(dbConnection, tableName, attributes, data.Count, col, flag, information, data);
                this.deleteFromTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要删除的职员信息所在行");
            }
        }

        protected void DeleteUserTable() // 删除用户信息
        {
            tableName = "user";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView2);
            if (data != null && data.Count > 0)
            {
                this.deleteFromTable = new CreateDeleteFromTable(dbConnection, tableName, attributes, data.Count, col, flag, information, data);
                this.deleteFromTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要删除的用户信息所在行");
            }
        }

        protected void DeleteBookTable() // 删除图书信息
        {
            tableName = "book";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView3);
            if (data != null && data.Count > 0)
            {
                this.deleteFromTable = new CreateDeleteFromTable(dbConnection, tableName, attributes, data.Count, col, flag, information, data);
                this.deleteFromTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要删除的图书信息所在行");
            }
        }

        protected void DeleteBorrowTable() // 归还图书
        {
            tableName = "borrow";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView4);
            if (data != null && data.Count > 0)
            {
                this.deleteFromTable = new CreateDeleteFromTable(dbConnection, tableName, attributes, data.Count, col, flag, information, data);
                this.deleteFromTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要删除的借阅信息所在行");
            }
        }

        protected void UpdateStaffTable() // 更新职员信息
        {
            tableName = "staff";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView1);
            if (data != null && data.Count > 0)
            {
                this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, data.Count, col, flag, data);
                this.updateSetTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要更新的职员信息所在行");
            }
        }

        protected void UpdateUserTable() // 更新用户信息
        {
            tableName = "user";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView2);
            if (data != null && data.Count > 0)
            {
                this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, data.Count, col, flag, data);
                this.updateSetTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要更新的用户信息所在行");
            }
        }

        protected void UpdateBookTable() // 更新图书信息
        {
            tableName = "book";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView3);
            if (data != null && data.Count > 0)
            {
                this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, data.Count, col, flag, data);
                this.updateSetTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要更新的图书信息所在行");
            }
        }

        protected void UpdateBorrowTable() // 更新借阅信息
        {
            tableName = "borrow";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView4);
            if (data != null && data.Count > 0)
            {
                this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, data.Count, col, flag, data);
                this.updateSetTable.Show();
            }
            else
            {
                MessageBox.Show("选中需要更新的借阅信息所在行");
            }
        }

        protected void Extension() // 延期归还图书
        {
            tableName = "borrow";
            GetTableColumns(); // 获取列名
            List<List<string>> data = GetSelectRows(dataGridView4);
            if (data != null && data.Count > 0)
            {
                OverdueForm overdueForm = new OverdueForm(dbConnection, attributes, data[0]);
                overdueForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("选中需要延期的借阅信息所在行");
            }
        }

        protected void UpdatePersonalInformation() // 更新个人信息
        {
            tableName = "staff";
            GetTableInformation();  // 从数据库中获取所需信息
            this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, 1, col, flag, information);
            this.updateSetTable.Show();
        }

        protected void SelectStaffTable()  // 查询图书信息
        {
            tableName = "staff";
            GetTableInformation();  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        protected void SelectUserTable()  // 查询用户信息
        {
            tableName = "user";
            GetTableInformation();  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        protected void SelectBookTable()  // 查询图书信息
        {
            tableName = "book";
            GetTableInformation();  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        protected void SelectBorrowTable()  // 查询借阅信息
        {
            tableName = "borrow";
            GetTableInformation();  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        protected void CancelAccount() // 注销账号
        {
            tableName = "staff";
            GetTableColumns(); // 获取列名
            this.deleteFromTable = new CreateDeleteFromTable(dbConnection, tableName, attributes, information.Count, col, flag, information, information);
            if (this.deleteFromTable.CancelAccountSuccess())
            {
                LoginForm.Show();
                this.Close();
            }
        }

        protected void LogInAgain() //重新登录
        {
            this.CloseDatabase();
            LoginForm.Show();
            this.Close();
        }

        protected void Exit() //退出
        {
            this.CloseDatabase();
            LoginForm.Close();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox1.SelectedIndex;
            if (currentIndex == 0) DisplayStaffTable(); // 显示职员信息
            else if (currentIndex == 1) { InsertStaffTable(); } // 增加职员信息
            else if (currentIndex == 2 && dataGridView1.Rows.Count > 0) DeleteStaffTable(); // 删除职员信息
            else if (currentIndex == 3 && dataGridView1.Rows.Count > 0) UpdateStaffTable(); // 更新职员信息
            else if (currentIndex == 4 && dataGridView1.Rows.Count > 0) { SelectStaffTable(); } // 查询职员信息
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox2.SelectedIndex;
            if (currentIndex == 0) DisplayUserTable(); // 显示用户信息
            else if (currentIndex == 1) InsertUserTable(); // 增加用户信息
            else if (currentIndex == 2 && dataGridView2.Rows.Count > 0) DeleteUserTable(); // 删除用户信息
            else if (currentIndex == 3 && dataGridView2.Rows.Count > 0) UpdateUserTable(); // 更新用户信息
            else if (currentIndex == 4 && dataGridView2.Rows.Count > 0) { SelectUserTable(); } // 查询用户信息
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox3.SelectedIndex;
            if (currentIndex == 0) DisplayBookTable(); // 显示图书信息
            else if (currentIndex == 1) InsertBookTable(); // 增加图书信息
            else if (currentIndex == 2 && dataGridView3.Rows.Count > 0) DeleteBookTable(); // 删除图书信息
            else if (currentIndex == 3 && dataGridView3.Rows.Count > 0) UpdateBookTable(); // 更新图书信息
            else if (currentIndex == 4 && dataGridView3.Rows.Count > 0) SelectBookTable(); // 查询图书信息
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox4.SelectedIndex;
            if (currentIndex == 0) DisplayBorrowTable(); // 显示借阅信息
            else if (currentIndex == 1) { InsertBorrowTable(); } // 借阅图书
            else if (currentIndex == 2 && dataGridView4.Rows.Count > 0) DeleteBorrowTable(); // 归还图书
            else if (currentIndex == 3 && dataGridView4.Rows.Count > 0) UpdateBorrowTable(); // 更新借阅信息
            else if (currentIndex == 4 && dataGridView4.Rows.Count > 0) Extension(); // 延期归还图书
            else if (currentIndex == 5 && dataGridView4.Rows.Count > 0) SelectBorrowTable(); // 查询借阅信息
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox5.SelectedIndex;
            if (currentIndex == 0) DisplayPersonalInformation(); // 显示个人信息
            else if (currentIndex == 1 && dataGridView5.Rows.Count > 0) UpdatePersonalInformation(); // 更新个人信息
        }

        private void toolStripMenuItem1_1_1_Click(object sender, EventArgs e) { DisplayStaffTable(); } // 显示职员信息

        private void toolStripMenuItem1_2_1_Click(object sender, EventArgs e) { DisplayUserTable(); } // 显示用户信息

        private void toolStripMenuItem1_3_1_Click(object sender, EventArgs e) { DisplayBookTable(); } // 显示图书信息

        private void toolStripMenuItem1_4_1_Click(object sender, EventArgs e) { DisplayBorrowTable(); } // 显示借阅信息

        private void toolStripMenuItem1_1_2_Click(object sender, EventArgs e) { InsertStaffTable(); } // 增加职员信息

        private void toolStripMenuItem1_2_2_Click(object sender, EventArgs e) { InsertUserTable(); } // 增加用户信息

        private void toolStripMenuItem1_3_2_Click(object sender, EventArgs e) { InsertBookTable(); } // 增加图书信息

        private void toolStripMenuItem1_4_2_Click(object sender, EventArgs e) { InsertBorrowTable(); } // 借阅图书

        private void toolStripMenuItem1_1_3_Click(object sender, EventArgs e) { DeleteStaffTable(); } // 删除职员信息

        private void toolStripMenuItem1_2_3_Click(object sender, EventArgs e) { DeleteUserTable(); } // 删除用户信息

        private void toolStripMenuItem1_3_3_Click(object sender, EventArgs e) { DeleteBookTable(); } // 删除图书信息

        private void toolStripMenuItem1_4_3_Click(object sender, EventArgs e) { DeleteBorrowTable(); } // 归还图书

        private void toolStripMenuItem1_1_4_Click(object sender, EventArgs e) { UpdateStaffTable(); } // 更新职员信息

        private void toolStripMenuItem1_2_4_Click(object sender, EventArgs e) { UpdateUserTable(); } // 更新用户信息

        private void toolStripMenuItem1_3_4_Click(object sender, EventArgs e) { UpdateBookTable(); } // 更新图书信息

        private void toolStripMenuItem1_4_4_Click(object sender, EventArgs e) { UpdateBorrowTable(); } // 更新借阅信息

        private void toolStripMenuItem1_4_5_Click(object sender, EventArgs e) { Extension(); } // 延期归还图书

        private void toolStripMenuItem1_1_5_Click(object sender, EventArgs e) { SelectStaffTable(); } // 查询职员信息

        private void toolStripMenuItem1_2_5_Click(object sender, EventArgs e) { SelectUserTable(); } // 查询用户信息

        private void toolStripMenuItem1_3_5_Click(object sender, EventArgs e) { SelectBookTable(); } // 查询图书信息

        private void toolStripMenuItem1_4_6_Click(object sender, EventArgs e) {  SelectBorrowTable(); } // 查询借阅信息

        private void toolStripMenuItem1_5_Click(object sender, EventArgs e) // 显示所有信息
        {
            DisplayStaffTable();
            DisplayUserTable();
            DisplayBookTable();
            DisplayBorrowTable();
            DisplayPersonalInformation();
        }

        private void toolStripMenuItem1_6_Click(object sender, EventArgs e) { LogInAgain(); } //重新登录

        private void toolStripMenuItem1_7_Click(object sender, EventArgs e) { Exit(); } //退出

        private void toolStripMenuItem1_8_Click(object sender, EventArgs e) // 注销账号
        {
            DialogResult result = MessageBox.Show("确定注销账号吗？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                CancelAccount();
            }
        }
    }
}
