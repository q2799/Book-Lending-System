using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookLendingSystem
{
    public class CreateUpdateSetTable : NewForm
    {
        protected List<List<string>> initialData;
        protected List<List<string>> updateData;
        protected int flag;

        public CreateUpdateSetTable(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null, int row = 0, int col = 0, int flag = 0, List<List<string>> initialData = null)
            : base(dbConnection, tableName, attributes, row, col)
        {
            this.flag = flag;
            this.initialData = initialData; // 保留初始数据
            button1.Click -= base.button1_Click;
            button1.Click += button1_Click;
            SetData();
        }

        protected void SetData()
        {
            SetItem();
            // 显示表的内容
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    string data = initialData[i][j];
                    dataGridView1.Rows[i].Cells[j].Value = data;
                    if (Config.Editable(flag, tableName, attributes[j]))
                        dataGridView1.Columns[j].ReadOnly = false;
                    else
                        dataGridView1.Columns[j].ReadOnly = true;
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
            //MySqlTransaction transaction = null;
            try
            {
                //transaction = this.dbConnection.BeginTransaction();
                for (int i = 0; i < this.row; i++)
                {
                    for (int j = 0; j < this.col; j++)
                    {
                        if (initialData[i][j] == updateData[i][j])
                            continue;
                        if (attributes[j] == "password")
                            updateData[i][j] = Config.ChangePassword(updateData[i][j]); // 加密

                        string sql = $"update {this.tableName} set {attributes[j]} = @Value where {attributes[0]} = @Id";
                        Console.WriteLine(sql);

                        using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                        {
                            cmd.Parameters.AddWithValue("@Value", updateData[i][j]);
                            cmd.Parameters.AddWithValue("@Id", initialData[i][0]);
                            cmd.ExecuteNonQuery();
                            //transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("更新数据库时出错: " + ex.Message);
                //transaction.Rollback();  // 回滚事务
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
