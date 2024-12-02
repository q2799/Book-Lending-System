using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookLendingSystem
{
    public class CreateSelectFromTable : NewForm
    {
        protected List<string> data;
        protected int flag;

        public CreateSelectFromTable(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null, int row = 0, int col = 0, int flag = 0)
            : base(dbConnection, tableName, attributes, row, col)
        {
            this.data = new List<string>();
            this.flag = flag;
            dataGridView1.ReadOnly = false;
            button1.Click -= base.button1_Click;
            button1.Click += button1_Click;
            SetItem();
            dataGridView1.ReadOnly = false;
        }

        protected void SelectFromTable() // 查询
        {
            string sql = "select ";
            for (int j = 0; j < attributes.Count; j++)
            {
                if (j > 0) sql += ", ";
                sql += attributes[j];
            }
            sql += " from " + tableName;
            if (dataGridView1.Rows.Count > 0)
            {
                for (int j = 0; j < col; j++)
                {
                    DataGridViewCell cell = dataGridView1.Rows[0].Cells[j];
                    data.Add(cell.Value?.ToString() ?? string.Empty);
                }

                // data中有非空字符串
                if (data.Any(d => !string.IsNullOrEmpty(d)))
                {
                    sql += " where ";
                    for (int j = 0; j < col; j++)
                    {
                        if (j > 0)
                            sql += " and ";
                        sql += attributes[j] + " like concat('%', @Param" + j + ", '%')";
                    }
                    //Console.WriteLine(sql);
                    using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                    {
                        for (int j = 0; j < data.Count; j++)
                        {
                            cmd.Parameters.AddWithValue("@Param" + j, data[j]);
                        }
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            dataGridView1.Rows.Clear();
                            foreach (DataRow row in dataTable.Rows)
                            {
                                dataGridView1.Rows.Add(row.ItemArray.Cast<object>().ToArray());
                            }
                        }
                    }
                }
                dataGridView1.ReadOnly = true;
            }
        }

        protected new void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确定执行该操作吗？", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                SelectFromTable();
            }
        }
    }
}
