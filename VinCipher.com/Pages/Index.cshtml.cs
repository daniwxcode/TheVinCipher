using System.Text;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinCipher.Com.Pages;

/// <summary>
/// Serves the main landing page and injects the HMAC challenge key server-side.
/// </summary>
public sealed class IndexModel : PageModel
{
    /// <summary>
    /// Comma-separated byte values of the HMAC secret, ready for JS consumption.
    /// </summary>
    public string HmacKeyJson { get; private set; } = string.Empty;

    private readonly IConfiguration _configuration;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet()
    {
        var secret = _configuration["Playground:HmacSecret"]
            ?? throw new InvalidOperationException("Playground:HmacSecret is not configured.");
        var bytes = Encoding.UTF8.GetBytes(secret);
        HmacKeyJson = string.Join(',', bytes);
    }
}
