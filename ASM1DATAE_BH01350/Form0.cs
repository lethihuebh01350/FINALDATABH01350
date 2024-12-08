using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASM1DATAE_BH01350
{
    public partial class Form0 : Form
    {
        
        public Form0()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnaccept_Click(object sender, EventArgs e)
        {
                                                                                                                             // Kiểm tra tên người dùng
            if (string.IsNullOrWhiteSpace(txtB_userName.Text))
            {
                MessageBox.Show("Username cannot be empty.");
                txtB_userName.Focus();
                return;                                                                                             // Dừng xử lý nếu tên người dùng không hợp lệ
            }
                                                                                                                     // Kiểm tra định dạng email
            if (!textBoxEmail.Text.Contains("@"))
            {
                MessageBox.Show("Invalid email format.");
                textBoxEmail.Focus();
                return;                                                                                                     // Dừng xử lý nếu email không hợp lệ
            }
                                                                                                                                        // Kiểm tra số điện thoại
            if (textBoxPhone.Text.Length != 10 || !textBoxPhone.Text.All(char.IsDigit))
            {
                MessageBox.Show("Phone number must be 10 digits.");
                textBoxPhone.Focus();
                return;                                                                                         // Dừng xử lý nếu số điện thoại không hợp lệ
            }


            // Kiểm tra mật khẩu
            string password = textBoxPassword.Text;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password cannot be empty. Please enter a valid password.");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Password must contain at least one letter, one number, and one special character.");
                return;
            }
            string connectionString = @"Data Source=LAPTOP-IG12I1HR\SQLEXPRESS;Initial Catalog=asm22;Integrated Security=True;TrustServerCertificate=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Lấy giới tính từ Radio Buttons
                string Gender = "";
                if (rad_male.Checked)
                {
                    Gender = "Male";
                }
                else if (rad_fema.Checked)
                {
                    Gender = "Female";
                }
                else
                {
                    MessageBox.Show("Please select gender!");
                    return;
                }

               // Thêm thông tin khách hàng vào bảng Customer

                string insertCustomerQuery = "INSERT INTO Customer (CustomerName, Gender, Birth, Phone, Email) " +
                                             "VALUES (@CustomerName, @Gender, @Birth, @Phone, @Email); " +
                                             "SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(insertCustomerQuery, connection);
                command.Parameters.AddWithValue("@CustomerName", txtB_userName.Text);
                command.Parameters.AddWithValue("@Gender", Gender);
                command.Parameters.AddWithValue("@Birth", dtp_time.Value);
                command.Parameters.AddWithValue("@Phone", textBoxPhone.Text);
                command.Parameters.AddWithValue("@Email", textBoxEmail.Text);
                                                                                                                                                                // Lấy CustomerID mới thêm
                int customerId = Convert.ToInt32(command.ExecuteScalar());

                // Thêm tài khoản đăng nhập vào bảng Users

                string salt = GenerateSalt();
                string hashedPassword = HashPassword(password, salt);
                string insertUserQuery = "INSERT INTO Users (UserName, PasswordHash, Salt, Role, CustomerID) " +
                                         "VALUES (@UserName, @PasswordHash, @Salt, 'User', @CustomerID)";

                SqlCommand userCommand = new SqlCommand(insertUserQuery, connection);
                userCommand.Parameters.AddWithValue("@UserName", textBoxCustomerName.Text);
                userCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                userCommand.Parameters.AddWithValue("@Salt", salt);
                userCommand.Parameters.AddWithValue("@CustomerID", customerId);

                userCommand.ExecuteNonQuery();
                MessageBox.Show("Register Successful!");
            }

            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
        // Hàm kiểm tra độ mạnh của mật khẩu
        private bool IsValidPassword(string password)
        {
            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecialChar = Regex.IsMatch(password, @"[\W_]");

            return hasLetter && hasDigit && hasSpecialChar;
        }

        private static string GenerateSalt()
        {
                                                                                                // Tạo mảng byte ngẫu nhiên để làm salt
            byte[] saltBytes = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
                                                                                                             // Chuyển đổi mảng byte sang chuỗi Base64
            return Convert.ToBase64String(saltBytes);
        }
        private static string HashPassword(string password, string salt)
        {
                                                                                        // Kết hợp mật khẩu với salt
            string saltedPassword = password + salt;
                                                                                             // Chuyển đổi chuỗi mật khẩu đã kết hợp thành mảng byte
            byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
                                                                                            // Sử dụng SHA256 để băm mật khẩu
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                                                                                                    // Chuyển đổi mảng byte thành chuỗi Base64
                return Convert.ToBase64String(hashBytes);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
