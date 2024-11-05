using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace voting_assi
{
    public partial class AdminForm : Form
    {
        Boolean FLAG = true;
        String imageLocation = "";
        String connectionString = "datasource=localhost;port=3306;username=root;password=\"\";database=voting_system;";
        public AdminForm()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void LoadCandidatesToComboBox()
        {
            cmbCandidates.Items.Clear(); // Clear previous items

            List<Tuple<string, string>> candidates = GetCandidateIDsAndNames(); // Get IDs and Names from DB
            foreach (var candidate in candidates)
            {
                cmbCandidates.Items.Add($"{candidate.Item1} - {candidate.Item2}"); // Add to ComboBox
            }

            // Debug: Check how many candidates were loaded
            if (candidates.Count == 0)
            {
                MessageBox.Show("No candidates found in the database.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (cmbCandidates.Items.Count > 0)
            {
                cmbCandidates.SelectedIndex = 0; // Select the first item if available
            }
        }


        private List<Tuple<string, string>> GetCandidateIDsAndNames()
        {
            List<Tuple<string, string>> candidates = new List<Tuple<string, string>>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT C_Id, C_Name FROM candidates";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string candidateID = reader.GetString("C_Id");
                        string candidateName = reader.GetString("C_Name");
                        candidates.Add(new Tuple<string, string>(candidateID, candidateName));
                        //MessageBox.Show($"Loaded candidate: {candidateID} - {candidateName}"); // Debug log
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving candidates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return candidates;
        }



        private void btnDisplay_Click(object sender, EventArgs e)
        {
            ArrayList votes = new ArrayList();
            List<Tuple<string, string>> candidates = new List<Tuple<string, string>>(); // Store ID and
            int count = 0;
            tabControl1.SelectTab(1);
            try
            {
                // Fetch candidate IDs and names from the database
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT C_Id, C_Name FROM candidates", connection);
                   
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string candidateID = reader.GetString("C_Id");
                        string candidateName = reader.GetString("C_Name");
                        candidates.Add(new Tuple<string, string>(candidateID, candidateName));
                    }
                }
                // For each candidate ID, retrieve their vote count and store it in the votes list
                foreach (var candidate in candidates)
                {
                    int voteCount = executeQuery(candidate.Item1);
                    votes.Add(voteCount);
                    // Check if all vote counts are zero (to detect if voting has not started)
                    if (voteCount == 0)
                    {
                          count++;
                    }
                }
                //count++;
                // If no votes have been cast, notify the admin
                if (count == candidates.Count)
                {
                    MessageBox.Show("Voting has not been conducted yet");
                    tabControl1.SelectTab(0);
                    return;
                }
                // Find the candidate(s) with the highest votes
                int maxVotes = (int)votes[0];
                List<int> maxVoteIndices = new List<int> { 0 };
                for (int i = 1; i < votes.Count; i++)
                {
                    int currentVotes = (int)votes[i];
                    if (currentVotes > maxVotes)
                    {
                        maxVotes = currentVotes;
                        maxVoteIndices.Clear();
                        maxVoteIndices.Add(i);
                    }
                    else if (currentVotes == maxVotes)
                    {
                        maxVoteIndices.Add(i);
                    }
                }
                // Display results with candidate IDs and names
                if (maxVoteIndices.Count == 1)
                {
                    var winner = candidates[maxVoteIndices[0]];
                    txtWinnersId.Text = $"{winner.Item2} (ID: {winner.Item1})";
                }
                else
                {
                    var winnerNames = string.Join(" and ", maxVoteIndices.Select(idx =>
                    $"{candidates[idx].Item2} (ID: {candidates[idx].Item1})"));
                    txtWinnersId.Text = $"Draw between {winnerNames}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying results: " + ex.Message, "Admin Form",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void btnCount_Click(object sender, EventArgs e)
        {
            if (cmbCandidates.SelectedItem != null)
            {
                // Extract the candidate ID from the selected item (which is in the format "ID - Name")
                string selectedItem = cmbCandidates.SelectedItem.ToString();
                string selectedID = selectedItem.Split('-')[0].Trim(); // Get the candidate ID part

                try
                {
                    txtNoOfVotes.Text = (executeQuery(selectedID)).ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error " + ex.Message, "Admin Form", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a candidate from the list.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int executeQuery(string Candidateid)
        {
            
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            MySqlCommand command = new MySqlCommand("SELECT COUNT(Votedfor) FROM voterstable WHERE Votedfor=@idToCheck;", connection);
            command.Parameters.AddWithValue("@idToCheck", Candidateid);
            MySqlDataReader reader = command.ExecuteReader();
            reader.Read();
            return reader.GetInt32(0);
        }

        private void btnSignout_Click(object sender, EventArgs e)
        {
            LoginForm LF = new LoginForm();
            LF.Show();
            this.Hide();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void SaveToDatabase(byte[] imageBytes, String C_id, String C_Name)
        {
            
            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();

                // Use parameters to insert data
                MySqlCommand command = new MySqlCommand("INSERT INTO candidates (C_Id, C_Name, C_Photo) VALUES (@C_Id, @C_Name, @C_Photo);", connection);
                command.Parameters.AddWithValue("@C_Id", C_id);
                command.Parameters.AddWithValue("@C_Name", C_Name);
                command.Parameters.AddWithValue("@C_Photo", imageBytes); // Add image as parameter

                command.ExecuteNonQuery();

                MessageBox.Show("Candidate added successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.jpeg;*.bmp;*.png;*.jpg)|*.jpeg;*.bmp;*.png;*.jpg";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                imageLocation = dialog.FileName;

                // Load and display the selected image in the PictureBox
                pbPreview.Image = Image.FromFile(imageLocation);
                pbPreview.SizeMode = PictureBoxSizeMode.StretchImage; // Adjust size to fit PictureBox
            }
        }

        private void btn_addC_Click(object sender, EventArgs e)
        {
            String C_id = txtCid.Text;
            String C_Name = txtCname.Text;
            byte[] imageBytes = null;

            // Validate that fields are not empty
            if (string.IsNullOrWhiteSpace(C_id))
            {
                errorProvider1.SetError(txtCid, "Please enter a candidate ID.");
                return;
            }
            if (string.IsNullOrWhiteSpace(C_Name))
            {
                errorProvider1.SetError(txtCname, "Please enter a candidate name.");
                return;
            }
            if (string.IsNullOrEmpty(imageLocation))
            {
                errorProvider1.SetError(button1, "Please add an image.");
                return;
            }

            // Validate if candidate ID already exists in the database
            if (IsDuplicateCandidateID(C_id))
            {
                MessageBox.Show("Candidate ID already exists. Please enter a unique ID.", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Convert image to byte array
            using (FileStream fs = new FileStream(imageLocation, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    imageBytes = br.ReadBytes((int)fs.Length);
                }
            }

            // Save the candidate to the database
            SaveToDatabase(imageBytes, C_id, C_Name);

            // Clear the fields after successful insertion
            txtCid.Clear();
            txtCname.Clear();
            imageBytes = null;
            pbPreview.Image = null;
        }

        private bool IsDuplicateCandidateID(string C_id)
        {
            bool isDuplicate = false;
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM candidates WHERE C_Id = @C_Id", connection);
                command.Parameters.AddWithValue("@C_Id", C_id);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0)
                {
                    isDuplicate = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking duplicate ID: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
            return isDuplicate;
        }

        private List<Tuple<string, string, byte[]>> GetCandidates()
        {
            List<Tuple<string, string, byte[]>> candidates = new List<Tuple<string, string, byte[]>>();
            try
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();
                string query = "SELECT C_Id, C_Name, C_Photo FROM candidates";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string candidateID = reader.GetString("C_Id");
                    string candidateName = reader.GetString("C_Name");
                    byte[] candidateImage = (byte[])reader["C_Photo"];
                    candidates.Add(new Tuple<string, string, byte[]>(candidateID, candidateName, candidateImage));
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving candidates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return candidates;
        }
        private Image ByteArrayToImage(byte[] byteArray)
        {
            

            if (byteArray == null || byteArray.Length == 0)
            {
                // Return a default image or handle the case when there is no image
                return uid_assi.Properties.Resources.hidePassword; // Assuming you have a default image in your resources
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(byteArray))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Error converting image: " + ex.Message, "Image Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Return a default image if conversion fails
                return uid_assi.Properties.Resources.hidePassword;
            }
        }
        private void LoadCandidates()
        {
            tabControl1.TabPages["tpRemove"].Controls.Clear(); 
            List<Tuple<string, string, byte[]>> candidates = GetCandidates();
            int topPosition = 20;

            foreach (var candidate in candidates)
            {
                PictureBox pictureBox = new PictureBox
                {
                    Left = 20,
                    Top = topPosition,
                    Width = 100,
                    Height = 100,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = ByteArrayToImage(candidate.Item3)
                };

                Label lblCandidate = new Label
                {
                    Text = $"{candidate.Item2} (ID: {candidate.Item1})", 
                    Left = 140,
                    Top = topPosition + 40,
                    AutoSize = true
                };

                Button btnRemove = new Button
                {
                    BackColor = Color.RoyalBlue,
                    ForeColor = Color.White,
                    Text = "Remove",
                    Left = 300,
                    Top = topPosition + 40,
                    Tag = candidate.Item1 
                };
                btnRemove.Click += new EventHandler(RemoveCandidate_Click);

                tabControl1.TabPages["tpRemove"].Controls.Add(pictureBox);
                tabControl1.TabPages["tpRemove"].Controls.Add(lblCandidate);
                tabControl1.TabPages["tpRemove"].Controls.Add(btnRemove);

                topPosition += 120; 
            }

            Button btnRemoveAll = new Button
            {
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Text = "Remove All Candidates",
                Left = 20,
                Top = topPosition,
                Width = 150
            };
            btnRemoveAll.Click += new EventHandler(RemoveAllCandidates_Click);
            tabControl1.TabPages["tpRemove"].Controls.Add(btnRemoveAll);
        }
        private void RemoveCandidate_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string candidateID = (string)clickedButton.Tag; // Get the CandidateID from the button's tag

            // Confirm before deletion
            if (MessageBox.Show($"Are you sure you want to remove Candidate ID: {candidateID}?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                using (MySqlCommand command = new MySqlCommand("DELETE FROM candidates WHERE C_Id = @C_Id;", connection))
                {
                    command.Parameters.AddWithValue("@C_Id", candidateID);
                    try
                    {
                        
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                        MessageBox.Show("Candidate removed successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error removing candidate: " + ex.Message);
                    }
                }
            }
            LoadCandidates(); // Refresh the candidate list
        }

        private void RemoveAllCandidates_Click(object sender, EventArgs e)
        {
            // Confirm before deleting all candidates
            if (MessageBox.Show("Are you sure you want to remove all candidates?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                using (MySqlCommand command = new MySqlCommand("DELETE FROM candidates;", connection))
                {
                    try
                    {
                        
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                        MessageBox.Show("All candidates removed successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error removing candidates: " + ex.Message);
                    }
                }
            }
            LoadCandidates(); // Refresh the candidate list
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
           
        }

        private void tpRemove_Enter(object sender, EventArgs e)
        {
            LoadCandidates();
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
            LoadCandidatesToComboBox();
        }
    }
}
