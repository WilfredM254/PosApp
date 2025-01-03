using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using PosApp.ViewModels;
using System.Windows.Input;

namespace PosApp
{
    public partial class CashierWindow : Window
    {
        private DataTable cartTable;

        public CashierWindow()
        {
            InitializeComponent();
            InitializeCart();

        }





        /// <summary>
        /// Initialize the cart DataTable structure.
        /// </summary>
        private void InitializeCart()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("Product ID");
            cartTable.Columns.Add("Product Name");
            cartTable.Columns.Add("Quantity");
            cartTable.Columns.Add("Price");
            cartTable.Columns.Add("Subtotal");

            dataGridCart.ItemsSource = cartTable.DefaultView;
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
        /// Add product to the cart based on barcode input.
        /// </summary>
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            string barcode = txtBarcode.Text;
            string txt_qty = txtQty.Text;
            if (string.IsNullOrWhiteSpace(barcode))
            {
                MessageBox.Show("Please enter a valid Product ID/Barcode.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(txt_qty, out int result) || result <= 0)
            {
                // The input is a valid integer, and 'result' contains the integer value
                MessageBox.Show("Enter valid amount.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            int quantity_toadd;
            if (string.IsNullOrWhiteSpace(txt_qty))
            {
                quantity_toadd = 1; // Default quantity
            }
            else
            {
                quantity_toadd = int.Parse(txt_qty);
            }




            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT id, name, price,stock FROM products WHERE id = @barcode";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@barcode", barcode);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string id = reader["id"].ToString();
                        string name = reader["name"].ToString();
                        decimal price = Convert.ToDecimal(reader["price"]);
                        int stock = Convert.ToInt32(reader["stock"]);


                        // Check if product already exists in cart
                        DataRow existingRow = cartTable.Rows.Cast<DataRow>()
                            .FirstOrDefault(r => r["Product ID"].ToString() == id);

                        int quantity = 0;

                        if (existingRow != null)
                        {
                            quantity = Convert.ToInt32(existingRow["Quantity"]) + quantity_toadd;

                            if (stock < quantity)
                            {
                                MessageBox.Show("Insufficient stock quantity for selected Product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            existingRow["Quantity"] = quantity;
                            existingRow["Subtotal"] = quantity * price;
                        }
                        else
                        {
                            quantity = quantity_toadd;

                            if (stock < quantity)
                            {
                                MessageBox.Show("Insufficient stock quantity for selected Product.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            cartTable.Rows.Add(id, name, quantity, price, quantity * price);
                        }



                        
                            UpdateTotal();
                        

                    }
                    else
                    {
                        MessageBox.Show("Product not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            txtBarcode.Clear();
            txtBarcode.Focus();
        }



        







        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (cartTable != null)
            {
                cartTable.Clear(); // Clears all rows in the DataTable
                UpdateTotal();
                txtBarcode.Focus();
            }
            else
            {
                MessageBox.Show("Your Cart is Empty.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
                        
        }







        /// <summary>
        /// Remove selected product from the cart.
        /// </summary>
        private void BtnRemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridCart.SelectedItem is DataRowView row)
            {
                cartTable.Rows.Remove(row.Row);
                UpdateTotal();
            }
            else
            {
                MessageBox.Show("Please select a product to remove.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }







        /// <summary>
        /// Complete the sale and calculate change.
        /// </summary>
        private void BtnCompleteSale_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtPayment.Text, out decimal payment))
            {
                MessageBox.Show("Please enter a valid payment amount.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal total = cartTable.AsEnumerable().Sum(r => Convert.ToDecimal(r["Subtotal"]));
            if (payment <= 0)
            {
                MessageBox.Show("Amount paid is less than/equal to 0.", "Payment Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            
            
            if (payment < total)
            {
                MessageBox.Show("Amount paid is less than the total amount.", "Payment Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal change = payment - total;
            txtChange.Text = $"Change: ${change:F2}";

            // Save transaction to database (example)
            SaveTransaction(total, payment, change);

            InitializeCart(); // Clear cart
        }




        /// <summary>
        /// Update the total amount in the cart.
        /// </summary>
        private void UpdateTotal()
        {
            decimal total = cartTable.AsEnumerable().Sum(row => Convert.ToDecimal(row["Subtotal"]));
            txtTotal.Text = total.ToString("F2");
            txtPayment.Text = total.ToString("F2");
        }





        /// <summary>
        /// Save the transaction to the database.
        /// </summary>
        private void SaveTransaction(decimal total, decimal payment, decimal change)
        {
            foreach (DataRow row in cartTable.Rows)
            {
                int productId = Convert.ToInt32(row["Product ID"]);
                string product_name = Convert.ToString(row["Product Name"]);
                int quantity = Convert.ToInt32(row["Quantity"]);


                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT id, name, price,stock FROM products WHERE id = @productId";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@productId", productId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string id = reader["id"].ToString();
                            string name = reader["name"].ToString();
                            decimal price = Convert.ToDecimal(reader["price"]);
                            int stock = Convert.ToInt32(reader["stock"]);

                            if (stock < quantity)
                            {
                                MessageBox.Show(name+" has Insufficient stock quantity available. Update cart!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                        }
                    }
                }
            }






            using (var connection = DatabaseHelper.GetConnection())
            {
                try
                {
                    connection.Open();

                    // Insert the sale into the sales table
                    string saleQuery = "INSERT INTO sales (total_amount, payment, change_amount, cashier_id) VALUES (@total, @payment, @change, @cashierId)";
                    MySqlCommand saleCommand = new MySqlCommand(saleQuery, connection);
                    saleCommand.Parameters.AddWithValue("@total", total);
                    saleCommand.Parameters.AddWithValue("@payment", payment);
                    saleCommand.Parameters.AddWithValue("@change", change);
                    saleCommand.Parameters.AddWithValue("@cashierId", "1");

                    saleCommand.ExecuteNonQuery();

                    // Get the ID of the newly inserted sale
                    int saleId = (int)saleCommand.LastInsertedId;

                    // Insert the items into the sale_items table
                    foreach (DataRow row in cartTable.Rows)
                    {
                        int productId = Convert.ToInt32(row["Product ID"]);
                        string product_name = Convert.ToString(row["Product Name"]);
                        int quantity = Convert.ToInt32(row["Quantity"]);
                        decimal price = Convert.ToDecimal(row["Price"]);
                        decimal subtotal = Convert.ToDecimal(row["Subtotal"]);

                        string itemQuery = "INSERT INTO sale_items (sale_id, product_id, product_name, quantity, price, subtotal) VALUES (@saleId, @productId, @product_name, @quantity, @price, @subtotal)";
                        MySqlCommand itemCommand = new MySqlCommand(itemQuery, connection);
                        itemCommand.Parameters.AddWithValue("@saleId", saleId);
                        itemCommand.Parameters.AddWithValue("@productId", productId);
                        itemCommand.Parameters.AddWithValue("@product_name", product_name);
                        itemCommand.Parameters.AddWithValue("@quantity", quantity);
                        itemCommand.Parameters.AddWithValue("@price", price);
                        itemCommand.Parameters.AddWithValue("@subtotal", subtotal);

                        itemCommand.ExecuteNonQuery();


                        UpdateStock(productId, quantity);
                    }

                    txtBarcode.Focus();
                    MessageBox.Show("Sale completed and recorded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }







        private void UpdateStock(int productId, int quantitySold)
        {

            // Execute the query
            using (var connection = DatabaseHelper.GetConnection())
            {


                try
                {
                    connection.Open();

                    string itemQuery = "UPDATE products SET stock = stock - @quantitySold WHERE id = @productId";
                    MySqlCommand itemCommand = new MySqlCommand(itemQuery, connection);
                    itemCommand.Parameters.AddWithValue("@quantitySold", quantitySold);
                    itemCommand.Parameters.AddWithValue("@productId", productId);

                    itemCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }








        private void QTy_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string Quantity = txtQty.Text;

            if (string.IsNullOrWhiteSpace(Quantity))
            {
                itemSearched_Display.Text = "Please amount.";
            }

            else if (!int.TryParse(Quantity, out int result) || result <= 0)
            {
                // The input is a valid integer, and 'result' contains the integer value
                itemSearched_Display.Text = "Enter valid amount.";
            }

            else
            {
                if (e.Key == Key.Enter)
                {
                    BtnAddToCart_Click(sender, new RoutedEventArgs());
                }
            }

        }
            
            
            
            
            
            
            
            
            
            
            private void Barcode_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string ProductID = txtBarcode.Text;

            if (string.IsNullOrWhiteSpace(ProductID))
            {
                itemSearched_Display.Text = "Please enter a valid Product ID/Barcode.";
            }
            else
            {


                if (e.Key == Key.Enter)
                {
                    txtQty.Focus();
                }


                    //Check item
                    using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM products WHERE id = @ProductID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProductID", ProductID);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if(count == 0)
                    {
                        itemSearched_Display.Text = "Product not found.";
                    }
                }
                
                
                
                
                
                //Display item
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT id, name, price,stock FROM products WHERE id = @ProductID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProductID", ProductID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string id = reader["id"].ToString();
                            string name = reader["name"].ToString();

                            
                            itemSearched_Display.Text = name;
                            
                        }
                    }
                }



            }

        }




    }
}
