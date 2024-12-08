using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ASM1DATAE_BH01350
{
    public partial class Form6 : Form
    {
        string connectionstring = @"Data Source=LAPTOP-IG12I1HR\SQLEXPRESS;Initial Catalog=asm22;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";



        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter adt;
        DataTable dt = new DataTable();
        public Form6()
        {
            InitializeComponent();           
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            LoadStatisticData();
        }

        private void LoadStatisticData()
        {
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                try
                {
                    connection.Open();
                    string query = @"
                    SELECT 
                        StatisticID,
                         CustomerProductID,
                         EmployeeID,
                         ProductID,
                         QuantitySold,
                         SaleDate,
                         TotalPrice,
                         InputPrice,
                         (TotalPrice - InputPrice) AS Profit
                    FROM Statistic
                    WHERE SaleDate BETWEEN @StartDate AND @EndDate";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@StartDate", dTP_start.Value.Date);
                    command.Parameters.AddWithValue("@EndDate", dTP_end.Value.Date);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                    CalculateTotals(dt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading data: " + ex.Message, "Error");
                }
            }
        }
        private void CalculateTotals(DataTable dt)
        {
            decimal totalRevenue = 0;
            decimal totalCost = 0;
            decimal totalProfit = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["TotalPrice"] != DBNull.Value)
                    totalRevenue += Convert.ToDecimal(row["TotalPrice"]);

                if (row["InputPrice"] != DBNull.Value)
                    totalCost += Convert.ToDecimal(row["InputPrice"]);

                if (row["Profit"] != DBNull.Value)
                    totalProfit += Convert.ToDecimal(row["Profit"]);
            }
            decimal baseCost = totalCost * 0.8m;
            txtB_TotalRevenue.Text = totalRevenue.ToString("C2");
            txtB_TotalCost.Text = totalCost.ToString("C2");
            txtB_Profit.Text = totalProfit.ToString("C2");
            MessageBox.Show(
                $"Summary:\n" +
                $"- Total Revenue: {totalRevenue:C2}\n" +
                $"- Total Cost: {totalCost:C2}\n" +
                $"- Profit: {totalProfit:C2}",
                "Calculation Results",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btn_dele_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0) 
            {
                try
                {
                    int statisticID = int.Parse(dataGridView1.SelectedRows[0].Cells["StatisticID"].Value.ToString());
                    con = new SqlConnection(connectionstring);
                    con.Open();
                    string query = "DELETE FROM Statistic WHERE StatisticID = @StatisticID";
                    cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@StatisticID", statisticID);
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dt.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                    }
                    else
                    {
                        MessageBox.Show("No record found to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (con != null && con.State == ConnectionState.Open)
                        con.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
