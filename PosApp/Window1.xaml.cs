using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace PosApp
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        private DataTable cartTable1;


        public Window1()
        {
            InitializeComponent();
            InitializeCart();
        }



        private void InitializeCart()
        {
            cartTable1 = new DataTable();
            cartTable1.Columns.Add("Product ID");
            cartTable1.Columns.Add("Product Name");
            cartTable1.Columns.Add("Quantity");
            cartTable1.Columns.Add("Price");
            cartTable1.Columns.Add("Subtotal");

            dataGridCart1.ItemsSource = cartTable1.DefaultView;
        }





        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }







        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            string barcode = txtBarcode.Text;
            string txt_qty = txtQty.Text;
            if (string.IsNullOrWhiteSpace(barcode))
            {
                MessageBox.Show("Please enter a valid barcode.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = "SELECT id, name, price FROM products WHERE id = @barcode";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@barcode", barcode);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string id = reader["id"].ToString();
                        string name = reader["name"].ToString();
                        decimal price = Convert.ToDecimal(reader["price"]);


                        // Check if product already exists in cart
                        DataRow existingRow = cartTable1.Rows.Cast<DataRow>()
                            .FirstOrDefault(r => r["Product ID"].ToString() == id);

                        int quantity=0;

                        if (existingRow != null)
                        {
                            quantity = Convert.ToInt32(existingRow["Quantity"]) + quantity_toadd;
                            existingRow["Quantity"] = quantity;
                            existingRow["Subtotal"] = quantity * price;
                        }
                        else
                        {
                            quantity = quantity_toadd;
                            cartTable1.Rows.Add(id, name, quantity, price, quantity * price);
                        }

                        UpdateTotal();
                    }
                    else
                    {
                        MessageBox.Show("Product not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            txtBarcode.Clear();
        }




        private void UpdateTotal()
        {
            decimal total = cartTable1.AsEnumerable().Sum(row => Convert.ToDecimal(row["Subtotal"]));
            txtTotal.Text = total.ToString("F2");
            txtPayment.Text = total.ToString("F2");
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

            decimal total = cartTable1.AsEnumerable().Sum(r => Convert.ToDecimal(r["Subtotal"]));
            if (payment < total)
            {
                MessageBox.Show("Payment is less than the total amount.", "Payment Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal change = payment - total;

            // Save transaction to database (example)
            SaveTransaction(total, payment, change);

            InitializeCart(); // Clear cart
            txtPayment.Clear();
            
            txtChange.Text = $"Change: ${change:F2}";
        }








        /// <summary>
        /// Save the transaction to the database.
        /// </summary>
        private void SaveTransaction(decimal total, decimal payment, decimal change)
        {
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
                    foreach (DataRow row in cartTable1.Rows)
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








    }
}
