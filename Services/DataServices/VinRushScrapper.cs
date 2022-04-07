using HtmlAgilityPack;

using Infrastructure.APIs.Interfaces;

using System.Net;
using System.Text.RegularExpressions;

namespace Services.DataServices
{
    public class VinRushScrapper : IVinDecoder<Dictionary<string, string>>
    {

        private readonly IScrappableSource source;
        public string Vin { get; set; }
        public VinRushScrapper (IScrappableSource scrappableSource)
        {
            this.source = scrappableSource;
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
                    result.Add(tmp[0],tmp[1]);
                }

            }
            return Task.FromResult(result);
        }

        public string GetUri (string vin)
        {
            return source.GetUrlAsync(vin).Result;
        }
    }
}
