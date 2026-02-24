using HtmlAgilityPack;

using Humanizer;

using Infrastructure.APIs.Interfaces;

using System.Net;
using System.Text.RegularExpressions;

namespace Services.DataServices
{
    public class VinRushScrapper : IVinDecoder<Dictionary<string, string>>
    {

        private readonly IScrappableSource source;
        public string Vin { get; set; }
        public VinRushScrapper(IScrappableSource scrappableSource)
        {
            this.source = scrappableSource;
        }

        public bool Succes { get; set; }

        public async Task<Dictionary<string, string>> IdentifyCarByVINAsync(string vin, int us = 0)
        {
            var response = new Dictionary<string, object>();
            WebClient webClient = new WebClient();
            string page = webClient.DownloadString(GetUri(vin, us));

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);
            var result = new Dictionary<string, string>();
            var htables = doc.DocumentNode.SelectNodes("//table");
            if (htables == null)
            {
                return result; // retourner le dictionnaire vide
            }
            foreach (HtmlNode htable in htables)
            {
                var tbody = htable.SelectNodes("tbody")?.FirstOrDefault();
                if (tbody == null) continue;
                var htr = tbody.SelectNodes("tr")
                ?.Where(tr => tr.SelectNodes("td")?.Count() > 1);
                if (htr == null) continue;
                foreach (HtmlNode row in htr)
                {
                    var cells = row.SelectNodes("td");
                    if (cells == null || cells.Count < 2) continue;
                    var tmp = new List<string>();

                    int i = 0;
                    foreach (HtmlNode cell in cells)
                    {
                        if (i == 0)
                            tmp.Add(Regex.Replace(cell.InnerText, @"[^0-9a-zA-Z:, /]+", "").Trim());
                        else
                            tmp.Add(cell.InnerText.Trim());
                        //  Console.WriteLine("cell: " + cell.InnerText);
                        i++;
                    }
                    if (tmp[0] == "Image")
                    {
                        var imgNode = row.SelectNodes("//td//img")?.FirstOrDefault();
                        if (imgNode != null)
                            tmp[1] = imgNode.GetAttributeValue("src", "");

                    }
                    result.TryAdd(tmp[0].Underscore(), tmp[1]);
                }
            }

            return result;
        }

        public string GetUri(string vin, int us)
        {
            return source.GetUrlAsync(vin, us).Result;
        }


    }
}
