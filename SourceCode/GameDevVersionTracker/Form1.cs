using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Linq.Expressions;

namespace GameDevVersionTracker
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        string connectionString = "Data Source=172.22.100.45;Initial Catalog=CSPCMTeamMarketIDs;Persist Security Info=True;User ID=cspcc;Password=#CSPCMT!K3rnel99";

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            groupBox14.Visible = false;
            label15.Visible = false;
            labelSuccess.Visible = false;
            clearButton.Visible = false;
            {
                // Load data for the initial selected tab
                updateDataGridView(tabControl1.SelectedIndex);
            }


            {
                // Retrieve and display the most recent value when the form loads
                DisplayMostRecentValue();
            }

            comboBoxDepartment.SelectedIndex = 0;

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)

        {
            // Update DataGridView when the selected tab changes
            updateDataGridView(tabControl1.SelectedIndex);
        }
        private void updateDataGridView(int selectedIndex)
        {

            // Determine the query based on the selected tab
            string query = "";
            switch (selectedIndex)
            {
                case 0: // Tab 1
                    query = "SELECT * FROM MarketVersionTracker ORDER BY id ASC";
                    groupBox14.Visible = false; // Hide groupBox14 for Tab 1 and Tab 2
                    break;
                case 1: // Tab 2
                    query = "SELECT * FROM MarketVersionTracker ORDER BY id ASC";
                    groupBox14.Visible = false; // Hide groupBox14 for Tab 1 and Tab 2
                    break;
                case 2: // Tab 3
                    query = "SELECT * FROM MarketVersionTracker ORDER BY id ASC";
                    groupBox14.Visible = true; // Show groupBox14 for Tab 3
                    break;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridViewInfo.DataSource = table;

                //Scroll to the latest entry
                if (dataGridViewInfo.Rows.Count > 0)
                {
                    dataGridViewInfo.FirstDisplayedScrollingRowIndex = dataGridViewInfo.Rows.Count - 1;
                }

            }

        }



        private void DisplayMostRecentValue()
        {
            // SQL query to retrieve the most recent value            
            string query = "SELECT TOP 1 Version# FROM MarketVersionTracker ORDER BY id DESC";

            // Create SqlConnection and SqlCommand objects
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        // Execute the query and get the result
                        object result = command.ExecuteScalar();

                        // Check if result is not null and convert to string
                        if (result != null)
                        {

                            // Convert the retrieved value to a string
                            string resultString = result.ToString();

                            // Split the result into integer and decimal parts
                            string[] parts = resultString.Split('.');

                            // Parse the integer part and increment it
                            int intValue = int.Parse(parts[0]) + 1;

                            // Reconstruct the result string with the incremented integer part
                            string displayValue = intValue + "." + parts[1];
                            textBoxVersion.Text = displayValue;
                            connection.Close();
                        }
                        else
                        {
                            // Handle case where no result is returned
                            MessageBox.Show("No data found.");
                            connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle error control
                        MessageBox.Show("Error: " + ex.Message);
                        connection.Close();
                    }
                }
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            updatelistBox.Items.Clear();
            backgroundWorker1.RunWorkerAsync();

        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            string storedProcedureName = "InsertMarketVersions";
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(storedProcedureName, connection))

                try
                {

                    addButton.Enabled = false;
                    connection.Open();

                    // Set command type to stored procedure
                    updatelistBox.Items.Add("Calling stored procedure.");
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    string selectedValueAsString = comboBoxDepartment.SelectedItem.ToString();

                    // Add parameters to 'InsertMarketVersion' stored procedure
                    command.Parameters.AddWithValue("@VERSION#", textBoxVersion.Text);
                    command.Parameters.AddWithValue("@MARKET", textBoxMarket.Text);
                    command.Parameters.AddWithValue("@CABINET", textBoxCabinet.Text);
                    command.Parameters.AddWithValue("@PLATFORM", textBoxPlatform.Text);
                    command.Parameters.AddWithValue("@DEPARTMENT", selectedValueAsString);
                    command.Parameters.AddWithValue("@LEADER", textBoxLeader.Text);
                    updatelistBox.Items.Add("Attempting to add: ");
                    updatelistBox.Items.Add(textBoxVersion.Text);
                    updatelistBox.Items.Add(textBoxMarket.Text);
                    updatelistBox.Items.Add(textBoxCabinet.Text);
                    updatelistBox.Items.Add(textBoxPlatform.Text);
                    updatelistBox.Items.Add(selectedValueAsString);
                    updatelistBox.Items.Add(textBoxLeader.Text);

                    // Add an output parameter for capturing success flag
                    SqlParameter successParam = new SqlParameter("@Success", System.Data.SqlDbType.Int);
                    successParam.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(successParam);

                    // Execute the stored procedure
                    command.ExecuteNonQuery();

                    // Retrieve the value of the output parameter
                    int successFlag = (int)successParam.Value;

                    // Update the ListBox with the success flag message
                    if (successFlag == 1)
                    {
                        updatelistBox.Items.Add("Success!");
                        // updates list to scroll to bottom
                        updatelistBox.TopIndex = updatelistBox.Items.Count - 1; 
                        connection.Close();
                        MessageBox.Show("Update successful!", "Success", MessageBoxButtons.OK);
                    }
                    else
                    {
                        // Handle error reporting with stored procedure
                        updatelistBox.Items.Add("Something happened within the successFlag -- Stored Procedure reporting.");
                        connection.Close();
                        addButton.Enabled = false;
                        
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    updatelistBox.Items.Add("Error: " + ex.Message);
                    connection.Close();
                }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // Clear text boxes after entry complete
            addButton.Enabled = true;
            updatelistBox.Items.Clear();
            textBoxMarket.Clear();
            textBoxCabinet.Clear();
            textBoxPlatform.Clear();
            comboBoxDepartment.SelectedItem = null;
            textBoxLeader.Clear();

            // Queries and updated Version # to next available
            DisplayMostRecentValue();
            
            updatelistBox.Items.Add("Bump to next available Version #");

        }

        private void UpdatelistBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0 && e.Index < updatelistBox.Items.Count)
            {
                e.DrawBackground();

                var myValue = updatelistBox.Items[e.Index].ToString();

                if (myValue.Contains("Error: "))
                    e.Graphics.FillRectangle(Brushes.PaleVioletRed, e.Bounds);
                else if (myValue.Contains("Success!"))
                    e.Graphics.FillRectangle(Brushes.LightGreen, e.Bounds);
                else if (myValue.Contains("Something happened within the successFlag -- Stored Procedure reporting."))
                    e.Graphics.FillRectangle(Brushes.PaleVioletRed, e.Bounds);
               

                e.Graphics.DrawString(updatelistBox.Items[e.Index].ToString(), e.Font, Brushes.Black, new PointF(e.Bounds.X, e.Bounds.Y));
                e.DrawFocusRectangle();
            }
        }

        private void SearchTrigger()
        {
            string searchTerm;
            label15.Visible = true;

            // Determine which column to search based on the provided input
            string columnName;
            if (!string.IsNullOrEmpty(MarketText.Text))
            {
                columnName = "Market";
                searchTerm = MarketText.Text;
            }
            else if (!string.IsNullOrEmpty(VersionsSearchBox.Text))
            {
                columnName = "Version#";
                searchTerm = VersionsSearchBox.Text;
            }
            else
            {
                MessageBox.Show("Please enter a Market or a Version#.");
                return;
            }


            string storedProcedureName = "SearchByMarketOrVersion";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ColumnName", columnName);
                command.Parameters.AddWithValue("@SearchTerm", searchTerm);

                connection.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dataGridView1.DataSource = dataTable;

                connection.Close();

                // Sort the DataGridView by ascending order of the first column
                dataGridView1.DataSource = dataTable;
                dataGridView1.Sort(dataGridView1.Columns[0], System.ComponentModel.ListSortDirection.Ascending);
            }

        }

            private void Searchbutton_Click(object sender, EventArgs e)
        {

            SearchTrigger();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Populating the Market info in order to make changes
                VerSearchtextBox.Text = row.Cells["Version#"].Value.ToString();
                MarkSerchtextBox.Text = row.Cells["Market"].Value.ToString();
                CabSearchtextBox.Text = row.Cells["Cabinet"].Value.ToString();
                textBox3.Text = row.Cells["Platform"].Value.ToString();
                DepttextBox.Text = row.Cells["Department"].Value.ToString();
                LeadtextBox.Text = row.Cells["Leader"].Value.ToString();
            }
        }

        private void Popbutton_Click(object sender, EventArgs e)
        {
            backgroundWorkerUpdater.RunWorkerAsync();
            Popbutton.Enabled = false;
        }
        private void backgroundWorkerUpdater_DoWork(object sender, DoWorkEventArgs e)
        {
            string storedProcedureUpdate = "UpdateMarketInfo";
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(storedProcedureUpdate, connection))

                try
                {
                    connection.Open();

                    // Set command type to stored procedure                    
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Add parameters to 'UpdateMarketInfo' stored procedure
                    command.Parameters.AddWithValue("@VERSION#", VerSearchtextBox.Text); // this value will not be updated
                    command.Parameters.AddWithValue("@MARKET", MarkSerchtextBox.Text);
                    command.Parameters.AddWithValue("@CABINET", CabSearchtextBox.Text);
                    command.Parameters.AddWithValue("@PLATFORM", textBox3.Text);
                    command.Parameters.AddWithValue("@DEPARTMENT", DepttextBox.Text);
                    command.Parameters.AddWithValue("@LEADER", LeadtextBox.Text);

                    // Add an output parameter for capturing success flag
                    SqlParameter successParam = new SqlParameter("@Success", System.Data.SqlDbType.Int);
                    successParam.Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(successParam);

                    // Execute the stored procedure
                    command.ExecuteNonQuery();

                    // Retrieve the value of the output parameter
                    int successFlag = (int)successParam.Value;

                    // Return success or not
                    if (successFlag == 1)
                    {
                       MessageBox.Show("Update successful!", "Success", MessageBoxButtons.OK);
                      
                       connection.Close();

                    }
                   else
                   {
                       //Handles error reporting
                       MessageBox.Show("Oops! Something happened within the successFlag", "Stored Procedure Report.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                       connection.Close();
                       Popbutton.Enabled = true;
                        
                    }

                }

                catch (Exception ex)
                {
                    // Handle exceptions
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    connection.Close();
                    Popbutton.Enabled = true;
                }


        }

        private void backgroundWorkerUpdater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Popbutton.Enabled = true;
        }

        private void MarketText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchTrigger();
                e.SuppressKeyPress = true; // Suppress the beep sound on Enter
            }
        }

        private void VersionsSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchTrigger();
                e.SuppressKeyPress = true; // Suppress the beep sound on Enter
            }
        }

        private void VersionsSearchBox_Click(object sender, EventArgs e)
        {
            MarketText.Clear();
        }

        private void MarketText_Click(object sender, EventArgs e)
        {
            VersionsSearchBox.Clear();
        }
    }
   
    }
