using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

// Many thanks to LuukEsselbrugge for making an old version of his SRooster code available on GitHub.
// This saved a lot of time researching how xedule authentication and xedule's api works.
namespace NSchedule.Helpers
{
    public class RestHelper
    {
        private HttpClientHandler _handler;
        private HttpClient _http;
        private CookieContainer _cookies;
        private List<Cookie> _koekjes;

        private bool _authenticated;

        private string _sessionCookie = null;
        private string _userCookie = null;

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
            _koekjes = new List<Cookie>();
        }

        public void TryGetTheDocentenHaha()
        {
            var resp = _http.GetAsync(Constants.API_ENDPOINT + "/docent").GetAwaiter().GetResult();
            var content = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private async Task SetCookieHeader()
        {

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

            _koekjes.AddRange(_cookies.GetCookies(uri).Cast<Cookie>().ToList());
            return resp;
        }

        private async Task<HttpResponseMessage> postAsync(string url, FormUrlEncodedContent form)
        {
            var uri = new Uri(url);
            _http.DefaultRequestHeaders.Remove("Cookie");
            _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            var resp = await _http.PostAsync(uri, form);

            var hh = resp.Headers.ToString();
            var h = resp.RequestMessage.Headers.ToString();

            _koekjes.AddRange(_cookies.GetCookies(uri).Cast<Cookie>().ToList());
            return resp;
        }

        private string generateCookieString()
        {
            return string.Join("; ", _koekjes.Select(x => x.Name + "=" + x.Value));
        }

        private string scrapeText(string regexstring, string html)
        {
            var regex = new Regex(regexstring);
            var matches = regex.Match(html);

            return matches.Groups[1].Value;
        }

        public async Task<bool> authenticate(string username, string password)
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
            var samlpath = scrapeText(Constants.AUTH_REGEX, loginpage);

            // Trying to authenticate and get saml response
            var samlform = new Dictionary<string, string>();
            samlform.Add("UserName", username);
            samlform.Add("Password", password);
            samlform.Add("Kmsi", "true");
            samlform.Add("AuthMethod", "FormsAuthentication");
            var samlpage = await (await postAsync(samlpath, new FormUrlEncodedContent(samlform))).Content.ReadAsStringAsync();
            var samlresponse = scrapeText(Constants.SAML_RESPONSE_REGEX, samlpage);

            // trying to get surf auth and relay state
            var surfform = new Dictionary<string, string>();
            surfform.Add("SAMLResponse", samlresponse);
            var surfresponse = await (await postAsync(Constants.SURF_ENDPOINT, new FormUrlEncodedContent(surfform))).Content.ReadAsStringAsync();
            samlresponse = scrapeText(Constants.SAML_RESPONSE_REGEX, surfresponse);
            var relaystate = scrapeText(Constants.RELAY_STATE_REGEX, surfresponse);
            relaystate = relaystate.Replace("&amp", "&");

            // Authenticating
            var assertform = new Dictionary<string, string>();
            assertform.Add("SAMLResponse", samlresponse);
            assertform.Add("return", "");
            assertform.Add("RelayState", relaystate);

            var auth = await postAsync(Constants.ASSERTION_ENDPOINT, new FormUrlEncodedContent(assertform));
            var authcontent = await auth.Content.ReadAsStringAsync();
            var postcontent = await auth.RequestMessage.Content.ReadAsStringAsync();

            return true;
        }
    }
}