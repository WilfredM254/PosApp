using System;
using System.Windows;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlX.XDevAPI.Common;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PosApp
{
    public partial class AddStaffWindow : Window
    {
        public AddStaffWindow()
        {
            InitializeComponent();
            PopulateCountryComboBox();
        }






        private void PopulateCountryComboBox()
        {
            // List of countries (this is a partial list; include all countries as needed)
            List<string> countries = new List<string>
            {
                "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda",
                "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain",
                "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan", "Bolivia",
                "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso",
                "Burundi", "Cabo Verde", "Cambodia", "Cameroon", "Canada", "Central African Republic",
                "Chad", "Chile", "China", "Colombia", "Comoros", "Congo (Congo-Brazzaville)",
                "Congo (Congo-Kinshasa)", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czechia (Czech Republic)",
                "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador",
                "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland", "France",
                "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada", "Guatemala", "Guinea",
                "Guinea-Bissau", "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia",
                "Iran", "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan",
                "Kenya", "Kiribati", "Korea, North", "Korea, South", "Kuwait", "Kyrgyzstan", "Laos", "Latvia",
                "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar",
                "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius",
                "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique",
                "Myanmar (formerly Burma)", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand", "Nicaragua",
                "Niger", "Nigeria", "North Macedonia (formerly Macedonia)", "Norway", "Oman", "Pakistan", "Palau",
                "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar",
                "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines",
                "Samoa", "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia", "Seychelles",
                "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa",
                "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Taiwan",
                "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia",
                "Turkey", "Turkmenistan", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom",
                "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City (Holy See)", "Venezuela", "Vietnam",
                "Yemen", "Zambia", "Zimbabwe"
            };

            // Sort the list alphabetically
            countries.Sort();

            // Add each country to the ComboBox
            foreach (var country in countries)
            {
                HomeCountryComboBox.Items.Add(country);
            }
        }











        // Save Button Click Handler
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Get input values
            string fullName = MakeFirstCharacterUppercaseEachWord(FullNameTextBox.Text.Trim());
            string nationalID = NationalIDTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim().ToLower();
            DateTime? dob = DOBDatePicker.SelectedDate;
            string homeAddress = MakeFirstCharacterUppercaseEachWord(HomeAddressTextBox.Text.Trim());
            string homeCountry = MakeFirstCharacterUppercaseEachWord(HomeCountryComboBox.Text);

            // Input validation
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(nationalID) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) ||
                dob == null || string.IsNullOrWhiteSpace(homeAddress) ||
                string.IsNullOrWhiteSpace(homeCountry))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please fill out all fields.";
                return;
            }


            if (fullName.Length < 5)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please enter Full Name.";
                return;
            }



            if (nationalID.Length <= 6)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please valid National ID.";
                return;
            }




            if (phone.Length < 10 || !(int.TryParse(phone, out int result)))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please valid Phone No.";
                return;
            }



            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (email.Length < 5 || !(Regex.IsMatch(email, pattern)))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please valid Email Address!";
                return;
            }


            
            if (!dob.HasValue)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please enter DoB.";
                return;
            }



            if (homeAddress.Length < 5)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please enter valid Home Address.";
                return;
            }


            
            if (string.IsNullOrWhiteSpace(homeCountry))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please select country.";
                return;
            }



            using (var connection = DatabaseHelper.GetConnection())
                try
            {
                
                {
                    connection.Open();


                        // Check if the user already exists
                        string checkQuery = "SELECT COUNT(*) FROM staff WHERE email = @email";
                        MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                        checkCommand.Parameters.AddWithValue("@email", email);

                        int emailExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (emailExists > 0)
                        {
                            ErrorMessageTextBlock.Visibility = Visibility.Visible;
                            ErrorMessageTextBlock.Text = "Email Address already exists.";
                            return;
                        }




                        string checkQuery1 = "SELECT COUNT(*) FROM staff WHERE Phone = @phone";
                        MySqlCommand checkCommand1 = new MySqlCommand(checkQuery1, connection);
                        checkCommand1.Parameters.AddWithValue("@phone", phone);

                        int phoneExists = Convert.ToInt32(checkCommand1.ExecuteScalar());

                        if (phoneExists > 0)
                        {
                            ErrorMessageTextBlock.Visibility = Visibility.Visible;
                            ErrorMessageTextBlock.Text = "Phone No. is already Registered.";
                            return;
                        }




                        string checkQuery2 = "SELECT COUNT(*) FROM staff WHERE NationalID = @nationalID";
                        MySqlCommand checkCommand2 = new MySqlCommand(checkQuery2, connection);
                        checkCommand2.Parameters.AddWithValue("@nationalID", nationalID);

                        int NationalIDExists = Convert.ToInt32(checkCommand2.ExecuteScalar());

                        if (NationalIDExists > 0)
                        {
                            ErrorMessageTextBlock.Visibility = Visibility.Visible;
                            ErrorMessageTextBlock.Text = "National ID is already registered.";
                            return;
                        }





                        string query = "INSERT INTO staff (FullName, NationalID, Phone, Email, DateOfBirth, HomeAddress, HomeCountry) VALUES (@fullName, @nationalID, @phone, @email, @dob, @homeAddress, @homeCountry)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@fullName", fullName);
                    cmd.Parameters.AddWithValue("@nationalID", nationalID);
                    cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@dob", dob);
                        cmd.Parameters.AddWithValue("@homeAddress", homeAddress);
                        cmd.Parameters.AddWithValue("@homeCountry", homeCountry);
                        cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Staff member added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Close the window after successful save

                }
            catch (Exception ex)
            {
                    ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    ErrorMessageTextBlock.Text = ex.Message;
                }
        }


        // Cancel Button Click Handler
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }






        public static string MakeFirstCharacterUppercaseEachWord(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower());  // Convert to title case
        }




        private void Fullname_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ThisVal = FullNameTextBox.Text;

            if (!string.IsNullOrWhiteSpace(ThisVal) && ThisVal.Length >= 5)
            {
                if (e.Key == Key.Enter)
                {
                    NationalIDTextBox.Focus();
                }
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Enter Full Name [Click Enter]";
            }

        }




        private void NationalID_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ThisVal = NationalIDTextBox.Text;

            if (!string.IsNullOrWhiteSpace(ThisVal) && ThisVal.Length >= 6)
            {
                if (e.Key == Key.Enter)
                {
                    PhoneTextBox.Focus();
                }
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Enter valid National ID [Click Enter]";
            }

        }




        private void Phone_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ThisVal = PhoneTextBox.Text;

            if (!string.IsNullOrWhiteSpace(ThisVal) && ThisVal.Length >= 10 && int.TryParse(ThisVal, out int result))
            {
                if (e.Key == Key.Enter)
                {
                    EmailTextBox.Focus();
                }
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Enter valid phone No. [Click Enter]";
            }

        }




        private void Email_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ThisVal = EmailTextBox.Text;
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (!string.IsNullOrWhiteSpace(ThisVal) && ThisVal.Length >= 6  && Regex.IsMatch(ThisVal, pattern))
            {
                if (e.Key == Key.Enter)
                {
                    DOBDatePicker.Focus();
                }
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Enter valid Email Address [Click Enter]";
            }

        }




        private void HomeAddress_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ThisVal = HomeAddressTextBox.Text;

            if (!string.IsNullOrWhiteSpace(ThisVal) && ThisVal.Length >= 4)
            {
                if (e.Key == Key.Enter)
                {
                    HomeCountryComboBox.Focus();
                }
                ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Enter Home Address [Click Enter]";
            }

        }





        private void Country_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ThisVal = HomeCountryComboBox.Text;

            if (!string.IsNullOrWhiteSpace(ThisVal))
            {
                if (e.Key == Key.Enter)
                {
                    SaveButton_Click(sender, new RoutedEventArgs());
                }
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Select Home Country [Click Enter]";
            }

        }





    }
}
