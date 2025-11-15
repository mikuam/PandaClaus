using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;

namespace PandaClaus.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : BasePageModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        // Log the error details
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        
        if (exceptionHandlerPathFeature?.Error != null)
        {
            _logger.LogError(exceptionHandlerPathFeature.Error,
                "An unhandled exception occurred. Path: {Path}, RequestId: {RequestId}",
                exceptionHandlerPathFeature.Path,
                RequestId);
        }
        else
        {
            _logger.LogWarning("Error page accessed without exception details. RequestId: {RequestId}", RequestId);
        }
    }
}

