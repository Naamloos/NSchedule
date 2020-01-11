using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NSchedule.Entities;
using Xamarin.Essentials;

// Many thanks to LuukEsselbrugge for making an old version of his SRooster code available on GitHub.
// This saved a lot of time researching how xedule authentication and xedule's api works.
namespace NSchedule.Helpers
{
    internal class RestHelper
    {
        private HttpClientHandler _handler;
        private HttpClient _http;
        private CookieContainer _cookies;
        private List<Cookie> _koekjes;

        public RestHelper()
        {
            _cookies = new CookieContainer();
            _handler = new HttpClientHandler()
            {
                CookieContainer = _cookies,
                UseCookies = true,
                AllowAutoRedirect = true
            };
            _http = new HttpClient(_handler);

            _http.DefaultRequestHeaders.Add("User-Agent", Constants.USER_AGENT);
            _http.DefaultRequestHeaders.Add("Accept", "*/*");
            _http.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _koekjes = new List<Cookie>();
        }

        public async Task<Schedule> GetScheduleAsync(int weekNumber, int year, int id, int schoolId)
        {
            var response = await getNoRedirectAsync(Constants.API_ENDPOINT + $"/schedule?ids%5B0%5D={schoolId}_{year}_{weekNumber}_{id}");
            var content = await response.Content.ReadAsStringAsync();

            // Verwerken content
            var schedule = JsonSerializer.Deserialize<Schedule>(content);

            return schedule;
        }

        public async Task<bool> ReconnectSessionAsync()
        {
            // This will check whether the saved session still works
            // TODO

            var usercookie = await SecureStorage.GetAsync("usercookie");
            var sessioncookie = await SecureStorage.GetAsync("sessioncookie");

            if(!string.IsNullOrEmpty(usercookie) && !string.IsNullOrEmpty(sessioncookie))
            {
                // Both are saved, now try to see whether these still work
                // TODO.
            }

            return false;
        }

        public async Task<bool> Authenticate(string username, string password, bool remember)
        {
            // Determine login type.
            var loginpath = "";
            if (username.EndsWith("nhlstenden.com"))
            {
                loginpath = Constants.NHL_STENDEN_AUTH_ENDPOINT;
            }
            else if (username.EndsWith("stenden.com"))
            {
                loginpath = Constants.STENDEN_AUTH_ENDPOINT;
            }
            else
            {
                return false;
            }

            // Getting SAML paths
            var loginpage = await (await getAsync(loginpath)).Content.ReadAsStringAsync();
            if(!scrapeText(Constants.AUTH_REGEX, loginpage, out string samlpath))
            {
                return false;
            }

            // Trying to authenticate and get saml response
            var samlform = new Dictionary<string, string>();
            samlform.Add("UserName", username);
            samlform.Add("Password", password);
            samlform.Add("AuthMethod", "FormsAuthentication");
            var samlpage = await (await postAsync(samlpath, new FormUrlEncodedContent(samlform))).Content.ReadAsStringAsync();
            if(!scrapeText(Constants.SAML_RESPONSE_REGEX, samlpage, out string samlresponse))
            {
                return false;
            }

            // trying to get surf auth and relay state
            var surfform = new Dictionary<string, string>();
            surfform.Add("SAMLResponse", samlresponse);
            var surfresponse = await (await postAsync(Constants.SURF_ENDPOINT, new FormUrlEncodedContent(surfform))).Content.ReadAsStringAsync();

            if(!scrapeText(Constants.SAML_RESPONSE_REGEX, surfresponse, out string samlresponse2) 
                || !scrapeText(Constants.RELAY_STATE_REGEX, surfresponse, out string relaystate))
            {
                return false;
            }
            relaystate = relaystate.Replace("&amp;", "&");

            // Authenticating
            var assertform = new Dictionary<string, string>();
            assertform.Add("SAMLResponse", samlresponse2);
            assertform.Add("RelayState", relaystate);

            var auth = await postAsync(Constants.ASSERTION_ENDPOINT, new FormUrlEncodedContent(assertform));

            if (remember)
            {
                await SecureStorage.SetAsync("usercookie", _koekjes.First(x => x.Name == "User").Value);
                await SecureStorage.SetAsync("sessioncookie", _koekjes.First(x => x.Name == "ASP.NET_SessionId").Value);
            }

            return true;
        }

        private async Task<HttpResponseMessage> getNoRedirectAsync(string url)
        {
            var uri = new Uri(url);
            _http.DefaultRequestHeaders.Remove("Cookie");
            _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            var resp = await _http.GetAsync(uri);

            updateCookies(_cookies.GetCookies(uri).Cast<Cookie>().ToList());
            return resp;
        }

        private async Task<HttpResponseMessage> getAsync(string url)
        {
            var uri = new Uri(url);
            _http.DefaultRequestHeaders.Remove("Cookie");
            _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            var resp = await _http.GetAsync(uri);

            if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode <= 399)
            {
                var redirectUri = new Uri(uri.GetLeftPart(UriPartial.Authority) + resp.Headers.Location.OriginalString);
                return await getAsync(redirectUri.ToString());
            }

            updateCookies(_cookies.GetCookies(uri).Cast<Cookie>().ToList());
            return resp;
        }

        private async Task<HttpResponseMessage> postAsync(string url, FormUrlEncodedContent form, bool nocookies = false)
        {
            Console.WriteLine("POSTING");
            var uri = new Uri(url);
            _http.DefaultRequestHeaders.Remove("Cookie");
            if (!nocookies)
                _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            var resp = await _http.PostAsync(uri, form);

            var hh = resp.Headers.ToString();
            var content = await resp.Content.ReadAsStringAsync();
            var h = resp.RequestMessage.Headers.ToString();

            if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode <= 399)
            {
                return await postAsync(resp.Headers.Location.ToString(), form, nocookies);
            }

            updateCookies(_cookies.GetCookies(uri).Cast<Cookie>().ToList());
            Console.WriteLine("DONE POST");
            return resp;
        }

        private void updateCookies(IEnumerable<Cookie> cookies)
        {
            foreach (var c in cookies)
            {
                _koekjes.RemoveAll(x => x.Name == c.Name);
                _koekjes.Add(c);
            }
        }

        private string generateCookieString()
        {
            return string.Join("; ", _koekjes.Select(x => x.Name + "=" + x.Value));
        }

        private bool scrapeText(string regexstring, string html, out string result)
        {
            var regex = new Regex(regexstring);
            var matches = regex.Match(html);
            result = "";

            if (matches.Groups.Count < 2)
            {
                return false;
            }
            result = matches.Groups[1].Value;

            return true;
        }
    }
}