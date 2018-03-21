﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BrainForReddit.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BrainForReddit.Controllers
{
    public class HomeController : Controller
    {
        private static string baseUrl = "https://ssl.reddit.com";
        private static string authEndpoint = "/api/v1/authorize/";
        private static string tokenEndpoint = "/api/v1/access_token";
        private static string clientId = "dYZpA0WtQHj1dA";
        private static string responseType = "code";
        private static string state = new Random().Next().ToString();
        private static string scope = "read";
        private static string duration = "temporary"; // or "permanent"
        private static string redirectUrl = "http://localhost:61428/Home/Authorize";
        private static string requestUrl = baseUrl + authEndpoint + 
            "?client_id=" + clientId + 
            "&redirect_uri=" + redirectUrl + 
            "&response_type=" + responseType + 
            "&scope=" + scope + 
            "&duration=" + duration + 
            "&state=" + state;

        public IActionResult Index()
        {
            var sessionToken = HttpContext.Session.GetString("AccessToken");

            if (sessionToken == null)
            {
                ViewData["requestUrl"] = requestUrl;

                Response.Redirect("/Home/Authorize");
            }

            return View();
        }

        public async Task<IActionResult> Authorize(string error, string code, string state, string access_token)
        {
            var currentAuthStep = HttpContext.Session.GetString("CurrentAuthStep");

            if (currentAuthStep == null)
            {
                HttpContext.Session.SetString("CurrentAuthStep", "code");
                Response.Redirect(requestUrl);
            }
            else if (currentAuthStep == "code")
            {
                if (code != null)
                {
                    HttpContext.Session.SetString("CurrentAuthStep", "token");
                    await RequestAccessToken(error, code, state);
                }
            }

            if (access_token != null)
            {
                HttpContext.Session.SetString("AccessToken", access_token);
                Response.Redirect("/Home");
            }

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static async Task<string> RequestAccessToken(string error, string code, string state)
        {
            HttpClient client = new HttpClient() { BaseAddress = new Uri("https://www.reddit.com/") };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response;

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            var httpContent = new FormUrlEncodedContent(
                new[]
                {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("redirect_uri", redirectUrl)
                });

            tokenRequest.Content = httpContent;
            response = await client.SendAsync(tokenRequest);

            var accessToken = await response.Content.ReadAsStringAsync();

            return accessToken;
        }
    }
}
