using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PosApp
{
    public class PasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32; // 256-bit
        private const int Iterations = 10000; // Recommended iteration count

        public static string HashPassword(string password)
        {
            // Generate a random salt
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                // Hash the password with the salt using PBKDF2
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] key = pbkdf2.GetBytes(KeySize);

                    // Combine salt and key into one byte array for storage
                    byte[] hashBytes = new byte[SaltSize + KeySize];
                    Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                    Array.Copy(key, 0, hashBytes, SaltSize, KeySize);

                    return Convert.ToBase64String(hashBytes);
                }
            }
        }




        public static bool VerifyPassword(string password, string storedHash)
        {
            // Decode the stored hash
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extract the salt and key from the stored hash
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            byte[] storedKey = new byte[KeySize];
            Array.Copy(hashBytes, SaltSize, storedKey, 0, KeySize);

            // Hash the provided password with the extracted salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = pbkdf2.GetBytes(KeySize);

                // Compare the computed hash with the stored hash
                return CryptographicOperations.FixedTimeEquals(key, storedKey);
            }
        }
    }



}//namespace
