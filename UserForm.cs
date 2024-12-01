using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using BookLendingSystem.tool;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI.CRUD;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BookLendingSystem
{
    public partial class UserForm : Form
    {
        private Form LoginForm;
        private List<List<string>> information = new List<List<string>>() { new List<string>() };
        private MySqlConnection dbConnection;
        private string tableName; // 当前表
        private List<string> attributes = new List<string>(); // 属性
        private List<List<string>> result = new List<List<string>>() { new List<string>() }; // 记录
        private int row;
        private int col;
        private List<List<string>> updateSetTable = new List<List<string>>() { new List<string>() }; // 更新
        private List<List<string>> selectFromTable = new List<List<string>>() { new List<string>() }; // 查询

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
            this.updateSetTable = null;  // 更新
            this.selectFromTable = null;  // 查询
            this.ConnectToDatabase();
        }

        private void ConnectToDatabase() // 连接数据库
        {
            DBUtil.ImportFromTxt();
            dbConnection = DBUtil.GetConnection();

            if (dbConnection == null)
            {
                ShowDatabaseError();
            }
        }

        private void CloseDatabase() // 关闭数据库
        {
            DBUtil.Close(dbConnection);
        }

        private void ShowDatabaseError() // 显示数据库错误信息
        {
            MessageBox.Show("数据库连接错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void UpdateInformation()  // 更新借阅信息的已借阅时间、逾期状态、逾期时间、逾期费用
        {
            MySqlCommand cmd = new MySqlCommand("", dbConnection);
            string sql = "select * from borrow";
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
                        for (int colIndex = 0; colIndex < initialResult.FieldCount; colIndex++)
                        {
                            if (colIndex == 0)
                            {
                                rowResult.Add(initialResult[0].ToString());
                            }
                            if (colIndex < 6)
                            {
                                continue;
                            }
                            else if (colIndex == 6)  // 获取更新后的已借阅时间
                            {
                                rowResult.Add(Config.BorrowedDate(initialResult[4].ToString()).ToString());
                            }
                            else if (colIndex == 7)  // 获取更新后的逾期状态
                            {
                                rowResult.Add(Config.Overdue(initialResult[5].ToString()).ToString());
                            }
                            else if (colIndex == 8)  // 获取更新后的逾期时间
                            {
                                rowResult.Add(Config.OverdueDate(initialResult[5].ToString()).ToString());
                            }
                            else if (colIndex == 9)  // 获取更新后的逾期费用
                            {
                                rowResult.Add(string.Format("{0:0.00}", Config.OverdueCost(Convert.ToInt32(rowResult[2]))));
                            }
                        }
                        updateResult.Add(rowResult);
                    }

                    // 更新数据
                    for (int i = 0; i < updateResult.Count; i++)
                    {
                        sql = $"update borrow set borrowedDate = '{updateResult[i][1]}', overdue = '{updateResult[i][2]}', overdueDate = '{updateResult[i][3]}', overdueCost = '{updateResult[i][4]}'  where borrowId = '{updateResult[i][0]}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (transaction != null)
                {
                    transaction.Rollback();  // 回滚事务
                }
            }
        }

        private void GetTableInformation(string tableName)  // 获取表的信息
        {
            MySqlCommand cmd = new MySqlCommand("", dbConnection);
            if (tableName == "borrow")
            {
                UpdateInformation();
            }
            string sql = $"select * from {tableName}";
            if (tableName == "borrow" || tableName == "user")
            {
                sql += $" where userId = '{(this.information[0][0])}'";
            }
            cmd.CommandText = sql;
            try
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    this.result = new List<List<string>>();
                    while (reader.Read())
                    {
                        List<string> resultList = new List<string>();
                        for (int colIndex = 0; colIndex < reader.FieldCount; colIndex++)
                        {
                            resultList.Add(reader[colIndex].ToString());
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

        public void DisplayTable(DataGridView dataGridView)  // 显示列表
        {
            if (dataGridView.RowCount > 0)
            {
                dataGridView.Rows.Clear();
            }
            dataGridView.RowCount = this.row;  // 设置表格行数
            dataGridView.ColumnCount = this.col;  // 设置表格列数

            // 显示表的内容
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    string data = result[i][j];
                    dataGridView.Rows[i].Cells[j].Value = data;
                }
            }
        }

        public void SelectItems()  // 查询
        {

        }

        private void DisplayBookTable()  // 显示图书信息
        {
            tableName = "book";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView1);  // 显示
        }

        private void GetBook(object sender, EventArgs e)  // 图书信息
        {
            int currentIndex = this.comboBox1.SelectedIndex;  // 获取图书列表的选项框的当前选项的索引
            if (currentIndex == 0)  // 显示图书信息
            {
                DisplayBookTable();
            }
            else if (currentIndex == 1 && this.dataGridView1.RowCount > 0)  // 查询图书信息
            {
                SelectBookTable();
            }
        }

        private void SelectBookTable()  // 查询图书信息
        {
            tableName = "book";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        private void GetBorrow(object sender, EventArgs e)  // 借阅信息
        {
            int currentIndex = this.comboBox2.SelectedIndex;  // 获取借阅列表的选项框的当前选项的索引
            if (currentIndex == 0)  // 显示借阅信息
            {
                DisplayBorrowTable();
            }
            else if (currentIndex == 1 && this.dataGridView2.RowCount > 0)  // 查询借阅信息
            {
                SelectBorrowTable();
            }
        }

        private void DisplayBorrowTable()  // 显示借阅信息
        {
            tableName = "borrow";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView2);  // 显示
        }

        private void SelectBorrowTable()  // 查询借阅信息
        {
            tableName = "borrow";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        public void DisplayPersonalInformation()  // 显示个人信息
        {
            tableName = "user";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            DisplayTable(dataGridView3);
        }

        public void UpdatePersonalInformation() // 更新个人信息
        {

        }

        public void DisplayAll()
        {
            this.DisplayBookTable();
            this.DisplayBorrowTable();
            this.DisplayTable(dataGridView3);
        }

        
        public void LogInAgain()//重新登录
        {
            //var login = new LoginForm();
            this.CloseDatabase();
            LoginForm.Show();
            this.Close();
        }

        
        public void Exit()//退出
        {
            this.CloseDatabase();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox1.SelectedIndex;
            if (currentIndex == 0)
            {
                // 显示图书信息
                DisplayBookTable();
            }
            else if (currentIndex == 1 && dataGridView1.Rows.Count > 0)
            {
                // 查询图书信息
                SelectBookTable();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox2.SelectedIndex;
            if (currentIndex == 0)
            {
                // 显示借阅信息
                DisplayBorrowTable();
            }
            else if (currentIndex == 1 && dataGridView2.Rows.Count > 0)
            {
                // 查询借阅信息
                SelectBorrowTable();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int currentIndex = comboBox2.SelectedIndex;
            if (currentIndex == 0)
            {
                // 显示个人信息
                this.DisplayPersonalInformation();
            }
            else if (currentIndex == 1 && dataGridView2.Rows.Count > 0)
            {
                // 更新个人信息
                UpdatePersonalInformation();
            }
        }
    }
}
