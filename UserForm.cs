using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI.CRUD;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BookLendingSystem
{
    internal partial class UserForm : Form
    {
        private Form LoginForm;
        private List<List<string>> information; // 登录信息
        private MySqlConnection dbConnection;
        private string tableName; // 当前表
        private List<string> attributes; // 属性
        private List<List<string>> result; // 记录
        private int row;
        private int col;
        private int flag;
        private CreateUpdateSetTable updateSetTable; // 更新
        private CreateSelectFromTable selectFromTable; // 查询

        public UserForm(Form LoginForm, List<List<string>> information = null)
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
            this.flag = 0; // 标识符
            this.updateSetTable = null;  // 更新
            this.selectFromTable = null;  // 查询
            this.ConnectToDatabase();
            this.Text = $"用户：{information[0][3]}（{information[0][1]}）";
        }

        protected void ConnectToDatabase() // 连接数据库
        {
            DBUtil.ImportFromTxt();
            dbConnection = DBUtil.GetConnection();

            if (dbConnection == null)
                ShowDatabaseError();
        }

        protected void CloseDatabase() {  DBUtil.Close(dbConnection); } // 关闭数据库

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
            string sql = $"SELECT * FROM {tableName}";
            if (tableName == "borrow" || tableName == "user")
            {
                sql += $" WHERE userId = '{(this.information[0][0])}'";
            }
            sql += $" ORDER BY {attributes[0]} ASC";
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
                dataGridView.Rows.Clear();

            DataTable dataTable = new DataTable();
            for (int j = 0; j < this.col; j++)
                dataTable.Columns.Add(attributes[j]);

            // 显示表的内容
            for (int i = 0; i < this.row; i++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int j = 0; j < this.col; j++)
                    dataRow[j] = result[i][j];
                dataGridView.Rows.Add(dataRow.ItemArray.Cast<object>().ToArray());
            }
        }

        protected void SelectItems()  // 查询
        {
            this.selectFromTable = new CreateSelectFromTable(dbConnection, tableName, attributes, 1, col, flag);
            this.selectFromTable.Show();
        }

        protected void DisplayBookTable()  // 显示图书信息
        {
            tableName = "book";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView1);  // 显示
        }

        protected void DisplayBorrowTable()  // 显示借阅信息
        {
            tableName = "borrow";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView2);  // 显示
        }

        protected void DisplayPersonalInformation()  // 显示个人信息
        {
            tableName = "user";
            GetTableInformation();  // 从数据库中获取所需信息
            DisplayTable(dataGridView3);
        }

        protected void UpdatePersonalInformation() // 更新个人信息
        {
            tableName = "user";
            GetTableInformation();  // 从数据库中获取所需信息
            this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, 1, col, flag, information);
            this.updateSetTable.Show();
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
            if (currentIndex == 0) DisplayBookTable(); // 显示图书信息
            else if (currentIndex == 1 && dataGridView1.Rows.Count > 0) SelectBookTable(); // 查询图书信息
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox2.SelectedIndex;
            if (currentIndex == 0) DisplayBorrowTable(); // 显示借阅信息
            else if (currentIndex == 1 && dataGridView2.Rows.Count > 0) SelectBorrowTable(); // 查询借阅信息
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox3.SelectedIndex;
            if (currentIndex == 0) DisplayPersonalInformation(); // 显示个人信息
            else if (currentIndex == 1 && dataGridView3.Rows.Count > 0) UpdatePersonalInformation(); // 更新个人信息
        }

        private void toolStripMenuItem1_1_1_Click(object sender, EventArgs e) { DisplayBookTable(); } // 显示图书信息

        private void toolStripMenuItem1_2_1_Click(object sender, EventArgs e) { DisplayBorrowTable(); } // 显示借阅信息

        private void toolStripMenuItem1_1_2_Click(object sender, EventArgs e) { SelectBookTable(); } // 查询图书信息

        private void toolStripMenuItem1_2_2_Click(object sender, EventArgs e) { SelectBorrowTable(); } // 查询借阅信息

        private void toolStripMenuItem1_3_Click(object sender, EventArgs e) // 显示所有信息
        {
            DisplayBookTable();
            DisplayBorrowTable();
            DisplayPersonalInformation();
        }

        private void toolStripMenuItem1_4_Click(object sender, EventArgs e) { LogInAgain(); } //重新登录

        private void toolStripMenuItem1_5_Click(object sender, EventArgs e) { Exit(); } //退出
    }
}
