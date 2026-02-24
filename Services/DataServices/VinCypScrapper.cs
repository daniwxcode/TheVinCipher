using HtmlAgilityPack;

using Humanizer;

namespace Services.DataServices;

/// <summary>
/// Scrapes vehicle specs from vincyp.com.
/// URL pattern: https://vincyp.com/vin/{VIN}
/// Specs are in a grid of div.group elements, each with two spans (label, value).
/// </summary>
public class VinCypScrapper
{
    private const string BaseUrl = "https://vincyp.com/vin/";
    private readonly HttpClient _httpClient;

    public VinCypScrapper(HttpClient httpClient)
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

        // Each spec row is a div.group containing two <span> children (label, value)
        var specDivs = doc.DocumentNode.SelectNodes(
            "//div[contains(@class,'group') and contains(@class,'justify-between')]");

        if (specDivs is null)
            return result;

        foreach (var div in specDivs)
        {
            var spans = div.SelectNodes("span");
            if (spans is null || spans.Count < 2)
                continue;

            var label = HtmlEntity.DeEntitize(spans[0].InnerText).Trim();
            var value = HtmlEntity.DeEntitize(spans[1].InnerText).Trim();

            if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(value))
                continue;

            result.TryAdd(label.Underscore(), value);
        }

        return result;
    }
}
