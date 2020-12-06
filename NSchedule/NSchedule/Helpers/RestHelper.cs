using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NSchedule.Entities;
using System.Collections;
using System.Reflection;
using HtmlAgilityPack;

// Many thanks to LuukEsselbrugge for making an old version of his SRooster code available on GitHub.
// This saved a lot of time researching how xedule authentication and xedule's api works.
namespace NSchedule.Helpers
{
    /// <summary>
    /// Class that helps with Rest stuff for xedule.
    /// </summary>
    internal class RestHelper
    {
        /// <summary>
        /// HTTP Client Handler. (for cookies)
        /// </summary>
        private HttpClientHandler _handler;
        /// <summary>
        /// Current HTTP client.
        /// </summary>
        private HttpClient _http;
        /// <summary>
        /// Cookie container.
        /// </summary>
        private CookieContainer _cookies;
        /// <summary>
        /// All cookies to gen a cookie string.
        /// </summary>
        private List<string> _koekjes;
        /// <summary>
        /// Database connection (for settings and caching)
        /// </summary>
        private Database _db;

        /// <summary>
        /// Constructs a new RestHelper.
        /// </summary>
        /// <param name="db">Databse connection.</param>
        public RestHelper(Database db)
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
            _koekjes = new List<string>();
            _db = db;
        }

        /// <summary>
        /// Authenticates this client.
        /// </summary>
        /// <param name="username">Email for user.</param>
        /// <param name="password">Password for user</param>
        /// <returns>Whether authentication was succesful, with a status message.</returns>
        public async Task<(bool Success, string Message)> Authenticate(string username, string password)
        {
            this._koekjes.Clear();
            // Determine login type.
            string loginpath;
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
                return (false, "You tried to log in with a non-stenden email address! This is not allowed.");
            }

            // Getting SAML paths
            var samlpath = await getSamlPathAsync(loginpath).ConfigureAwait(false);
            if (samlpath == null)
            {
                return (false, "Wait... something went wrong. Did NHL Stenden change their login?");
            }

            // Trying to authenticate and get saml response
            var samlresponse = await getSamlResponseAsync(samlpath, username, password).ConfigureAwait(false);
            if (samlresponse == null)
            {
                return (false, "Invalid credentials!");
            }

            // trying to get surf auth and relay state

            var surf = await getSurfAsync(samlresponse).ConfigureAwait(false);
            if (surf.samlresponse == null || surf.relaystate == null)
            {
                return (false, "Something went really really wrong behind the scenes!");
            }
            surf.relaystate = surf.relaystate.Replace("&amp;", "&");

            // Authenticating
            var auth = await authenticateAsync(surf.samlresponse, surf.relaystate).ConfigureAwait(false);

            var settings = await _db.GetSettingsAsync().ConfigureAwait(false);
            settings.CookieString = generateCookieString();
            settings.Email = username;
            settings.Password = password;
            await _db.SetSettingsAsync(settings).ConfigureAwait(false);

            if (auth)
                return (true, "Sign in successful!");
            else
                return (false, "Oops, surf error??");
        }

        /// <summary>
        /// Checks whether cookies still work, and tries to reconnect with cached username/password combo.
        /// </summary>
        /// <returns></returns>
        public async Task<(bool Success, string Message)> CheckAndReconnectSessionAsync()
        {
            // This will check whether the saved session still works
            var settings = await _db.GetSettingsAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(settings.CookieString))
            {
                splitCookieString(settings.CookieString);
                // Do een request om te testen of cookies valid zijn
                var response = await getAsync(Constants.API_ENDPOINT + $"/year", settings.CookieString).ConfigureAwait(false);

                var forbidden = response.StatusCode == HttpStatusCode.Forbidden;
                if (!forbidden)
                {
                    // YES! sessie nog valid. we stellen gewoon lekker deze koekjes weer in.
                    return (true, "Re-auth via stored cookie success!");
                }
            }

            // als invalid koekjes, probeer nog 1x in te loggen met oude auth..
            var auth = await this.Authenticate(settings.Email, settings.Password).ConfigureAwait(false);

            // Re-auth met login OK
            if (auth.Success)
                return (true, "Re-auth via re-login success!");

            // Re-auth niet mogelijk, return false en leeg koekjes.
            this._koekjes.Clear();
            return (false, "Re-auth failed!");
        }

        /// <summary>
        /// Resets this class.
        /// </summary>
        public void Reset()
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
            _koekjes = new List<string>();
        }

        // TODO ERRORS OPVANGEN LIKE A BITCH

        /// <summary>
        /// Gets a calendar.
        /// </summary>
        /// <param name="school">School id. (probably 0?)</param>
        /// <param name="weekNumber">Week number.</param>
        /// <returns>A calendar.</returns>
        public async Task<Calendar> GetCalendarAsync(int school, int weekNumber)
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/calendar/{school}_{weekNumber}").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<Calendar>(content);
        }

        /// <summary>
        /// Gets all orginisational units.
        /// </summary>
        /// <returns>A list of organisational units.</returns>
        public async Task<List<OrganisationalUnit>> GetOrganisationalUnitsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/organisationalUnit").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<OrganisationalUnit>>(content);
        }

        /// <summary>
        /// Gets all years.
        /// </summary>
        /// <returns>A list of years.</returns>
        public async Task<List<Year>> GetYearsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/year").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<Year>>(content);
        }

        /// <summary>
        /// Gets all rooms.
        /// </summary>
        /// <returns>A list of rooms.</returns>
        public async Task<List<Room>> GetRoomsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/facility").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<Room>>(content);
        }

        /// <summary>
        /// Gets all teachers.
        /// </summary>
        /// <returns>A list of teachers.</returns>
        public async Task<List<Teacher>> GetTeachersAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/docent").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<Teacher>>(content);
        }

        /// <summary>
        /// Gets all teams.
        /// </summary>
        /// <returns>A list of teams.</returns>
        public async Task<List<Team>> GetTeamsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/team").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<Team>>(content);
        }

        /// <summary>
        /// Gets all groups.
        /// </summary>
        /// <returns>A list of groups.</returns>
        public async Task<List<Group>> GetGroupsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/group").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<Group>>(content);
        }

        /// <summary>
        /// Gets a specific schedule.
        /// </summary>
        /// <param name="schoolid">School id. (0?)</param>
        /// <param name="year">Year number. (+1?)</param>
        /// <param name="weeknum">Week number.</param>
        /// <param name="id">Teacher or Group or Room etc id.</param>
        /// <returns>A Schedule.</returns>
        public async Task<Schedule> GetScheduleAsync(long schoolid, long year, long weeknum, long id)
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/schedule/?ids%5B0%5D={schoolid}_{year}_{weeknum}_{id}").ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var results = JsonConvert.DeserializeObject<List<Schedule>>(content);

            return results.Count > 0 ? results[0] : null;
        }

        /// <summary>
        /// Authenticates with SAML response and relay state.
        /// </summary>
        /// <param name="samlresponse">SAML response.</param>
        /// <param name="relaystate">Relay state.</param>
        /// <returns>Whether auth was succesfull.</returns>
        private async Task<bool> authenticateAsync(string samlresponse, string relaystate)
        {
            var assertform = new Dictionary<string, string>();
            assertform.Add("SAMLResponse", samlresponse);
            assertform.Add("RelayState", relaystate);

            var resp = await postAsync(Constants.ASSERTION_ENDPOINT, new FormUrlEncodedContent(assertform)).ConfigureAwait(false);

            return resp.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Gets surf SAML response and relay state.
        /// </summary>
        /// <param name="samlresponse">Previous SAML response.</param>
        /// <returns>New SAML response and relay state.</returns>
        private async Task<(string samlresponse, string relaystate)> getSurfAsync(string samlresponse)
        {
            var surfform = new Dictionary<string, string>();
            surfform.Add("SAMLResponse", samlresponse);

            var surfresponse = await (await postAsync(Constants.SURF_ENDPOINT, new FormUrlEncodedContent(surfform)).ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false);
            var surfdoc = new HtmlDocument();
            surfdoc.LoadHtml(surfresponse);

            var form = surfdoc.DocumentNode.SelectSingleNode("//body/div/form");
            var samlresponse2 = form.ChildNodes[1].GetAttributeValue("value", null);
            var relaystate = form.ChildNodes[3].GetAttributeValue("value", null);

            return (samlresponse2, relaystate);
        }

        /// <summary>
        /// Gets SAML path.
        /// </summary>
        /// <param name="loginpath">Login path to use.</param>
        /// <returns>SAML path.</returns>
        private async Task<string> getSamlPathAsync(string loginpath)
        {
            var loginpage = await (await getAsync(loginpath).ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false);
            var logindoc = new HtmlDocument();
            logindoc.LoadHtml(loginpage);

            return logindoc.GetElementbyId("options").GetAttributeValue<string>("action", null);
        }

        /// <summary>
        /// Gets SAML response.
        /// </summary>
        /// <param name="samlpath">SAML path.</param>
        /// <param name="username">Email address.</param>
        /// <param name="pass">Password.</param>
        /// <returns>SAML response.</returns>
        private async Task<string> getSamlResponseAsync(string samlpath, string username, string pass)
        {
            var samlform = new Dictionary<string, string>();
            samlform.Add("UserName", username);
            samlform.Add("Password", pass);
            samlform.Add("AuthMethod", "FormsAuthentication");

            var samlpage = await (await postAsync(samlpath, new FormUrlEncodedContent(samlform)).ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false);
            var samldoc = new HtmlDocument();
            samldoc.LoadHtml(samlpage);

            return samldoc.DocumentNode.SelectSingleNode("//body/form/input").GetAttributeValue("value", null);
        }

        /// <summary>
        /// Makes a GET request with cookies.
        /// </summary>
        /// <param name="url">URL to request.</param>
        /// <param name="cookieoverride">Override for cookies.</param>
        /// <returns>HTTP response.</returns>
        private async Task<HttpResponseMessage> getAsync(string url, string cookieoverride = "")
        {
            var uri = new Uri(url);

            // Expire all old cookies
            var old = _cookies.GetCookies(uri);
            foreach (Cookie c in old)
            {
                c.Expired = true;
            }

            _http.DefaultRequestHeaders.Remove("Cookie");
            if (string.IsNullOrEmpty(cookieoverride))
                _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            else
                _http.DefaultRequestHeaders.Add("Cookie", cookieoverride);
            var resp = await _http.GetAsync(uri).ConfigureAwait(false);

            if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode <= 399)
            {
                var redirectUri = new Uri(uri.GetLeftPart(UriPartial.Authority) + resp.Headers.Location.OriginalString);
                return await getAsync(redirectUri.ToString()).ConfigureAwait(false);
            }

            updateCookies();
            return resp;
        }

        /// <summary>
        /// Makes a POST request.
        /// </summary>
        /// <param name="url">URL to request.</param>
        /// <param name="form">Form to submit.</param>
        /// <param name="nocookies">Whether not to use cookies.</param>
        /// <returns>HTTP response.</returns>
        private async Task<HttpResponseMessage> postAsync(string url, FormUrlEncodedContent form, bool nocookies = false)
        {
            var uri = new Uri(url);

            _http.DefaultRequestHeaders.Remove("Cookie");
            if (!nocookies)
                _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            var resp = await _http.PostAsync(uri, form).ConfigureAwait(false);

            var hh = resp.Headers.ToString();
            var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var h = resp.RequestMessage.Headers.ToString();

            if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode <= 399)
            {
                return await postAsync(resp.Headers.Location.ToString(), form, nocookies).ConfigureAwait(false);
            }

            updateCookies();
            return resp;
        }

        /// <summary>
        /// Updates local cookies.
        /// </summary>
        private void updateCookies()
        {
            var cookies = getAllCookies();
            foreach (var c in cookies)
            {
                _koekjes.RemoveAll(x => x == c.Name + "=" + c.Value);
                _koekjes.Add(c.Name + "=" + c.Value);
            }
        }

        /// <summary>
        /// Gets ALL cookies. (any domain)
        /// </summary>
        /// <returns>List of cookies.</returns>
        private List<Cookie> getAllCookies()
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)_cookies.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.Instance,
                null,
                _cookies,
                null);

            foreach (string key in table.Keys)
            {
                var item = table[key];
                var items = (ICollection)item.GetType().GetProperty("Values").GetGetMethod().Invoke(item, null);
                foreach (CookieCollection cc in items)
                {
                    foreach (Cookie cookie in cc)
                    {
                        cookies.Add(cookie);
                    }
                }
            }

            return cookies;
        }

        /// <summary>
        /// Makes a cookie string.
        /// </summary>
        /// <returns>Cookie string.</returns>
        private string generateCookieString()
        {
            return string.Join("; ", _koekjes);
        }

        /// <summary>
        /// Splits cookie string to several strings and sets local cookies.
        /// </summary>
        /// <param name="input">Cookie string</param>
        private void splitCookieString(string input)
        {
            this._koekjes = input.Split(';').ToList();
            this._koekjes.ForEach(x =>
            {
                x = x.Trim();
                Console.WriteLine(x);
            });
        }
    }
}