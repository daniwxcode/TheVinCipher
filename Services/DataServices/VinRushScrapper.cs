using HtmlAgilityPack;

using Humanizer;

using Infrastructure.APIs.Interfaces;

using System.Text.RegularExpressions;

namespace Services.DataServices
{
    public class VinRushScrapper : IVinDecoder<Dictionary<string, string>>
    {
        private readonly IScrappableSource _source;
        private readonly HttpClient _httpClient;

        public VinRushScrapper(IScrappableSource scrappableSource, HttpClient httpClient)
        {
            _source = scrappableSource;
            _httpClient = httpClient;
        }

        public bool Succes { get; set; }

        public async Task<Dictionary<string, string>> IdentifyCarByVINAsync(string vin, int us = 0)
        {
            var uri = await _source.GetUrlAsync(vin, us);
            var page = await _httpClient.GetStringAsync(uri);

            var doc = new HtmlDocument();
            doc.LoadHtml(page);
            var result = new Dictionary<string, string>();
            var htables = doc.DocumentNode.SelectNodes("//table");
            if (htables is null)
                return result;

            foreach (var htable in htables)
            {
                var tbody = htable.SelectNodes("tbody")?.FirstOrDefault();
                if (tbody is null) continue;

                var rows = tbody.SelectNodes("tr")
                    ?.Where(tr => tr.SelectNodes("td")?.Count() > 1);
                if (rows is null) continue;

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td");
                    if (cells is null || cells.Count < 2) continue;

                    var key = Regex.Replace(cells[0].InnerText, @"[^0-9a-zA-Z:, /]+", "").Trim();
                    var value = cells[1].InnerText.Trim();

                    if (key == "Image")
                    {
                        var imgNode = row.SelectNodes("//td//img")?.FirstOrDefault();
                        if (imgNode is not null)
                            value = imgNode.GetAttributeValue("src", "");
                    }

                    result.TryAdd(key.Underscore(), value);
                }
            }

            return result;
        }

        public string GetUri(string vin, int us)
        {
            return _source.GetUrlAsync(vin, us).GetAwaiter().GetResult();
        }
    }
}
