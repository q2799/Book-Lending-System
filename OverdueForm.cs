using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using MySqlX.XDevAPI.CRUD;
using MySqlX.XDevAPI.Relational;

namespace BookLendingSystem
{
    internal partial class OverdueForm : Form
    {
        protected MySqlConnection dbConnection;
        protected string tableName;
        protected List<string> attributes; // 属性
        protected List<string> initialData;
        protected List<string> updateData;

        public OverdueForm(MySqlConnection dbConnection = null, List<string> attributes = null, List<string> initialData = null)
        {
            InitializeComponent();
            this.dbConnection = dbConnection;
            this.attributes = attributes;
            this.initialData = initialData; // 保留初始数据
            this.Text = "图书延期";
            SetData();
        }

        protected void SetData()
        {
            updateData = new List<string>();
            for (int j = 0; j < initialData.Count; j++)
                updateData.Add(initialData[j]);
        }

        protected void Extension()
        {
            MySqlTransaction transaction = null;

            try
            {
                int day = Convert.ToInt32(textBox1.Text);
                if (day > 0)
                {
                    try
                    {
                        updateData[5] = Config.ExtensionReturnDate(initialData[5], day);
                        updateData[7] = Config.Overdue(initialData[5]).ToString();
                        updateData[8] = Config.OverdueDate(initialData[5]).ToString();
                        updateData[9] = string.Format("{0:0.00}", Config.OverdueCost(Convert.ToInt32(updateData[8])));

                        string sql = $"UPDATE borrow SET returnDate = @Value1, overdue = @Value2, overdueDate = @Value3, " +
                            $"overdueCost = @Value4 WHERE {attributes[0]} = @Id";
                        transaction = dbConnection.BeginTransaction();
                        using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@Value1", updateData[5]);
                            cmd.Parameters.AddWithValue("@Value2", updateData[7]);
                            cmd.Parameters.AddWithValue("@Value3", updateData[8]);
                            cmd.Parameters.AddWithValue("@Value4", updateData[9]);
                            cmd.Parameters.AddWithValue("@Id", initialData[0]);
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();  // 回滚事务
                        }
                        Console.WriteLine("图书延期时出错: " + ex.Message);
                        MessageBox.Show("图书延期时出错: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("延期时间异常");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取延期数据时出错: " + ex.Message);
                MessageBox.Show("获取延期数据时出错: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定延期？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Extension();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
