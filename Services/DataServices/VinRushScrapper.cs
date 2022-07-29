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

        public async Task<Dictionary<string, string>> IdentifyCarByVINAsync (string vin)
        {
            var response = new Dictionary<string, object>();
            WebClient webClient = new WebClient();
            //  string page = webClient.DownloadString(GetUri(vin));
            string page = webClient.DownloadString(GetUri(vin));

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);
            var result = new Dictionary<string, string>();
            var htables = doc.DocumentNode.SelectNodes("//table");
            foreach (HtmlNode htable in htables)
            {
                var htr = htable.SelectNodes("tbody").FirstOrDefault().
                SelectNodes("tr")
                .Where(tr => tr.SelectNodes("td").Count() > 1);
                foreach (HtmlNode row in htr)
                {
                    var tmp = new List<string>();

                    int i = 0;
                    foreach (HtmlNode cell in row.SelectNodes("td"))
                    {
                        if(i==0)
                        tmp.Add(Regex.Replace(cell.InnerText, @"[^0-9a-zA-Z:, /]+", "").Trim());
                        else
                            tmp.Add(cell.InnerText);
                        //  Console.WriteLine("cell: " + cell.InnerText);
                        i++;
                    }
                    if (tmp[0] == "Image")
                    {
                        tmp[1] = row.SelectNodes("//td//img").FirstOrDefault().GetAttributeValue("src", "");

                    }
                    result.TryAdd(tmp[0].Underscore(), tmp[1]);
                }

            }
           // var car = _dbContext.Cars.Where(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11)).FirstOrDefault();
           // var carbase = _dbContext.CarsBases.Where(c => c.Vin.Substring(0, 11) == vin.Substring(0, 11) ||
           //c.Vin.Substring(3,9)==vin.Substring(3,9)).FirstOrDefault();
            
               
           //     result.TryAdd("year",((carbase?.Year?? car?.Year)??0).ToString());
           //     result.Add("trim", (car?.Trim ?? ""));
           //     result.TryAdd("number_of_seater", ((carbase?.StandardSeating ?? car?.Seats)??0).
           //         ToString());
           //     result.TryAdd("trim", ((carbase?.Trim ?? car?.Trim)??"").ToString());
           //     result.TryAdd("model", ((carbase?.Model ?? car?.Model)??"").ToString());
           // int cardoors = 0;
           // cardoors = carbase?.Doors ?? 0;
           // if(cardoors == 0)
           // {
           //     cardoors= car?.Doors ?? 0;
           // }
           // result.TryAdd("number_of_doors", cardoors.ToString());
           //     result.TryAdd("engine_type", (carbase?.FuelType ?? car?.Energy)??"");
           //     var capacity = car?.EngineCylender < 1000 ? car?.EngineCylender * 1000 : car?.EngineCylender;
           //     if (capacity == 0)
           //     {
           //         result.TryAdd("capacity", carbase?.EngineCylinders);
           //     }
           //     else
           //     {
           //         result.TryAdd("capacity",(capacity).ToString());
           //     }
           //     result.TryAdd("engine_power", (car?.EnginPower ?? 0).ToString());
                
                
           //     result.TryAdd("category",(carbase?.Category??car?.Category)??"");
           //     result.TryAdd("gearbox_type",(carbase?.TransmissionType?? car?.Transmission)??"");
           //    var t1= result.TryAdd("make",(carbase?.Make?? car?.Make)??"");
           //    var t2= result.TryAdd("madein",carbase?.MadeIn??"");
           //    var t3= result.TryAdd("made_in_city",carbase?.MadeInCity??"");
           //    var t4= result.TryAdd("overall_width", carbase?.OverallWidth??"");
           //    var t5= result.TryAdd("highway_mileage", carbase?.HighwayMileage??"");
           // result["status"]= (!(t1&& t2 && t3 && t4 && t4 && carbase==null && result.Count<25)).ToString();
            
            return result;
        }

        public string GetUri (string vin)
        {
            return $"https://www.freevindecoder.eu/{vin}";
            //source.GetUrlAsync(vin).Result;
        }
    }
}
