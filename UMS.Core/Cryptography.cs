using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace UMS.Core
{

    public static class Cryptography
    {
        #region Settings

        private const int Iterations = 2;
        private const int KeySize = 256;

        private const string Hash = "SHA1";
        private const string Salt = "aselrias38490a32";
        private const string Vector = "8947az34awl34kjq";

        #endregion

        public static string Encrypt(string value, string password)
        {
            return Encrypt<AesManaged>(value, password);
        }
        public static string Decrypt(string value, string password)
        {
            return Decrypt<AesManaged>(value, password);
        }
        private static string Encrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            var vectorBytes = Encoding.ASCII.GetBytes(Vector);
            var saltBytes = Encoding.ASCII.GetBytes(Salt);
            var valueBytes = Encoding.UTF8.GetBytes(value);

            using var cipher = new T();
            var passwordBytes =
                new PasswordDeriveBytes(password, saltBytes, Hash, Iterations);
            var keyBytes = passwordBytes.GetBytes(KeySize / 8);

            cipher.Mode = CipherMode.CBC;

            using var encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes);
            using var to = new MemoryStream();
            using var writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write);
            writer.Write(valueBytes, 0, valueBytes.Length);
            writer.FlushFinalBlock();
            var encrypted = to.ToArray();

            cipher.Clear();

            return Convert.ToBase64String(encrypted);
        }
        private static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            var valueBytes = Convert.FromBase64String(value);
            var vectorBytes = Encoding.ASCII.GetBytes(Vector);
            var saltBytes = Encoding.ASCII.GetBytes(Salt);
            byte[] decrypted;
            var decryptedByteCount = 0;

            using var cipher = new T();
            var passwordBytes = new PasswordDeriveBytes(password, saltBytes, Hash, Iterations);
            var keyBytes = passwordBytes.GetBytes(KeySize / 8);

            cipher.Mode = CipherMode.CBC;

            try
            {
                using var decrypt = cipher.CreateDecryptor(keyBytes, vectorBytes);
                using var from = new MemoryStream(valueBytes);
                using var reader = new CryptoStream(@from, decrypt, CryptoStreamMode.Read);
                decrypted = new byte[valueBytes.Length];
                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }

            cipher.Clear();
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
        public static string GetKey()
        {
            var guid = Guid.NewGuid();
            var encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded.Replace("/", "_").Replace("+", "-");
            return encoded[..22];
        }
    }
}
