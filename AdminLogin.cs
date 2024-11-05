using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using voting_assi;

namespace uid_assi
{
    public partial class AdminLogin : Form
    {
        Boolean FLAG = true;
        public AdminLogin()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }


        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private Boolean validateFields()
        {
            errorProvider1.Clear();
            FLAG = false;
            var txtbox = new[] { txtID, txtPass };
            foreach (var control in txtbox.Where(e => string.IsNullOrEmpty(e.Text)))
            {
                errorProvider1.SetError(control, "Please enter complete details");
                FLAG = true;
            }
            return FLAG;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            FLAG = validateFields();
            if (FLAG == false)
            {
                int count = 0;
                String password = null;
                String connectionString = "datasource=localhost;port=3306;username=root;password=\"\";database=voting_system;";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand command = new MySqlCommand();
                MySqlDataReader reader;
                try
                {
                    connection.Open();
                    command.CommandText = "SELECT COUNT(*)FROM voterstable WHERE VotersID=@idToCheck;";
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@idToCheck", txtID.Text);
                    reader = command.ExecuteReader();
                    reader.Read();
                    count = reader.GetInt32(0);
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error " + ex.Message, "Login Form", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }
                if (count > 0)
                {
                    command.CommandText = "SELECT VotersID,Password,COUNT(Votedfor) FROM voterstable WHERE VotersID=@id;";
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@id", txtID.Text);
                    reader = command.ExecuteReader();
                    reader.Read();
                    password = reader.GetString(1);
                    if (reader.GetString(0) == "Admin" && txtPass.Text == password)
                    {
                        AdminForm AF = new AdminForm();
                        AF.Show();
                        this.Hide();
                    }
                    else
                    {
                        errorProvider1.SetError(txtPass, "Wrong Password");
                    }
                    reader.Close();

                }
                else
                {
                    errorProvider1.SetError(txtID, "Invalid ID");
                }
                connection.Close();
            }

        }


        private void btnBack_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Hide();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            if (txtPass.PasswordChar == '\0')
            {
                btnShow.BringToFront();
                txtPass.PasswordChar = '*';
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            if (txtPass.PasswordChar == '*')
            {
                btnHide.BringToFront();
                txtPass.PasswordChar = '\0';
            }
        }
    }
}
