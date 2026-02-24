using HtmlAgilityPack;

using Humanizer;

namespace Services.DataServices;

/// <summary>
/// Scrapes vehicle specs from freevindecoder.eu.
/// Uses the English version for consistent labels: https://www.freevindecoder.eu/en/{VIN}
/// Data is in table rows with td.info-left (label) and td.info-right (value).
/// </summary>
public class FreeVinDecoderScrapper
{
    private const string BaseUrl = "https://www.freevindecoder.eu/en/";
    private readonly HttpClient _httpClient;

    public FreeVinDecoderScrapper(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Dictionary<string, string>> DecodeAsync(string vin, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, string>();

        string page;
        try
        {
            page = await _httpClient.GetStringAsync($"{BaseUrl}{vin}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return result;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(page);

        // Each spec row: <tr> with <td class="info-left"> and <td class="info-right">
        var rows = doc.DocumentNode.SelectNodes("//tr[td[@class='info-left'] and td[@class='info-right']]");
        if (rows is null)
            return result;

        foreach (var row in rows)
        {
            var labelNode = row.SelectSingleNode("td[@class='info-left']");
            var valueNode = row.SelectSingleNode("td[@class='info-right']");

            if (labelNode is null || valueNode is null)
                continue;

            var label = HtmlEntity.DeEntitize(labelNode.InnerText).Trim();
            var value = HtmlEntity.DeEntitize(valueNode.InnerText).Trim();

            if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(value))
                continue;

            result.TryAdd(label.Underscore(), value);
        }

        return result;
    }
}
