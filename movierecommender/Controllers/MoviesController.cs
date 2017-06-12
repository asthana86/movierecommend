using System;
using System.Collections.Generic;
using movierecommender.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace movierecommender.Controllers
{
    
    public class MoviesController : Controller
    {

        private readonly AppSettings _appSettings;
        private static HttpClient Client = new HttpClient();
        private readonly ILogger<MoviesController> _logger;
        public static string recommendeditems;
        // GET: Movies  
        public ActionResult Choose()
        {
            Movies movies = new Movies();

            ViewData["movies"] = movies; 
            return View();
        }

        public MoviesController(ILogger<MoviesController> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        static async Task InvokeRequestResponseService(int id, ILogger logger, AppSettings appSettings)
        {
            using (var client = MoviesController.Client)
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "UserId", "1"
                                            },
                                            {
                                                "MovieId", id.ToString()
                                            },
                                            {
                                                "Rating", "10"
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                string apiKey = appSettings.apikey;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(appSettings.uri);

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:b
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)
                
                //HttpResponseMessage response = client.PostAsJsonAsync("", scoreRequest).Result;
                HttpResponseMessage response = await client.PostAsync(appSettings.uri, new JsonContent(scoreRequest));
                
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    MoviesController.recommendeditems = result;
                }
                else
                {
                   logger.LogError(string.Format("The request failed with status code: {0}", response.StatusCode));
                    
                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                   logger.LogDebug(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                   logger.LogDebug(responseContent);
                }
        }
}
        public ActionResult Recommend(int id)
        {
            InvokeRequestResponseService(id, _logger, _appSettings).Wait();
            Movies movies = new Movies();
            int[] movieselector = new int[Movies.moviestorecommend];
            
            string parsestr = MoviesController.recommendeditems;
            var obj = JObject.Parse(parsestr);
            String text; 
            for (int i = 0; i < Movies.moviestorecommend; i++)
            {
                text = (string)obj["Results"]["output1"][0]["Related Item " + (i + 1)];
                movieselector[i] = Int32.Parse(text);
            }
            ViewData["movies"] = movies; 
            ViewData["movieselector"] = movieselector;
            return View();
        }

        public ActionResult Watch()
        {
            return View();
        }

        public class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }
    }
}