using Fernet;
using Plugin.Toast;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NSchedule
{
    public static class StaticMethods
    {
        public static void Toast(string message)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(() => { CrossToastPopUp.Current.ShowToastMessage(message); });
            else
                CrossToastPopUp.Current.ShowToastMessage(message);
        }

        public static async Task<string> EncryptText(string src)
        {
            var key = await GetKey();

            return _encryptString(key, src);
        }

        public static async Task<string> DecryptText(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return "";
            }

            var key = await GetKey();

            return _decryptString(key, token);
        }

        // encrypt and decrypt from:
        // https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
        private static string _encryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private static string _decryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static async Task<string> GetKey()
        {
            var key = await SecureStorage.GetAsync("encryption_key");
            if (key != null)
            {
                return key;
            }

            var newkey = new byte[16];
            RNGCryptoServiceProvider.Create().GetBytes(newkey);
            await SecureStorage.SetAsync("encryption_key", Convert.ToBase64String(newkey));
            return Convert.ToBase64String(newkey);
        }

        public static async Task SafeGotoAsync(string page)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync(page); });
            else
                await Shell.Current.GoToAsync(page);
        }

        public static void UiThreadSafeExecute(Action a)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(a);
            else
                a.Invoke();
        }

        public static async Task UiThreadSafeExecuteAsync(Task t)
        {
            if (!MainThread.IsMainThread)
                MainThread.BeginInvokeOnMainThread(async () => { await t; });
            else
                await t;
        }
    }
}
