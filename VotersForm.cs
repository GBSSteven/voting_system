using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace voting_assi
{
    public partial class VotersForm : Form
    {
        private Image selectedCandidateImage;
        private string selectedCandidateName;
        private string selectedCandidateID;

        LoginForm LF = new LoginForm();
        public string votersID;
        static String connectionString = "datasource=localhost;port=3306;username=root;password=\"\";database=voting_system;";
        static MySqlConnection connection = new MySqlConnection(connectionString);
        MySqlCommand command = new MySqlCommand("UPDATE voterstable SET Votedfor=@CandidateID WHERE VotersID=@id;", connection);
        public VotersForm()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void btnSignout_Click(object sender, EventArgs e)
        {
            LoginForm LF = new LoginForm();
            LF.Show();
            this.Hide();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(2);
            tabControl1.TabPages["tpConfirm"].Enabled = false;
            tabControl1.TabPages["tpCandidates"].Enabled = false;
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message, "Login Form", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(0);
            tabControl1.TabPages["tpConfirm"].Enabled = false;
            tabControl1.TabPages["tpCandidates"].Enabled = true;
            command = new MySqlCommand("UPDATE voterstable SET Votedfor=@CandidateID WHERE VotersID=@id;", connection);
        }

        private void VotersForm_Load(object sender, EventArgs e)
        {
            tabControl1.TabPages["tpConfirm"].Enabled = false;
            tabControl1.TabPages["tpEnd"].Enabled = false;

            tabControl1.TabPages["tpCandidates"].Controls.Clear();
            List<Tuple<string, string, byte[]>> candidates = GetCandidates();
            int topPosition = 20;
            foreach (var candidate in candidates)
            {
                PictureBox pictureBox = new PictureBox();
                pictureBox.Left = 20;
                pictureBox.Top = topPosition;
                pictureBox.Width = 100;
                pictureBox.Height = 100;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox.Image = ByteArrayToImage(candidate.Item3);


                Label lblCandidate = new Label();
                lblCandidate.Text = $"{candidate.Item2} (ID: {candidate.Item1})";  // CandidateName (ID: CandidateID)
                lblCandidate.Left = 140;
                lblCandidate.Top = topPosition + 40;  // Position next to the image
                lblCandidate.AutoSize = true;

                Button btnVote = new Button();
                btnVote.BackColor = Color.RoyalBlue;
                btnVote.ForeColor = Color.White;
                btnVote.Text = "Vote";
                btnVote.Left = 300;
                btnVote.Top = topPosition + 40;
                btnVote.Tag = candidate.Item1;  // Store CandidateID in the button's tag
                btnVote.Click += new EventHandler(CandidateButton_Click);

                tabControl1.TabPages["tpCandidates"].Controls.Add(btnVote);
                tabControl1.TabPages["tpCandidates"].Controls.Add(pictureBox);
                tabControl1.TabPages["tpCandidates"].Controls.Add(lblCandidate);

                topPosition += 120;
            }

        }
        private void createQuery(int CandidateID, string VotersID)
        {
            try
            {
                connection.Open();
                command.Parameters.AddWithValue("@CandidateID", CandidateID);
                command.Parameters.AddWithValue("@id", VotersID);
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex.Message, "Login Form", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
        }

        private void tpCandidates_Click(object sender, EventArgs e)
        {

        }
        private List<Tuple<string, string, byte[]>> GetCandidates()
        {
            List<Tuple<string, string, byte[]>> candidates = new List<Tuple<string, string, byte[]>>();
            try
            {
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
        private void CandidateButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string candidateID = (string)clickedButton.Tag;  // Retrieve CandidateID from the button's tag

            // Retrieve the candidate's details using the candidateID from the current list of candidates
            var selectedCandidate = GetCandidates().FirstOrDefault(c => c.Item1 == candidateID);
            if (selectedCandidate != null)
            {
                selectedCandidateID = selectedCandidate.Item1;
                selectedCandidateName = selectedCandidate.Item2;
                selectedCandidateImage = ByteArrayToImage(selectedCandidate.Item3);
            }

            // Switch to the confirmation tab and enable it
            tabControl1.SelectTab(1); // Switch to the confirmation tab
            tabControl1.TabPages["tpConfirm"].Enabled = true;
            tabControl1.TabPages["tpCandidates"].Enabled = false;

            // Clear previous controls on the confirmation tab (if any) to avoid duplicate entries
            tabControl1.TabPages["tpConfirm"].Controls.Clear();

            // 1. Create and add the "Confirm Vote" label at the top center of the page
            Label lblConfirmVote = new Label
            {
                Text = "Confirm Vote",
                Font = new Font("Arial", 16, FontStyle.Bold),  // Use a larger, bold font
                AutoSize = true
            };
            // Center the label horizontally
            lblConfirmVote.Left = (tabControl1.TabPages["tpConfirm"].Width - lblConfirmVote.Width) / 2;
            lblConfirmVote.Top = 20;  // Position it at the top

            // 2. Create and add the "Are you sure you want to vote for this candidate?" label
            Label lblAreYouSure = new Label
            {
                Text = "Are you sure you want to vote for this candidate:",
                Font = new Font("Arial", 12, FontStyle.Regular),
                AutoSize = true
            };
            lblAreYouSure.Left = 20;   // Position it just above the candidate's details
            lblAreYouSure.Top = 80;

            // 3. Add the selected candidate's image
            PictureBox pictureBoxConfirm = new PictureBox
            {
                Left = 20,
                Top = 120,   // Set the position below the "Are you sure" label
                Width = 100,
                Height = 100,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = selectedCandidateImage  // Set the selected candidate's image
            };

            // 4. Add the selected candidate's name and ID with a bigger font
            Label lblCandidateConfirm = new Label
            {
                Text = $"{selectedCandidateName} (ID: {selectedCandidateID})",  // Candidate Name (ID: CandidateID)
                Font = new Font("Arial", 14, FontStyle.Bold),  // Make it larger and bold
                Left = 140,
                Top = 140,   // Position next to the image
                AutoSize = true
            };

            // 5. Add the "Yes, I am sure" button
            Button btnYesSure = new Button
            {
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Text = "Yes, I am sure",
                Left = 100,  // Position it below the candidate details
                Top = 240,
                Width = 150
            };
            btnYesSure.Click += new EventHandler(BtnYesSure_Click);

            // 6. Add the "No, go back" button
            Button btnNoGoBack = new Button
            {
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                Text = "No, go back",
                Left = 260,  // Position next to the "Yes" button
                Top = 240,
                Width = 150
            };
            btnNoGoBack.Click += new EventHandler(BtnNoGoBack_Click);

            // Add all controls to the confirmation tab
            tabControl1.TabPages["tpConfirm"].Controls.Add(lblConfirmVote);     // Add "Confirm Vote" label
            tabControl1.TabPages["tpConfirm"].Controls.Add(lblAreYouSure);      // Add "Are you sure" label
            tabControl1.TabPages["tpConfirm"].Controls.Add(pictureBoxConfirm);  // Add candidate image
            tabControl1.TabPages["tpConfirm"].Controls.Add(lblCandidateConfirm);// Add candidate name and ID
            tabControl1.TabPages["tpConfirm"].Controls.Add(btnYesSure);         // Add "Yes, I am sure" button
            tabControl1.TabPages["tpConfirm"].Controls.Add(btnNoGoBack);        // Add "No, go back" button
        }
        // "Yes, I am sure" button click handler
        private void BtnYesSure_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if the connection is closed before opening it
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();  // Open the connection only if it's not already open
                }

                // Ensure the command has the necessary parameters for the query
                command.Parameters.Clear(); // Clear any previous parameters
                command.Parameters.AddWithValue("@CandidateID", selectedCandidateID);  // Make sure selectedCandidateID is defined earlier
                command.Parameters.AddWithValue("@id", votersID);  // Ensure votersID is set elsewhere

                // Execute the update query to finalize the vote
                command.ExecuteNonQuery();

                MessageBox.Show("Your vote has been recorded!", "Vote Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Optionally, switch to a different tab, like a thank-you page
                tabControl1.SelectTab(2); // Switch to the 'End' tab or thank-you tab after successful voting
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
            finally
            {
                // Ensure the connection is closed after the query is executed
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }


        // "No, go back" button click handler
        private void BtnNoGoBack_Click(object sender, EventArgs e)
        {
            // Switch back to the candidate selection tab without executing any SQL query
            tabControl1.SelectTab(0); // Switch back to the first tab (candidates tab)
            tabControl1.TabPages["tpConfirm"].Enabled = false;
            tabControl1.TabPages["tpCandidates"].Enabled = true;
        }




    }
}
