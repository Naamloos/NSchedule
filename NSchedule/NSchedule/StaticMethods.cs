using Fernet;
using Plugin.Toast;
using System;
using System.Collections.Generic;
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
            var key = (await GetKey()).UrlSafe64Decode();
            var src64 = src.ToBase64String();
            var encoded = SimpleFernet.Encrypt(key, src64.UrlSafe64Decode());

            return encoded;
        }

        public static async Task<string> DecryptText(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return "";
            }

            var key = await GetKey();
            var key64 = key.UrlSafe64Decode();
            var decoded64 = SimpleFernet.Decrypt(key64, token, out var timestamp);
            var decoded = decoded64.UrlSafe64Encode().FromBase64String();

            return decoded;
        }

        private static async Task<string> GetKey()
        {
            var key = await SecureStorage.GetAsync("encryption_key");
            if (key != null && key.Length == 57)
            {
                return key;
            }

            var newkey = SimpleFernet.GenerateKey();
            await SecureStorage.SetAsync("encryption_key", newkey);
            return newkey;
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
