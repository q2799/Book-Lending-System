using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookLendingSystem
{
    public partial class NewForm : Form
    {
        protected MySqlConnection dbConnection;
        protected string tableName;
        protected List<string> attributes = new List<string>(); // 属性
        protected int row;
        protected int col;

        public NewForm(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null, int row = 0, int col = 0)
        {
            InitializeComponent();
            this.dbConnection = dbConnection;
            this.tableName = tableName;
            this.attributes = attributes;
            this.row = row;
            this.col = col;
        }

        protected void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定执行该操作吗？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Console.WriteLine("NewForm");
            }
        }

        protected void SetItem() // 设置表格属性
        {
            if (dataGridView1.RowCount > 0)
            {
                dataGridView1.Rows.Clear();
            }
            dataGridView1.RowCount = this.row;  // 设置表格行数
            dataGridView1.ColumnCount = this.col;  // 设置表格列数

            if (this.col != 0)
            {
                string className = "BookLendingSystem." + char.ToUpper(this.tableName[0]) + this.tableName.Substring(1).ToLower() + "Attributes";
                Type classType = Assembly.GetExecutingAssembly().GetType(className);
                if (classType == null)
                {
                    MessageBox.Show("类异常");
                    return;
                }

                for (int j = 0; j < col; j++)
                {
                    string propertyName = char.ToUpper(attributes[j][0]) + attributes[j].Substring(1);
                    FieldInfo fieldInfo = classType.GetField(propertyName, BindingFlags.Public | BindingFlags.Static);
                    if (fieldInfo == null)
                    {
                        MessageBox.Show("属性异常");
                        continue;
                    }
                    object propertyValue = fieldInfo.GetValue(null);
                    dataGridView1.Columns[j].HeaderText = propertyValue.ToString();
                }
            }
        }
    }

}
