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
using static Mysqlx.Expect.Open.Types;

namespace BookLendingSystem
{
    internal class CreateSelectFromTable : NewForm
    {
        protected List<string> data;

        public CreateSelectFromTable()
        {
            this.data = new List<string>();
            this.Text = "查询";
        }

        public CreateSelectFromTable(MySqlConnection dbConnection = null, string tableName = null, List<string> attributes = null,
            int row = 0, int col = 0, int flag = 0)
            : base(dbConnection, tableName, attributes, row, col, flag)
        {
            this.data = new List<string>();
            this.Text = "查询";
            button1.Click -= base.button1_Click;
            button1.Click += button1_Click;
            SetItem();
            dataGridView1.ReadOnly = false;
            for (int j = 0; j < col; j++)
                dataGridView1.Columns[j].ReadOnly = false;
        }

        protected void SelectFromTable() // 查询
        {
            string sql1 = string.Join(",", attributes);
            string sql = $"SELECT {sql1} FROM {tableName}";
            List<string> conditions = new List<string>(); // WHERE语句

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
                    for (int j = 0; j < data.Count; j++)
                    {
                        DataGridViewCell cell = dataGridView1.Rows[0].Cells[j];
                        string value = cell.Value?.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(value))
                        {
                            conditions.Add($"{attributes[j]} LIKE CONCAT('%', @Param{j}, '%')");
                        }
                    }

                    if (conditions.Count > 0)
                        sql += " WHERE " + string.Join(" AND ", conditions);
                }
            }
            Console.WriteLine(sql);
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(sql, dbConnection))
                {
                    for (int j = 0; j < data.Count; j++)
                    {
                        cmd.Parameters.AddWithValue("@Param" + j, data[j]);
                    }
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        dataGridView1.Rows.Clear();
                        while (reader.Read())
                        {
                            List<object> rowValues = new List<object>();
                            for (int j = 0; j < attributes.Count; j++)
                            {
                                string value = "";
                                if (Config.IsDate(attributes[j]))
                                    value = reader.GetDateTime(j).ToString("yyyy-MM-dd");
                                else
                                    value = reader[j].ToString();
                                rowValues.Add(value);
                            }
                            dataGridView1.Rows.Add(rowValues.ToArray());
                        }
                    }
                }
                dataGridView1.ReadOnly = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SelectFromTable: " + ex.Message);
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
