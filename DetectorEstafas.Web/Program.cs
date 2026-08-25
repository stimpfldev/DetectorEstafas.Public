using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services;
using DetectorEstafas.Web.Services.Api;
using DetectorEstafas.Web.Services.Api.Administracion;
using DetectorEstafas.Web.Services.Audios;
using DetectorEstafas.Web.Services.Capturas;
using DetectorEstafas.Web.Services.Comercial;
using DetectorEstafas.Web.Services.Comercial.MercadoPago;
using DetectorEstafas.Web.Services.Correo;
using DetectorEstafas.Web.Services.InteligenciaArtificial;
using DetectorEstafas.Web.Services.Telefonos;
using FFMpegCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);
string ffmpegBinaryFolder =
    builder.Configuration["FFmpeg:BinaryFolder"]
    ?? throw new InvalidOperationException(
        "No se configuró FFmpeg:BinaryFolder.");

GlobalFFOptions.Configure(options =>
{
    options.BinaryFolder = ffmpegBinaryFolder;
});
string connectionString =
    builder.Configuration.GetConnectionString(
        "DetectorEstafas")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión DetectorEstafas.");

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        32 * 1024;
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<CorreoOptions>(
    builder.Configuration.GetSection(
        CorreoOptions.SectionName));

builder.Services.AddScoped<
    ICorreoRegistroService,
    SmtpCorreoRegistroService>();

builder.Services.AddOpenApi();

builder.Services.Configure<ApiComercialOptions>(
    builder.Configuration.GetSection(ApiComercialOptions.SectionName));

builder.Services.Configure<MercadoPagoOptions>(
    builder.Configuration.GetSection(
        MercadoPagoOptions.SectionName));

builder.Services.AddHttpClient<
    IMercadoPagoSuscripcionService,
    MercadoPagoSuscripcionService>(httpClient =>
    {
        httpClient.BaseAddress =
            new Uri("https://api.mercadopago.com/");
        httpClient.Timeout =
            TimeSpan.FromSeconds(15);
    });

builder.Services.AddSingleton<
    IMercadoPagoWebhookSignatureValidator,
    MercadoPagoWebhookSignatureValidator>();

builder.Services.AddScoped<
    IProvisionamientoApiComercialService,
    ProvisionamientoApiComercialService>();

builder.Services.AddScoped<
    IComercializacionApiService,
    ComercializacionApiService>();

builder.Services.AddScoped<
    ICorreoComercialService,
    SmtpCorreoComercialService>();

builder.Services.AddHostedService<
    SuscripcionesVencidasHostedService>();

builder.Services.Configure<ApiAdministracionOptions>(
    builder.Configuration.GetSection(ApiAdministracionOptions.SectionName));

builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

builder.Services.AddScoped<
    IApiAdministracionService,
    ApiAdministracionService>();

builder.Services.Configure<InteligenciaArtificialOptions>(
    builder.Configuration.GetSection(
        InteligenciaArtificialOptions.SectionName));

builder.Services.AddHttpClient<
    IProveedorEvaluacionIa,
    OpenAiProveedorEvaluacionIa>();

builder.Services.AddScoped<
    IAnalisisIaService,
    AnalisisIaService>();

builder.Services.Configure<TranscripcionOptions>(
    builder.Configuration.GetSection(
        TranscripcionOptions.SectionName));

builder.Services.AddSingleton<
    ITranscriptorAudioService,
    WhisperTranscriptorAudioService>();

builder.Services.AddSingleton<
    IAudioNormalizadorService,
    FfmpegAudioNormalizadorService>();

builder.Services.Configure<AudioOptions>(
    builder.Configuration.GetSection(
        AudioOptions.SectionName));

builder.Services.AddScoped<
    IAudioTemporalService,
    AudioTemporalService>();

builder.Services.Configure<OcrOptions>(
    builder.Configuration.GetSection(
        OcrOptions.SectionName));

builder.Services.AddSingleton<
    IOcrCapturaService,
    TesseractOcrCapturaService>();

builder.Services.Configure<CapturaOptions>(
    builder.Configuration.GetSection(
        CapturaOptions.SectionName));

builder.Services.AddScoped<
    ICapturaTemporalService,
    CapturaTemporalService>();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name =
        "DetectorEstafas.Antiforgery";

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;

    options.Cookie.SecurePolicy =
     builder.Environment.IsDevelopment()
         ? CookieSecurePolicy.SameAsRequest
         : CookieSecurePolicy.Always;
});

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 500;
});

builder.Services.AddDistributedMemoryCache();

int adminSessionMinutes = Math.Clamp(
    builder.Configuration.GetValue<int?>(
        "ApiAdministracion:SessionMinutes") ?? 20,
    5,
    120);

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "DetectorEstafas.ApiAdmin";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(adminSessionMinutes);
});

builder.Services.AddDbContext<DetectorEstafasDbContext>(
    options =>
        options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<UsuarioAplicacion, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<DetectorEstafasDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<
    IAnalizadorEstafasService,
    AnalizadorEstafasService>();

builder.Services.AddSingleton<
    IIdentificacionTelefonoService,
    IdentificacionTelefonoService>();

builder.Services.AddHttpClient<
    IRdapService,
    RdapService>(httpClient =>
    {
        httpClient.BaseAddress =
            new Uri("https://rdap.nic.ar/");

        httpClient.Timeout =
            TimeSpan.FromSeconds(3);

        httpClient.DefaultRequestHeaders
            .UserAgent
            .ParseAdd("DetectorEstafas/2.0");

        httpClient.DefaultRequestHeaders
            .Accept
            .ParseAdd("application/rdap+json");
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "analisis",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 12,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "capturas",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 6,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "audios",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 4,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "api-comercial",
        httpContext =>
        {
            string partitionKey =
                httpContext.Request.Headers[ApiKeyMiddleware.HeaderName]
                    .FirstOrDefault()
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "sin-clave";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });

    options.AddPolicy(
        "registro",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "comercial-alta",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "admin-login",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "feedback",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        "rdap",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?
                    .ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.OnRejected = async (
        context,
        cancellationToken) =>
    {
        bool esperaHtml =
            context.HttpContext.Request.Headers
                .Accept
                .Any(value =>
                    value?.Contains(
                        "text/html",
                        StringComparison.OrdinalIgnoreCase)
                    == true);

        if (esperaHtml)
        {
            context.HttpContext.Response.Redirect(
                "/Analisis/Limite");

            return;
        }

        context.HttpContext.Response.ContentType =
            "application/json";

        await context.HttpContext.Response
            .WriteAsJsonAsync(
                new
                {
                    ok = false,
                    mensaje =
                        "Se alcanzó el límite temporal de solicitudes."
                },
                cancellationToken);
    };
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Analisis/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    IHeaderDictionary headers =
        context.Response.Headers;

    headers["X-Content-Type-Options"] =
        "nosniff";

    headers["X-Frame-Options"] =
        "DENY";

    headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";

    headers["Permissions-Policy"] =
        "camera=(), microphone=(self), geolocation=()";

    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' https://www.googletagmanager.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https://*.google-analytics.com https://www.googletagmanager.com; " +
        "font-src 'self'; " +
        "connect-src 'self' https://*.google-analytics.com https://*.analytics.google.com https://www.googletagmanager.com; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none';";

    await next();
});

app.UseSession();
app.UseRateLimiter();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapGet(
    "/robots.txt",
    (HttpContext context) =>
    {
        string baseUrl =
            $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";

        string robots =
            "User-agent: *\n" +
            "Allow: /\n" +
            "Disallow: /Cuenta/\n" +
            "Disallow: /AdministracionApi/\n" +
            "Disallow: /api/\n" +
            "Disallow: /openapi/\n" +
            $"Sitemap: {baseUrl}/sitemap.xml\n";

        return Results.Text(
            robots,
            "text/plain; charset=utf-8");
    })
    .ExcludeFromDescription();

app.MapGet(
    "/sitemap.xml",
    (HttpContext context) =>
    {
        string baseUrl =
            $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";

        string escapedBaseUrl =
            System.Security.SecurityElement.Escape(baseUrl)
            ?? baseUrl;

        string sitemap =
            $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url><loc>{escapedBaseUrl}/</loc></url>
              <url><loc>{escapedBaseUrl}/planes</loc></url>
              <url><loc>{escapedBaseUrl}/Contacto</loc></url>
              <url><loc>{escapedBaseUrl}/Legal/Privacidad</loc></url>
              <url><loc>{escapedBaseUrl}/Legal/Condiciones</loc></url>
              <url><loc>{escapedBaseUrl}/Legal/Terceros</loc></url>
            </urlset>
            """;

        return Results.Text(
            sitemap,
            "application/xml; charset=utf-8");
    })
    .ExcludeFromDescription();

app.MapOpenApi();

app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Analisis}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();