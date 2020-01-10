using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace NSchedule.Helpers
{
    /// <summary>
    /// This class is for assistance when logging in to xedule using an nhlstenden.com email.
    /// </summary>
    internal class Authenticator
    {
        private HttpClient _http = null;
        private string cookies = "";
        public Authenticator()
        {
        }

        /// <summary>
        /// This method checks whether the user has their authentication data stored.
        /// </summary>
        /// <returns>Whether login data is remembered.</returns>
        public bool AreValuesStored()
        {
            // We'll just run this method synchronously, nobody gets hurt.
            var user = SecureStorage.GetAsync("email").GetAwaiter().GetResult();
            var pass = SecureStorage.GetAsync("password").GetAwaiter().GetResult();

            return string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass);
        }

        /// <summary>
        /// Tries to log in with saved credentials.
        /// </summary>
        /// <returns>Returns whether the login was successful.</returns>
        public bool TryLogin()
        {
            var email = SecureStorage.GetAsync("email").GetAwaiter().GetResult();
            var password = SecureStorage.GetAsync("password").GetAwaiter().GetResult();

            return Authenticate(email, password);
        }

        /// <summary>
        /// Tries to log in with a username and password.
        /// </summary>
        /// <param name="username">Current username.</param>
        /// <param name="password">Current password.</param>
        /// <param name="rememberme">Whether to remember login details</param>
        /// <returns>Returns whether the login was successful.</returns>
        public bool TryLogin(string email, string password, bool rememberme)
        {
            if (rememberme)
            {
                SecureStorage.SetAsync("email", email);
                SecureStorage.SetAsync("password", password);
            }
            return Authenticate(email, password);
        }

        /// <summary>
        /// Invalidates remembered login data.
        /// </summary>
        public void InvalidateLoginData()
        {
            SecureStorage.Remove("email");
            SecureStorage.Remove("password");
        }

        public HttpClient GetHttpClient()
        {
            return this._http;
        }

        private string Post(HttpClient http, string url, string content, bool assert = false)
        {
            var nvc = HttpUtility.ParseQueryString(content);
            var kvp = new List<KeyValuePair<string, string>>();
            foreach(var n in nvc.AllKeys)
            {
                kvp.Add(new KeyValuePair<string, string>(n, nvc[n]));
            }

            if (assert)
            {
                http.DefaultRequestHeaders.Add("Referer", "https://engine.surfconext.nl/authentication/sp/consume-assertion");
                http.DefaultRequestHeaders.Add("Origin", "https://engine.surfconext.nl");
            }

            var req = http.PostAsync(url, new FormUrlEncodedContent(kvp))
                    .GetAwaiter().GetResult();

            var headers = req.RequestMessage.Headers.ToString();

            var h = req.Headers.ToString();
            if (assert)
            {
                this.cookies = req.Headers.GetValues("Set-Cookies").ToString();
            }

            return req.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private HttpResponseMessage GetRedirected(HttpClient http, string url)
        {
            var uri = new Uri(url);
            var resp = http.GetAsync(uri).GetAwaiter().GetResult();

            if((int)resp.StatusCode >= 300 && (int)resp.StatusCode <= 399)
            {
                var redirectUri = new Uri(uri.GetLeftPart(UriPartial.Authority) + resp.Headers.Location.OriginalString);
                return GetRedirected(http, redirectUri.ToString());
            }

            return resp;
        }

        private bool Authenticate(string email, string password)
        {
            var authendpoint = "";

            // Check login type and set auth endpoint accordingly
            if (email.EndsWith("@nhlstenden.com") || email.EndsWith("@student.nhlstenden.com"))
            {
                authendpoint = Constants.NHL_STENDEN_AUTH_ENDPOINT;
            }
            else if (email.EndsWith("@stenden.com") || email.EndsWith("@student.stenden.com"))
            {
                authendpoint = Constants.STENDEN_AUTH_ENDPOINT;
            }
            else
            {
                // Invalid login name
                Toast.MakeText(Application.Context, "Invalid username. Contact Naamloos if this is incorrect.", ToastLength.Long).Show();
                return false;
            }

            // Next step, make http handler, http client and cookie container.
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true, 
                CookieContainer = new CookieContainer()
            };
            this._http = new HttpClient(handler);
            this._http.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.USER_AGENT);

            // Getting SAML auth paths from auth page.
            var authpage = GetRedirected(this._http, authendpoint);
            var authcontent = authpage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            // using a regular expression to strip the saml auth url from the html page.
            var saml = new Regex(Constants.AUTH_REGEX).Match(authcontent).Groups[1].Value;

            // Now that we've got the saml auth path, let's try to authenticate
            var samlstring = Post(this._http, saml, $"UserName={email}&Password={password}&Kmsi=true&AuthMethod=FormsAuthentication");
            var samlgroups = new Regex(Constants.SAML_RESPONSE_REGEX).Match(samlstring).Groups;
            var samlresponse = samlgroups[1].Value;

            // SAML response obtained, use it to get surf auth and relay state.
            var surfstring = Post(this._http, Constants.SURF_ENDPOINT, $"SAMLResponse={HttpUtility.UrlEncode(samlresponse)}");

            samlresponse = new Regex(Constants.SAML_RESPONSE_REGEX).Match(surfstring).Groups[1].Value;
            var relaystate = new Regex(Constants.RELAY_STATE_REGEX).Match(surfstring).Groups[1].Value.Replace("&amp", "&");

            // Finally authenticate. When nothing goes wrong up until after this point we can return true.
            Post(this._http, Constants.ASSERTION_ENDPOINT, $"SAMLResponse={HttpUtility.UrlEncode(samlresponse)}&return=&RelayState={HttpUtility.UrlEncode(relaystate)}", true);
            //placeholder auth fail.
            return true;
        }
    }
}