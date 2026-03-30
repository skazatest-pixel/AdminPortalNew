using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

// this provides encryption
namespace DTPortal.Common
{
    public partial class EncryptionLibrary
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public static string EncryptText(string input, string encryptionPassword, string passwordSalt)
        {
            if (string.IsNullOrEmpty(input)) return input;

            byte[] passwordBytes = Encoding.UTF8.GetBytes(encryptionPassword);
            byte[] saltBytes = Encoding.UTF8.GetBytes(passwordSalt);
            byte[] plainBytes = Encoding.UTF8.GetBytes(input);

            using var keyDerivation = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Iterations, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(KeySize);

            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            using var chaCha = new ChaCha20Poly1305(key);
            byte[] cipherText = new byte[plainBytes.Length];
            byte[] tag = new byte[TagSize];

            chaCha.Encrypt(nonce, plainBytes, cipherText, tag);

            byte[] result = new byte[NonceSize + TagSize + cipherText.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
            Buffer.BlockCopy(cipherText, 0, result, NonceSize + TagSize, cipherText.Length);

            return Convert.ToBase64String(result);
        }

        public static string DecryptText(string input, string encryptionPassword, string passwordSalt)
        {
            if (string.IsNullOrEmpty(input)) return input;

            byte[] fullCipher = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(encryptionPassword);
            byte[] saltBytes = Encoding.UTF8.GetBytes(passwordSalt);

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] cipherText = new byte[fullCipher.Length - NonceSize - TagSize];

            Buffer.BlockCopy(fullCipher, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(fullCipher, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(fullCipher, NonceSize + TagSize, cipherText, 0, cipherText.Length);

            using var keyDerivation = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Iterations, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(KeySize);

            using var chaCha = new ChaCha20Poly1305(key);
            byte[] plainBytes = new byte[cipherText.Length];

            try
            {
                chaCha.Decrypt(nonce, cipherText, tag, plainBytes);
            }
            catch (CryptographicException)
            {
                throw new SecurityException("Data integrity check failed.");
            }

            return Encoding.UTF8.GetString(plainBytes);
        }

        public static class KeyGenerator
        {
            public static string GetUniqueKey(int maxSize = 36)
            {
                return Convert.ToHexString(RandomNumberGenerator.GetBytes(maxSize / 2));
            }
        }
        //public static byte[] AESEncrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes, byte[] saltBytes)
        //{
        //    byte[] encryptedBytes = null;

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (RijndaelManaged aes = new RijndaelManaged())
        //        {
        //            aes.KeySize = 256;
        //            aes.BlockSize = 128;

        //            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
        //            aes.Key = key.GetBytes(aes.KeySize / 8);
        //            aes.IV = key.GetBytes(aes.BlockSize / 8);

        //            aes.Mode = CipherMode.CBC;

        //            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
        //                cs.Close();
        //            }
        //            encryptedBytes = ms.ToArray();
        //        }
        //    }

        //    return encryptedBytes;
        //}

        //public static byte[] AESDecrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes, byte[] saltBytes)
        //{
        //    byte[] decryptedBytes = null;

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (RijndaelManaged aes = new RijndaelManaged())
        //        {
        //            aes.KeySize = 256;
        //            aes.BlockSize = 128;

        //            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
        //            aes.Key = key.GetBytes(aes.KeySize / 8);
        //            aes.IV = key.GetBytes(aes.BlockSize / 8);

        //            aes.Mode = CipherMode.CBC;

        //            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
        //                cs.Close();
        //            }
        //            decryptedBytes = ms.ToArray();
        //        }
        //    }

        //    return decryptedBytes;
        //}

        //public static string EncryptText(string input, string encryptionPassword, string passwordSalt)
        //{
        //    // Get the bytes of the string
        //    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(encryptionPassword);

        //    // Hash the password with SHA256
        //    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        //    byte[] saltBytes = Encoding.UTF8.GetBytes(passwordSalt);

        //    byte[] bytesEncrypted = AESEncrypt(bytesToBeEncrypted, passwordBytes, saltBytes);

        //    string result = Convert.ToBase64String(bytesEncrypted);

        //    return result;
        //}

        //public static string DecryptText(string input, string encryptionPassword, string passwordSalt)
        //{
        //    // Get the bytes of the string
        //    byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(encryptionPassword);
        //    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        //    // Hash the password with SHA256
        //    byte[] saltBytes = Encoding.UTF8.GetBytes(passwordSalt);

        //    byte[] bytesDecrypted = AESDecrypt(bytesToBeDecrypted, passwordBytes, saltBytes);

        //    string result = Encoding.UTF8.GetString(bytesDecrypted);

        //    return result;
        //}

        public static void GenerateUniqueKey(out string clientID, out string clientSecert)
        {
            clientID = KeyGenerator.GetUniqueKey();
            clientSecert = KeyGenerator.GetUniqueKey();
        }

        //public static string GenerateToken(ClientKey clientKey, DateTime issuedOn, string password, string passwordSalt)
        //{
        //    try
        //    {
        //        string randomnumber =
        //           string.Join(":", new string[]
        //           {
        //               Convert.ToString(clientKey.ClientId),
        //               KeyGenerator.GetUniqueKey(),
        //               Convert.ToString(issuedOn.Ticks),
        //               clientKey.ClientId
        //           });

        //        return EncryptionLibrary.EncryptText(randomnumber, password, passwordSalt);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public static class KeyGenerator
        //{
        //    public static string GetUniqueKey(int maxSize = 36)
        //    {
        //        char[] chars = new char[62];
        //        chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        //        byte[] data = new byte[1];
        //        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        //        {
        //            crypto.GetNonZeroBytes(data);
        //            data = new byte[maxSize];
        //            crypto.GetNonZeroBytes(data);
        //        }
        //        StringBuilder result = new StringBuilder(maxSize);
        //        foreach (byte b in data)
        //        {
        //            result.Append(chars[b % (chars.Length)]);
        //        }
        //        return result.ToString();
        //    }
        //}
    }
}
