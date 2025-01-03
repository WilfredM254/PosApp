using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace PosApp
{

    

    public partial class AddUserWindow : Window
    {
        public ObservableCollection<Staff> StaffList { get; set; }

        public AddUserWindow()
        {
            InitializeComponent();

            StaffList = new ObservableCollection<Staff>();
            LoadStaffMembers();
            DataContext = this;
        }







        private void LoadStaffMembers()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT StaffID, FullName FROM staff";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            StaffList.Add(new Staff
                            {
                                StaffID = reader.GetInt32("StaffID"),
                                FullName = reader.GetString("FullName")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff members: {ex.Message}");
            }
        }







        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Get input values
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();


            // Validation: Check if any field is empty
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ErrorMessage.Text = "Please fill in all fields.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }



            string staffID = "";
            var selectedStaff = StaffMember_ComboBox.SelectedItem as Staff; // 
            if (selectedStaff != null)
            {
                staffID = selectedStaff.FullName; // 
            }
            else
            {
                ErrorMessage.Text = "An existing staff member is required";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }


            if (username.Length < 5  || username.Length > 32)
            {
                ErrorMessage.Text = "Username should be 5 - 32 characters";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }



            if (password.Length < 8)
            {
                ErrorMessage.Text = "Password should atleast 8 characters";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }


            try
            {
                // Database connection

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Check if the user already exists
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@Username", username);

                    int userExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (userExists > 0)
                    {
                        ErrorMessage.Text = "Username already exists.";
                        ErrorMessage.Visibility = Visibility.Visible;
                        return;
                    }



                    string hashedPassword = PasswordHasher.HashPassword(password);



                    // If user doesn't exist, proceed to add the new user
                    string insertQuery = "INSERT INTO Users (staffid, Username, Password, Role) VALUES (@StaffID, @Username, @hashedPassword, @Role)";
                    MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);

                    // Add parameters to prevent SQL injection
                    insertCommand.Parameters.AddWithValue("@StaffID", staffID);
                    insertCommand.Parameters.AddWithValue("@Username", username);
                    insertCommand.Parameters.AddWithValue("@hashedPassword", hashedPassword); // Consider hashing the password
                    insertCommand.Parameters.AddWithValue("@Role", role);
                    

                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("User added successfully!");
                        this.Close(); // Close the window after adding the user
                    }
                    else
                    {
                        ErrorMessage.Text = "An error occurred while adding the user.";
                        ErrorMessage.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = "Error: " + ex.Message;
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }//AddUserButton_Click







        private void Username_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string Username = UsernameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(Username))
            {
                if (e.Key == Key.Enter)
                {
                    PasswordBox.Focus();
                }
            }

        }





        private void Pwd_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string Pwd = PasswordBox.Password;

            if (!string.IsNullOrWhiteSpace(Pwd))
            {
                if (e.Key == Key.Enter)
                {
                    StaffMember_ComboBox.Focus();
                }
            }

        }





    }
}
