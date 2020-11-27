﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using NSchedule.Entities;
using System.Collections;
using System.Reflection;
using HtmlAgilityPack;

// Many thanks to LuukEsselbrugge for making an old version of his SRooster code available on GitHub.
// This saved a lot of time researching how xedule authentication and xedule's api works.
namespace NSchedule.Helpers
{
    internal class RestHelper
    {
        private HttpClientHandler _handler;
        private HttpClient _http;
        private CookieContainer _cookies;
        private List<string> _koekjes;
        private Database _db;

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

        public async Task<Calendar> GetCalendarAsync(int school, int weekNumber)
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/calendar/{school}_{weekNumber}");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Calendar>(content);
        }

        public async Task<List<OrganisationalUnit>> GetOrganisationalUnitsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/organisationalUnit");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<OrganisationalUnit>>(content);
        }

        public async Task<List<Year>> GetYearsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/year");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Year>>(content);
        }

        public async Task<List<Room>> GetRoomsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/facility");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Room>>(content);
        }

        public async Task<List<Teacher>> GetTeachersAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/docent");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Teacher>>(content);
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/team");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Team>>(content);
        }

        public async Task<List<Entities.Group>> GetGroupsAsync()
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/group");
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Entities.Group>>(content);
        }

        public async Task<Schedule> GetScheduleAsync(long schoolid, long year, long weeknum, long id)
        {
            var response = await getAsync(Constants.API_ENDPOINT + $"/schedule/?ids%5B0%5D={schoolid}_{year}_{weeknum}_{id}");
            var content = await response.Content.ReadAsStringAsync();

            var results = JsonConvert.DeserializeObject<List<Schedule>>(content);

            return results.Count > 0 ? results[0] : null;
        }

        public async Task<(bool Success, string Message)> CheckAndReconnectSessionAsync()
        {
            // This will check whether the saved session still works
            var settings = await _db.GetSettingsAsync();

            if (!string.IsNullOrEmpty(settings.CookieString))
            {
                splitCookieString(settings.CookieString);
                // Do een request om te testen of cookies valid zijn
                var response = await getAsync(Constants.API_ENDPOINT + $"/year", settings.CookieString);

                var forbidden = response.StatusCode == HttpStatusCode.Forbidden;
                if (!forbidden)
                {
                    // YES! sessie nog valid. we stellen gewoon lekker deze koekjes weer in.
                    return (true, "Re-auth via stored cookie success!");
                }
            }

            // als invalid koekjes, probeer nog 1x in te loggen met oude auth..
            var auth = await this.Authenticate(settings.Email, settings.Password);

            // Re-auth met login OK
            if (auth.Success)
                return (true, "Re-auth via re-login success!");

            // Re-auth niet mogelijk, return false en leeg koekjes.
            this._koekjes.Clear();
            return (false, "Re-auth failed!");
        }

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
            var samlpath = await getSamlPathAsync(loginpath);
            if (samlpath == null)
            {
                return (false, "Wait... something went wrong. Did NHL Stenden change their login?");
            }

            // Trying to authenticate and get saml response
            var samlresponse = await getSamlResponseAsync(samlpath, username, password);
            if (samlresponse == null)
            {
                return (false, "Invalid credentials!");
            }

            // trying to get surf auth and relay state

            var surf = await getSurfAsync(samlresponse);
            if (surf.samlresponse == null || surf.relaystate == null)
            {
                return (false, "Something went really really wrong behind the scenes!");
            }
            surf.relaystate = surf.relaystate.Replace("&amp;", "&");

            // Authenticating
            var auth = await authenticateAsync(surf.samlresponse, surf.relaystate);

            var settings = await _db.GetSettingsAsync();
            settings.CookieString = generateCookieString();
            settings.Email = username;
            settings.Password = password;
            await _db.SetSettingsAsync(settings);

            if (auth)
                return (true, "Sign in successful!");
            else
                return (false, "Oops, surf error??");
        }

        private async Task<bool> authenticateAsync(string samlresponse, string relaystate)
        {
            var assertform = new Dictionary<string, string>();
            assertform.Add("SAMLResponse", samlresponse);
            assertform.Add("RelayState", relaystate);

            var resp = await postAsync(Constants.ASSERTION_ENDPOINT, new FormUrlEncodedContent(assertform));

            return resp.StatusCode == HttpStatusCode.OK;
        }

        private async Task<(string samlresponse, string relaystate)> getSurfAsync(string samlresponse)
        {
            var surfform = new Dictionary<string, string>();
            surfform.Add("SAMLResponse", samlresponse);

            var surfresponse = await (await postAsync(Constants.SURF_ENDPOINT, new FormUrlEncodedContent(surfform))).Content.ReadAsStringAsync();
            var surfdoc = new HtmlDocument();
            surfdoc.LoadHtml(surfresponse);

            var form = surfdoc.DocumentNode.SelectSingleNode("//body/div/form");
            var samlresponse2 = form.ChildNodes[1].GetAttributeValue("value", null);
            var relaystate = form.ChildNodes[3].GetAttributeValue("value", null);

            return (samlresponse2, relaystate);
        }

        private async Task<string> getSamlPathAsync(string loginpath)
        {
            var loginpage = await (await getAsync(loginpath)).Content.ReadAsStringAsync();
            var logindoc = new HtmlDocument();
            logindoc.LoadHtml(loginpage);

            return logindoc.GetElementbyId("options").GetAttributeValue<string>("action", null);
        }

        private async Task<string> getSamlResponseAsync(string samlpath, string username, string pass)
        {
            var samlform = new Dictionary<string, string>();
            samlform.Add("UserName", username);
            samlform.Add("Password", pass);
            samlform.Add("AuthMethod", "FormsAuthentication");

            var samlpage = await(await postAsync(samlpath, new FormUrlEncodedContent(samlform))).Content.ReadAsStringAsync();
            var samldoc = new HtmlDocument();
            samldoc.LoadHtml(samlpage);

            return samldoc.DocumentNode.SelectSingleNode("//body/form/input").GetAttributeValue("value", null);
        }

        private async Task<HttpResponseMessage> getAsync(string url, string cookieoverride = "")
        {
            var uri = new Uri(url);

            // Expire all old cookies
            var old = _cookies.GetCookies(uri);
            foreach(Cookie c in old)
            {
                c.Expired = true;
            }

            _http.DefaultRequestHeaders.Remove("Cookie");
            if (string.IsNullOrEmpty(cookieoverride))
                _http.DefaultRequestHeaders.Add("Cookie", generateCookieString());
            else
                _http.DefaultRequestHeaders.Add("Cookie", cookieoverride);
            var resp = await _http.GetAsync(uri);

            if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode <= 399)
            {
                var redirectUri = new Uri(uri.GetLeftPart(UriPartial.Authority) + resp.Headers.Location.OriginalString);
                return await getAsync(redirectUri.ToString());
            }

            updateCookies();
            return resp;
        }

        private async Task<HttpResponseMessage> postAsync(string url, FormUrlEncodedContent form, bool nocookies = false)
        {
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

            updateCookies();
            return resp;
        }

        private void updateCookies()
        {
            var cookies = getAllCookies();
            foreach (var c in cookies)
            {
                _koekjes.RemoveAll(x => x == c.Name + "=" + c.Value);
                _koekjes.Add(c.Name + "=" + c.Value);
            }
        }

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

        private string generateCookieString()
        {
            return string.Join("; ", _koekjes);
        }

        private void splitCookieString(string input)
        {
            this._koekjes = input.Split(';').ToList();
            this._koekjes.ForEach(x =>
            {
                x = x.Trim();
                Console.WriteLine(x);
            });
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