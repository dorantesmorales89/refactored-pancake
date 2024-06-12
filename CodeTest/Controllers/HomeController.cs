using System;
/*[B]Dorantes 12/06/2024*/
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using PruebaIngreso.Models;
/*[E]Dorantes 12/06/2024*/
using Quote.Contracts;
using Quote.Models;

namespace PruebaIngreso.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQuoteEngine quote;
        private readonly IMarginProvider marginProvider;
        
        public HomeController(IQuoteEngine quote, IMarginProvider marginProvider)
        {
            this.quote = quote;
            this.marginProvider = marginProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            var request = new TourQuoteRequest
            {
                adults = 1,
                ArrivalDate = DateTime.Now.AddDays(1),
                DepartingDate = DateTime.Now.AddDays(2),
                getAllRates = true,
                GetQuotes = true,
                RetrieveOptions = new TourQuoteRequestOptions
                {
                    GetContracts = true,
                    GetCalculatedQuote = true,
                },
                TourCode = "E-U10-PRVPARKTRF",
                Language = Language.Spanish
            };

            var result = this.quote.Quote(request);
            var tour = result.Tours.FirstOrDefault();
            ViewBag.Message = "Test 1 Correcto";
            return View(tour);
        }

        public ActionResult Test2()
        {
            ViewBag.Message = "Test 2 Correcto";
            return View();
        }
                
        public ActionResult Test3()
        {
            /*[B]Dorantes 12/06/2024*/
            var codes = new List<string> { "E-U10-UNILATIN", "E-U10-DSCVCOVE", "E-E10-PF2SHOW" };
            var apiUrlTemplate = "http://refactored-pancake.free.beeceptor.com/margin/{0}";
            var results = new List<ApiResult>();

            using (var client = new HttpClient())
            {
                foreach (var code in codes)
                {
                    var apiUrl = string.Format(apiUrlTemplate, code);
                    try
                    {
                        var response = client.GetAsync(apiUrl).Result;
                        int statusCode = (int)response.StatusCode;
                        double margin;

                        if (statusCode == 200)
                        {
                            var content = response.Content.ReadAsStringAsync().Result;
                            dynamic json = JObject.Parse(content);
                            margin = json.margin;
                        }
                        else
                        {
                            margin = 0.0;
                        }
                        
                        results.Add(new ApiResult
                        {
                            Code = code,
                            Margin = margin,
                            StatusCode = statusCode
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new ApiResult
                        {
                            Code = code,
                            Margin = 0.0,
                            StatusCode = 0 /*StatusCode default en cero para indicar si hubo un error*/
                        });
                    }
                }
            }
            
            ViewBag.Results = results;
			/*[E]Dorantes 12/06/2024*/
            return View();
        }
        public ActionResult Test4()
        {
            decimal defaultMargin = this.marginProvider.GetMargin("E-U10-UNILATIN");

            var request = new TourQuoteRequest
            {
                adults = 1,
                ArrivalDate = DateTime.Now.AddDays(1),
                DepartingDate = DateTime.Now.AddDays(2),
                getAllRates = true,
                GetQuotes = true,
                RetrieveOptions = new TourQuoteRequestOptions
                {
                    GetContracts = true,
                    GetCalculatedQuote = true,
                },
                Language = Language.Spanish
            };

            var result = this.quote.Quote(request);

            var apiUrlTemplate = "http://refactored-pancake.free.beeceptor.com/margin/{0}";

            using (var client = new HttpClient())
            {
                foreach (var tour in result.TourQuotes)
                {
                    var code = tour.TourCode;
                    var apiUrl = string.Format(apiUrlTemplate, code);
                    try
                    {
                        var response = client.GetAsync(apiUrl).Result;
                        int statusCode = (int)response.StatusCode;
                        decimal margin;

                        if (statusCode == 200)
                        {
                            var content = response.Content.ReadAsStringAsync().Result;
                            dynamic json = JObject.Parse(content);
                            margin = json.margin;
                        }
                        else
                        {
                            margin = defaultMargin;
                        }
                        tour.margin = margin;
                    }
                    catch (Exception ex)
                    {
                        tour.margin = defaultMargin;
                    }
                }
            }

            return View(result.TourQuotes);
        }
    }
}