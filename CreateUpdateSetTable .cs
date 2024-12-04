using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookLendingSystem
{
    internal class CreateUpdateSetTable : NewForm
    {
        protected List<List<string>> initialData;
        protected List<List<string>> updateData;

        public CreateUpdateSetTable() : base()
        {
            this.initialData = null;
            this.Text = "更新";
        }

        public CreateUpdateSetTable(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null,
            int row = 0, int col = 0, int flag = 0, List<List<string>> initialData = null)
            : base(dbConnection, tableName, attributes, row, col, flag)
        {
            this.initialData = initialData; // 保留初始数据
            this.Text = "更新";
            button1.Click -= base.button1_Click;
            button1.Click += button1_Click;
            SetData();
        }

        protected void SetData() // 设置表格
        {
            SetItem();
            // 显示表的内容
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    string data = initialData[i][j];
                    dataGridView1.Rows[i].Cells[j].Value = data;
                }
            }
        }

        protected void GetTableInformation(DataGridView dataGridView1) // 获取当前表信息
        {
            if (updateData == null)
                this.updateData = new List<List<string>>();
            else
                this.updateData.Clear();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                List<string> rowData = new List<string>();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    rowData.Add(cell.Value?.ToString() ?? string.Empty);
                }
                updateData.Add(rowData);
            }
        }

        protected void UpdateSetTable() // 更新数据库表
        {
            MySqlTransaction transaction = null;
            try
            {
                for (int i = 0; i < this.row; i++)
                {
                    for (int j = 0; j < this.col; j++)
                    {
                        if (initialData[i][j] == updateData[i][j])
                            continue;
                        if (attributes[j] == "password")
                            updateData[i][j] = Config.ChangePassword(updateData[i][j]); // 加密

                        string sql = $"UPDATE {this.tableName} SET {attributes[j]} = @Value WHERE {attributes[0]} = @Id";

                        transaction = dbConnection.BeginTransaction();
                        using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                        {
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@Value", updateData[i][j]);
                            cmd.Parameters.AddWithValue("@Id", initialData[i][0]);
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                }
                MessageBox.Show($"数据更新成功！");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    transaction.Rollback();  // 回滚事务
                }
                Console.WriteLine("更新数据库时出错: " + ex.Message);
            }
        }

        protected new void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定执行该操作吗？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                GetTableInformation(dataGridView1);
                UpdateSetTable();
            }
        }
    }
}
