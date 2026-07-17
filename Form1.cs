using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;



namespace ExportDataApp
{
    public partial class Form1 : Form
    {
        private DataTable salesTable = new DataTable();

        private PrintDocument printDocument = new PrintDocument();

        private int printRowIndex = 0;

        private void AddSalesRecord(string salesId,
                            string customerName,
                            string product,
                            int quantity,
                            decimal unitPrice)
        {
            decimal totalAmount = quantity * unitPrice;

            salesTable.Rows.Add(salesId,
                                customerName,
                                product,
                                quantity,
                                unitPrice,
                                totalAmount);
        }
        private void UpdateSummary()
        {
            int recordCount = salesTable.Rows.Count;
            decimal grandTotal = 0;

            foreach (DataRow row in salesTable.Rows)
            {
                grandTotal += Convert.ToDecimal(row["Total Amount"]);
            }

            lblRecordCount.Text = $"Records: {recordCount}";
            lblGrandTotal.Text = $"Grand Total: RM {grandTotal:0.00}";
        }
        private void ExportDataGridViewToCsv(string filePath)
        {
            StringBuilder csvContent = new StringBuilder();

            for (int i = 0; i < dgvSales.Columns.Count; i++)
            {
                csvContent.Append(EscapeCsvValue(dgvSales.Columns[i].HeaderText));

                if (i < dgvSales.Columns.Count - 1)
                {
                    csvContent.Append(",");
                }
            }

            csvContent.AppendLine();

            foreach (DataGridViewRow row in dgvSales.Rows)
            {
                if (!row.IsNewRow)
                {
                    for (int i = 0; i < dgvSales.Columns.Count; i++)
                    {
                        object value = row.Cells[i].Value;

                        csvContent.Append(EscapeCsvValue(value?.ToString() ?? ""));

                        if (i < dgvSales.Columns.Count - 1)
                        {
                            csvContent.Append(",");
                        }
                    }

                    csvContent.AppendLine();
                }
            }

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
        }

        private string EscapeCsvValue(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            Font headerFont = new Font("Segoe UI", 10, FontStyle.Bold);
            Font bodyFont = new Font("Consolas", 10);

            float y = 50;
            float leftMargin = e.MarginBounds.Left;
            float lineHeight = bodyFont.GetHeight(e.Graphics) + 6;

            e.Graphics.DrawString("Sales Report", titleFont, Brushes.Black, leftMargin, y);
            y += 40;

            e.Graphics.DrawString($"Date: {DateTime.Now}", bodyFont, Brushes.Black, leftMargin, y);
            y += 30;

            e.Graphics.DrawString("Sales ID   Customer Name        Product             Qty   Price     Total",
                                  headerFont,
                                  Brushes.Black,
                                  leftMargin,
                                  y);

            y += 20;

            e.Graphics.DrawLine(Pens.Black, leftMargin, y, e.MarginBounds.Right, y);
            y += 15;

            while (printRowIndex < salesTable.Rows.Count)
            {
                DataRow row = salesTable.Rows[printRowIndex];

                string salesId = row["Sales ID"].ToString();
                string customerName = row["Customer Name"].ToString();
                string product = row["Product"].ToString();
                int quantity = Convert.ToInt32(row["Quantity"]);
                decimal unitPrice = Convert.ToDecimal(row["Unit Price"]);
                decimal totalAmount = Convert.ToDecimal(row["Total Amount"]);

                string line = $"{salesId,-10} {customerName,-20} {product,-18} {quantity,3} {unitPrice,8:0.00} {totalAmount,10:0.00}";

                e.Graphics.DrawString(line, bodyFont, Brushes.Black, leftMargin, y);

                y += lineHeight;
                printRowIndex++;

                if (y > e.MarginBounds.Bottom - 80)
                {
                    e.HasMorePages = true;
                    return;
                }
            }

            y += 20;
            e.Graphics.DrawLine(Pens.Black, leftMargin, y, e.MarginBounds.Right, y);
            y += 20;

            decimal grandTotal = 0;

            foreach (DataRow row in salesTable.Rows)
            {
                grandTotal += Convert.ToDecimal(row["Total Amount"]);
            }

            e.Graphics.DrawString($"Grand Total: RM {grandTotal:0.00}",
                                  headerFont,
                                  Brushes.Black,
                                  leftMargin,
                                  y);

            e.HasMorePages = false;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            salesTable.Columns.Add("Sales ID", typeof(string));
            salesTable.Columns.Add("Customer Name", typeof(string));
            salesTable.Columns.Add("Product", typeof(string));
            salesTable.Columns.Add("Quantity", typeof(int));
            salesTable.Columns.Add("Unit Price", typeof(decimal));
            salesTable.Columns.Add("Total Amount", typeof(decimal));

            dgvSales.DataSource = salesTable;

            dgvSales.Columns["Unit Price"].DefaultCellStyle.Format = "0.00";
            dgvSales.Columns["Total Amount"].DefaultCellStyle.Format = "0.00";

            printDocument.PrintPage += PrintDocument_PrintPage;

            UpdateSummary();

        }

        private void btnLoadSampleData_Click(object sender, EventArgs e)
        {
            salesTable.Rows.Clear();

            AddSalesRecord("S1001", "David Tan", "Wireless Mouse", 2, 35.50m);
            AddSalesRecord("S1002", "Siti Aminah", "USB Drive", 3, 25.00m);
            AddSalesRecord("S1003", "John Lee", "Keyboard", 1, 45.00m);
            AddSalesRecord("S1004", "Aisha Wong", "Notebook", 5, 5.50m);
            AddSalesRecord("S1005", "Michael Lim", "Headphones", 2, 60.00m);

            UpdateSummary();

            MessageBox.Show("Sample data loaded successfully.",
                            "Data Loaded",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (salesTable.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.",
                                "No Data",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveFileDialog.Title = "Export Data to Excel";
            saveFileDialog.FileName = "SalesReport.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportDataGridViewToCsv(saveFileDialog.FileName);

                    MessageBox.Show("Data exported successfully.",
                                    "Export Complete",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting data.\n\n" + ex.Message,
                                    "Export Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }

        }

        private void btnPrintPdf_Click(object sender, EventArgs e)
        {
            if (salesTable.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to print.",
                                "No Data",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                return;
            }

            printRowIndex = 0;

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (salesTable.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to clear.",
                                "No Data",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to clear all records?",
                                                  "Confirm Clear",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                salesTable.Rows.Clear();
                UpdateSummary();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?",
                                      "Confirm Exit",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }

        }

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            if (salesTable.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to preview.",
                                "No Data",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                return;
            }

            printRowIndex = 0;

            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument;
            previewDialog.Width = 1000;
            previewDialog.Height = 700;

            previewDialog.ShowDialog();

        }
    }

}

        
    
