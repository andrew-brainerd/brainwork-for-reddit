using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BrainForReddit.Models;

namespace BrainForReddit.Controllers
{
    public class HomeController : Controller
    {
        private static string baseUrl = "https://ssl.reddit.com";
        private static string authEndpoint = "/api/v1/authorize/";
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
                Response.Redirect("/Home/Authorize");

                ViewData["requestUrl"] = requestUrl;
            }

            return View();
        }

        public IActionResult Authorize(string access_token)
        {
            var alreadyBeenHere = HttpContext.Session.GetString("TriedToAuthBefore");

            if (alreadyBeenHere == null)
            {
                HttpContext.Session.SetString("TriedToAuthBefore", "totes");
                Response.Redirect(requestUrl);
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
    }
}
