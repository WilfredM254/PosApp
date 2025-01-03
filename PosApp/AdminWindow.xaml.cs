using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using PosApp.ViewModels;

namespace PosApp
{
    public partial class AdminWindow : Window
    {
        private MainViewModel _viewModel;

        public AdminWindow()
        {
            InitializeComponent();
            LoadProducts();
            LoadUsers();
            LoadStaffMembers();
            DataContext = this;

            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }






        /// <summary>
        /// Load products into the DataGrid. 
        /// </summary>
        private void LoadProducts()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT Id, name AS Product, price As Price, stock AS Quantity FROM products";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridProducts.ItemsSource = dt.DefaultView;
            }
        }




        private void LoadUsers()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT id AS UserID, username AS Username, role AS Role, status AS Status, staffid AS Owner, DATE_FORMAT(date_added, '%Y-%m-%d') AS Date_added FROM users";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridUsers.ItemsSource = dt.DefaultView;
            }
        }







        public void LoadStaffMembers()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT StaffID, FullName, NationalID, Phone, Email, DATE_FORMAT(DateOfBirth, '%Y-%m-%d') AS DateOfBirth, HomeAddress, DATE_FORMAT(DateAdded, '%Y-%m-%d') AS DateAdded FROM staff";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGrid_StaffMembers.ItemsSource = dt.DefaultView;
            }
        }






        // Handle when a product is selected
        private void ProductsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dataGridProducts.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)dataGridProducts.SelectedItem;
                txtProductID.Text = selectedRow["id"].ToString();
                txtProductName.Text = selectedRow["Product"].ToString();
                txtProductPrice.Text = selectedRow["Price"].ToString();
                txtProductStock.Text = selectedRow["Quantity"].ToString();
            }
        }






        public static string MakeFirstCharacterUppercaseEachWord(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower());  // Convert to title case
        }





        /// <summary>
        /// Add a new product to the database.
        /// </summary>
        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            string prodname = MakeFirstCharacterUppercaseEachWord(txtProductName.Text);
            if (string.IsNullOrWhiteSpace(prodname))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Product Name required.";
                return;
            }

            if (!decimal.TryParse(txtProductPrice.Text, out decimal price) || !int.TryParse(txtProductStock.Text, out int stock))
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Enter valid price and stock.";
                return;
            }



            if (price < 0)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Invalid price value.";
                return;
            }


            if (stock < 0)
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Invalid quantity value.";
                return;
            }




            using (var connection = DatabaseHelper.GetConnection())
            { 
                        connection.Open();


                        // Check if the user already exists
                        string checkQuery = "SELECT COUNT(*) FROM products WHERE name = @prodname";
                        MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                        checkCommand.Parameters.AddWithValue("@prodname", prodname);

                        int prodExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (prodExists > 0)
                        {
                            ErrorMessageTextBlock.Visibility = Visibility.Visible;
                            ErrorMessageTextBlock.Text = "Product already exists. Update";
                            return;
                        }


             }






            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO products (name, price, stock) VALUES (@prodname, @price, @stock)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@prodname", prodname);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@stock", stock);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearProductForm();
            LoadProducts();
            txtProductName.Focus();
            ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
        }










        /// <summary>
        /// Edit the selected product in the database.
        /// </summary>
        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridProducts.SelectedItem is DataRowView selectedRow)
            {
                int id = Convert.ToInt32(selectedRow["id"]);
                string name = txtProductName.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                    ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    ErrorMessageTextBlock.Text = "Product Name required.";
                    return;
                }



                if (!decimal.TryParse(txtProductPrice.Text, out decimal price) || !int.TryParse(txtProductStock.Text, out int stock))
                {
                    MessageBox.Show("Please enter valid price and stock values.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (price < 0)
                {
                    ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    ErrorMessageTextBlock.Text = "Invalid price value.";
                    return;
                }


                if (stock < 0)
                {
                    ErrorMessageTextBlock.Visibility = Visibility.Visible;
                    ErrorMessageTextBlock.Text = "Invalid quantity value.";
                    return;
                }


                var result = MessageBox.Show("Are you sure you want to EDIT this product?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        string query = "UPDATE products SET name = @name, price = @price, stock = @stock WHERE id = @id";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@stock", stock);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearProductForm();
                    LoadProducts();
                    ErrorMessageTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Please select a product to edit.";
                return;
            }
        }






       private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the AddUserWindow
            AddUserWindow addUserWindow = new AddUserWindow();

            // Show the AddUserWindow as a modal dialog
            addUserWindow.ShowDialog();
        }







        private void AddStaffMembersButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the AddUserWindow
            AddStaffWindow addStaffWindow = new AddStaffWindow();

            // Show the AddUserWindow as a modal dialog
            addStaffWindow.ShowDialog();
        }






        private void RefreshStaffMembersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStaffMembers();
        }




        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Open the LoginWindow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close the current window
            this.Close();
        }





        /// <summary>
        /// Delete the selected product from the database.
        /// </summary>
        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridProducts.SelectedItem is DataRowView selectedRow)
            {
                int id = Convert.ToInt32(selectedRow["id"]);

                var result = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        string query = "DELETE FROM products WHERE id = @id";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Generate sales reports for the selected date range.
        /// </summary>
        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a valid date range.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = dpStartDate.SelectedDate.Value;
            DateTime endDate = dpEndDate.SelectedDate.Value.AddDays(1).AddSeconds(-1); ;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT id AS SaleID, total_amount AS Total, payment AS Paid,change_amount as ChangeAmount, sale_date AS Date FROM sales WHERE sale_date BETWEEN @startDate AND @endDate ORDER BY id DESC";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridSalesReports.ItemsSource = dt.DefaultView;
            }
        }

        /// <summary>
        /// Clear the product form inputs.
        /// </summary>
        private void ClearProductForm()
        {
            txtProductName.Clear();
            txtProductPrice.Clear();
            txtProductStock.Clear();
        }

        private void RefreshUserButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}
