using Microsoft.AspNetCore.Http.Features;
using PandaClaus.Web;
using PandaClaus.Web.Core;
using PandaClaus.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<FeatureFlags>();
builder.Services.AddScoped<GoogleSheetsClient>();
builder.Services.AddScoped<BlobClient>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddHttpClient<GetResponseClient>();

// Add HttpClient for InPost API
builder.Services.AddHttpClient<InPostApiClient>();
builder.Services.AddScoped<InPostApiClient>();

builder.Services.AddScoped<InPostShipmentRequestBuilder>();

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = 10 * 1024 * 1024; // 10MB
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.MultipartHeadersLengthLimit = 10 * 1024 * 1024; // 10MB
});
builder.Services.AddScoped<LetterNumerationService>();
builder.Services.AddScoped<ICsvExporter, CsvExporter>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowInPostOrigin", policy =>
    {
        policy.WithOrigins("https://geowidget-app.inpost.pl")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Add only if you need to support credentials
    });
});

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // In development, still catch exceptions but show detailed error page
    app.UseExceptionHandler("/Error");
}

// Handle status code pages (404, 403, etc.)
app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

// The legacy adminCode query still exists in this branch: never leak it via Referer.
app.Use(async (context, next) =>
{
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    var path = context.Request.Path;
    if (path == "/" || path.StartsWithSegments("/Index", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/StatusUpdate", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Letters", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/Confirmation", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/LetterAddedConfirmation", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/ReceiveConfirmation", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers.CacheControl = "private, no-store";
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    }
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseCors("AllowInPostOrigin");

app.UseAuthorization();

app.MapRazorPages();

app.Run();
