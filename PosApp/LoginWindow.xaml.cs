using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using PosApp;
using PosApp.ViewModels;


namespace PosApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }







        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;




            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    MessageBox.Show("Account is UnAvailable!");
                    return;
                }
            }





            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM users WHERE username = @username";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string status = reader["status"].ToString();
                        string role = reader["role"].ToString();
                        string userid = reader["id"].ToString();
                        string storedpassword = reader["password"].ToString();


                        bool isVerified = PasswordHasher.VerifyPassword(password, storedpassword);
                        if (!isVerified)
                        {
                            MessageBox.Show("Wrong Password!");
                            return;
                        }


                        if (status != "Active")
                        {
                            MessageBox.Show("Your account is "+ status);
                            return;
                        }
                        if (role == null)
                        {
                            MessageBox.Show("Account type is Unknown");
                            return;
                        }
                        else
                        {
                            proceed_to_login(username, userid, role);
                        }

                    }
                }
            }


        }//BtnLogin_Click






        private void proceed_to_login(string username, string userid, string role)
        {

            // Create a User instance and set it in the UserSession
            UserSession.CurrentUser = new User
            {
                Username = username,
                Role = role.ToString()
            };




            using (var connection = DatabaseHelper.GetConnection())
            {
                try
                {
                    connection.Open();
                    
                            //Record login
                            string saleQuery = "INSERT INTO user_logins  (userid, username, role) VALUES (@userid, @username, @role)";
                            MySqlCommand UserloginsCommand = new MySqlCommand(saleQuery, connection);
                            UserloginsCommand.Parameters.AddWithValue("@userid", userid);
                            UserloginsCommand.Parameters.AddWithValue("@username", username);
                            UserloginsCommand.Parameters.AddWithValue("@role", role);

                            UserloginsCommand.ExecuteNonQuery();
                    
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
           }



            if (role.ToLower() == "admin")
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
            }
            else if (role.ToLower() == "cashier")
            {
                CashierWindow cashierWindow = new CashierWindow();
                cashierWindow.Show();
            }
            this.Close();


    }//proceed_to_login







        private void Username_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string Username = txtUsername.Text;

            if (!string.IsNullOrWhiteSpace(Username))
            {
                if (e.Key == Key.Enter)
                {
                    txtPassword.Focus();
                }
            }

        }





        private void Pwd_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string Username = txtUsername.Text;
            string Passwd = txtPassword.Password;

            if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Passwd))
            {
                if (e.Key == Key.Enter)
                {
                    BtnLogin_Click(sender, new RoutedEventArgs());
                }
            }

        }



    }

}