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
    public partial class UserForm : Form
    {
        private Form LoginForm;
        private List<List<string>> information; // 登录信息
        private MySqlConnection dbConnection;
        private string tableName; // 当前表
        private List<string> attributes; // 属性
        private List<List<string>> result; // 记录
        private int row;
        private int col;
        private Form updateSetTable; // 更新
        private Form selectFromTable; // 查询

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

        protected void ConnectToDatabase() // 连接数据库
        {
            DBUtil.ImportFromTxt();
            dbConnection = DBUtil.GetConnection();

            if (dbConnection == null)
            {
                ShowDatabaseError();
            }
        }

        protected void CloseDatabase() // 关闭数据库
        {
            DBUtil.Close(dbConnection);
        }

        protected void ShowDatabaseError() // 显示数据库错误信息
        {
            MessageBox.Show("数据库连接错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        protected void UpdateInformation()  // 更新借阅信息的已借阅时间、逾期状态、逾期时间、逾期费用
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

        protected void GetTableInformation(string tableName)  // 获取表的信息
        {
            MySqlCommand cmd = new MySqlCommand("", dbConnection);

            // 获取属性名
            string sqlCol = $"show columns from {tableName}";
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("无法获取到属性名" + ex.Message);
                return;
            }

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
                    if (result == null)
                        this.result = new List<List<string>>();
                    else
                        this.result.Clear();
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

        protected void SelectItems()  // 查询
        {
            this.selectFromTable = new CreateSelectFromTable(dbConnection, tableName, attributes, 1, col, 0);
            this.selectFromTable.ShowDialog();
        }

        protected void DisplayBookTable()  // 显示图书信息
        {
            tableName = "book";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView1);  // 显示
        }

        protected void GetBook(object sender, EventArgs e)  // 图书信息
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

        protected void SelectBookTable()  // 查询图书信息
        {
            tableName = "book";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        protected void GetBorrow(object sender, EventArgs e)  // 借阅信息
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

        protected void DisplayBorrowTable()  // 显示借阅信息
        {
            tableName = "borrow";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            DisplayTable(this.dataGridView2);  // 显示
        }

        protected void SelectBorrowTable()  // 查询借阅信息
        {
            tableName = "borrow";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            SelectItems();  // 查询
        }

        protected void DisplayPersonalInformation()  // 显示个人信息
        {
            tableName = "user";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            DisplayTable(dataGridView3);
        }

        protected void UpdatePersonalInformation() // 更新个人信息
        {
            tableName = "user";
            GetTableInformation(tableName);  // 从数据库中获取所需信息
            this.updateSetTable = new CreateUpdateSetTable(dbConnection, tableName, attributes, 1, col, 0, information);
            this.updateSetTable.Show();
        }

        protected void DisplayAll()
        {
            this.DisplayBookTable();
            this.DisplayBorrowTable();
            this.DisplayTable(dataGridView3);
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
            int currentIndex = comboBox3.SelectedIndex;
            if (currentIndex == 0)
            {
                // 显示个人信息
                DisplayPersonalInformation();
            }
            else if (currentIndex == 1 && dataGridView3.Rows.Count > 0)
            {
                // 更新个人信息
                UpdatePersonalInformation();
            }
        }

        private void toolStripMenuItem1_1_1_Click(object sender, EventArgs e)
        {
            // 显示图书信息
            DisplayBookTable();
        }

        private void toolStripMenuItem1_1_2_Click(object sender, EventArgs e)
        {
            // 查询图书信息
            SelectBookTable();
        }

        private void toolStripMenuItem1_2_1_Click(object sender, EventArgs e)
        {
            // 显示借阅信息
            DisplayBorrowTable();
        }

        private void toolStripMenuItem1_2_2_Click(object sender, EventArgs e)
        {
            // 查询借阅信息
            SelectBorrowTable();
        }

        private void toolStripMenuItem1_3_Click(object sender, EventArgs e)
        {
            // 显示图书信息
            DisplayBookTable();
            // 显示借阅信息
            DisplayBorrowTable();
            // 显示个人信息
            DisplayPersonalInformation();
        }

        private void toolStripMenuItem1_4_Click(object sender, EventArgs e)
        {
            //重新登录
            LogInAgain();
        }

        private void toolStripMenuItem1_5_Click(object sender, EventArgs e)
        {
            //退出
            Exit();
        }
    }
}
