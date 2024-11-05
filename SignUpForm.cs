using MySql.Data.MySqlClient;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace voting_assi
{
    public partial class SignUpForm : Form
    {
        Boolean FLAG = true;
        public SignUpForm()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtName.Clear();
            txtPass.Clear();
            txtPN.Clear();
            txtVoterID.Clear();
        }

        private void llbLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Hide();
        }

        private void btnSignIn_Click(object sender, EventArgs e)
        {
            FLAG = validateFields();
            if (FLAG == false)
            {
                int count=0;
                String connectionString = "datasource=localhost;port=3306;username=root;password=\"\";database=voting_system;";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand command = new MySqlCommand();
                try
                {
                    connection.Open();
                    command.CommandText = "SELECT COUNT(*)FROM voterstable WHERE VotersID=@idToCheck;";
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@idToCheck", txtVoterID.Text);
                    MySqlDataReader reader = command.ExecuteReader();
                    reader.Read();
                    count = reader.GetInt32(0);
                    reader.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error " + ex.Message, "Login Form", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    errorProvider1.Clear();
                }
                if (count == 0)
                {
                    command.CommandText = "INSERT INTO `voterstable` (`VotersID`, `Name`, `Phoneno`, `Password`) VALUES ('" + txtVoterID.Text + "', '" + txtName.Text + "', '" + txtPN.Text + "', '" + txtPass.Text + "');";
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                    connection.Close();

                    MessageBox.Show("SignUp Succesfull head to login");
                    LoginForm loginForm = new LoginForm();
                    loginForm.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("This ID already has been registered");
                    errorProvider1.Clear();
                }
            }
        }
        private Boolean validateFields()
        {
            Boolean isIntString = txtPN.Text.All(char.IsDigit);
            errorProvider1.Clear();
            FLAG = false;
            var txtbox = new[] { txtName, txtPass, txtPN, txtVoterID };
            foreach (var control in txtbox.Where(e => string.IsNullOrEmpty(e.Text)))
            {
                errorProvider1.SetError(control, "Please enter complete details");
                FLAG = true;
            }
            if (clbAgree.CheckedItems.Count < 1)
            {
                errorProvider1.SetError(clbAgree, "Please Agree to the terms and conditions");
                FLAG = true;
            }
            if (txtVoterID.Text.Length < 10) //&& txtVoterID.Text.Length > 10)
            {
                errorProvider1.SetError(txtVoterID, "ID should be of 10 characters");
                FLAG = true;
            }
            if (txtPN.Text.Length < 10 || txtPN.Text.Length > 10)
            {
                errorProvider1.SetError(txtPN, "Phone number should be of 10 numbers");
                FLAG = true;
            }
            if (isIntString == false)
            {
                errorProvider1.SetError(txtPN, "Phone number should contain only numbers");
                FLAG = true;
            }
            return FLAG;
        }
    }
}
