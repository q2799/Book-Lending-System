using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace BookLendingSystem
{
    internal partial class LoginForm : Form
    {
        public List<List<string>> information = new List<List<string>>() { new List<string>() };
        private MySqlConnection dbConnection; // 数据库连接
        private Form mainWindow; // 系统窗口
        private string name; // 用户名
        private string password; // 密码
        private int isStaff = 0; // 登录角色标识，0为用户，1为职员
        private int loginFlag = 0; // 登录标识，0为登录失败，1为用户，2为管理员

        public LoginForm()
        {
            InitializeComponent();
            ConnectToDatabase();
        }

        private void ConnectToDatabase()
        {
            DBUtil.ImportFromTxt();
            dbConnection = DBUtil.GetConnection();
            
            if (dbConnection == null)
            {
                ShowDatabaseError();
            }
        }

        private void CloseDatabase()
        {
            DBUtil.Close(dbConnection);
        }

        private void ShowDatabaseError() // 显示数据库错误信息
        {
            MessageBox.Show("数据库连接错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            isStaff = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            isStaff = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            name = textBox1.Text;
            password = textBox2.Text;


            if (isStaff == 1)
            {
                loginFlag = SelectStaffId(name, password);
                if (loginFlag == 2) // 管理员
                {
                    mainWindow = new AdministratorForm(this, this.information);
                }
                else if (loginFlag == 1) // 职员
                {
                    mainWindow = new AdministratorForm(this, this.information);
                }
                else
                {
                    MessageBox.Show("用户名或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (isStaff == 0)
            {
                loginFlag = SelectUserId(name, password);
                if (loginFlag == 1)
                {
                    mainWindow = new UserForm(this, this.information);
                }
                else
                {
                    MessageBox.Show("用户名或密码错误", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("未知的用户类型", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            mainWindow.Show();
            this.textBox1.Clear();
            this.textBox2.Clear();
            radioButton1.Checked = false;
            radioButton2.Checked = true;
            this.Hide();
        }

        public int SelectStaffId(string staffName, string password)
        {
            string query = "SELECT * FROM staff WHERE staffName = @staffName AND password = @password";
            using (MySqlCommand cmd = new MySqlCommand(query, dbConnection))
            {
                cmd.Parameters.AddWithValue("@staffName", staffName);
                cmd.Parameters.AddWithValue("@password", Config.ChangePassword(password));

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int isAdmin = reader.GetInt32(7);
                        if (isAdmin == 1)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                                this.information[0].Add(reader[i].ToString()); // 记录查询到的用户信息
                            return 2;
                        }
                        else if (isAdmin == 0)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                                this.information[0].Add(reader[i].ToString()); // 记录查询到的用户信息
                            return 1;
                        }
                    }
                }
            }
            return 0;
        }

        public int SelectUserId(string userName, string password)
        {
            string query = "SELECT * FROM user WHERE userName = @userName AND password = @password";
            using (MySqlCommand cmd = new MySqlCommand(query, dbConnection))
            {
                cmd.Parameters.AddWithValue("@userName", userName);
                cmd.Parameters.AddWithValue("@password", Config.ChangePassword(password));

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            this.information[0].Add(reader[i].ToString()); // 记录查询到的用户信息
                        return 1;
                    }
                }
            }
            return 0;
        }
    }
}
