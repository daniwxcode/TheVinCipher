using HtmlAgilityPack;

using Humanizer;

using Infrastructure.APIs.Interfaces;
using Infrastructure.Contexts;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Net;
using System.Text.RegularExpressions;

namespace Services.DataServices
{
    public class VinRushScrapper : IVinDecoder<Dictionary<string, string>>
    {

        private readonly IScrappableSource source;
        private readonly HermesContext _dbContext;
        public string Vin { get; set; }
        public VinRushScrapper (IScrappableSource scrappableSource, HermesContext context)
        {
            this.source = scrappableSource;
            this._dbContext = context;
        }

        public bool Succes { get; set; }

        public Task<Dictionary<string, string>> IdentifyCarByVINAsync (string vin)
        {
            var response = new Dictionary<string, object>();
            WebClient webClient = new WebClient();
          //  string page = webClient.DownloadString(GetUri(vin));
            string page = webClient.DownloadString(GetUri(vin));

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);
            var result = new Dictionary<string,string>();
            var htables = doc.DocumentNode.SelectNodes("//table");
            foreach (HtmlNode htable in htables)
            {
                var htr = htable.SelectNodes("tbody").FirstOrDefault().
                SelectNodes("tr")
                .Where(tr => tr.SelectNodes("td").Count() > 1);
                foreach (HtmlNode row in htr)
                {
                    var tmp = new List<string>();
                        
                        
                    foreach (HtmlNode cell in row.SelectNodes("td"))
                    {
                        tmp.Add(Regex.Replace(cell.InnerText, @"[^0-9a-zA-Z:, /]+", "").Trim());
                       
                        //  Console.WriteLine("cell: " + cell.InnerText);
                    }
                    if (tmp[0] == "Image")
                    {
                        tmp[1]= row.SelectNodes("//td//img").FirstOrDefault().GetAttributeValue("src","");

                    }
                    result.Add(tmp[0].Underscore(), tmp[1]);
                }

            }
            //if(result.Count > 20)
            //return Task.FromResult(result);
            //else
            //{
            //    return Task
            //}
            //if (Vin.Length >= 11)
            //{
            //    var car = _dbContext.Cars.FirstOrDefault(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11));
            //    var json = JsonConvert.SerializeObject(car);
            //    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            //    foreach (var pair in dictionary)
            //    {
            //        if (pair.Key == "Description")
            //            break;
            //       result.Add(pair.Key, pair.Value);
            //    }
            //    return Task.FromResult(dictionary);
            //}
            return Task.FromResult(result);
        }

        public string GetUri (string vin)
        {
            return source.GetUrlAsync(vin).Result;
        }
    }
}
