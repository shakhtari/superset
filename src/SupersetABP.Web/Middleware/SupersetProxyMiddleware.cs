using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SupersetABP.Web.Middleware
{
    public class SupersetProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SupersetProxyMiddleware> _logger;

        public SupersetProxyMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            ILogger<SupersetProxyMiddleware> logger)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // Check if this is a Superset request
            if (ShouldProxy(path))
            {
                await ProxyRequest(context);
                return;
            }

            await _next(context);
        }

        private bool ShouldProxy(string path)
        {
            if (path.StartsWith("/Identity", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/Abp", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/abp", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/SupersetDashboard", StringComparison.OrdinalIgnoreCase) || // ✅ EKLE
                path.StartsWith("/Dashboard", StringComparison.OrdinalIgnoreCase) || // ✅ EKLE
                path.StartsWith("/HostDashboard", StringComparison.OrdinalIgnoreCase) || // ✅ EKLE
                path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_vs", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/api/Superset", StringComparison.OrdinalIgnoreCase)) // ✅ EKLE
            {
                return false;
            }

            var proxyPaths = new[]
            {
                "/superset-proxy", // En spesifik
                "/static/assets",
                "/api/v1/",
                "/superset/dashboard", // Daha spesifik
                "/superset/explore",
                "/superset/sqllab",
                "/chart/",
                "/explore/",
                "/sqllab/"
            };

            return proxyPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
        private async Task ProxyRequest(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value ?? "";

                // Remove /superset-proxy prefix if present
                if (path.StartsWith("/superset-proxy", StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring("/superset-proxy".Length);
                    if (string.IsNullOrEmpty(path))
                    {
                        path = "/";
                    }
                }

                var targetUrl = $"http://localhost:8088{path}";
                var queryString = context.Request.QueryString.ToString();
                if (!string.IsNullOrEmpty(queryString))
                {
                    targetUrl += queryString;
                }

                _logger.LogDebug($"Proxying: {context.Request.Method} {targetUrl}");

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var request = new HttpRequestMessage(
                    new HttpMethod(context.Request.Method),
                    targetUrl
                );

                // Copy headers
                foreach (var header in context.Request.Headers)
                {
                    if (!IsRestrictedHeader(header.Key))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                // Copy body for POST/PUT/PATCH
                if (context.Request.ContentLength > 0 &&
                    (context.Request.Method == "POST" ||
                     context.Request.Method == "PUT" ||
                     context.Request.Method == "PATCH"))
                {
                    var ms = new MemoryStream();
                    await context.Request.Body.CopyToAsync(ms);
                    ms.Position = 0;
                    request.Content = new StreamContent(ms);

                    if (!string.IsNullOrEmpty(context.Request.ContentType))
                    {
                        try
                        {
                            request.Content.Headers.ContentType =
                                new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
                        }
                        catch
                        {
                            // Ignore invalid content type
                        }
                    }
                }

                var response = await client.SendAsync(request);

                // Copy response status
                context.Response.StatusCode = (int)response.StatusCode;

                // Copy response headers
                foreach (var header in response.Headers)
                {
                    if (!IsRestrictedHeader(header.Key))
                    {
                        try
                        {
                            context.Response.Headers[header.Key] = header.Value.ToArray();
                        }
                        catch
                        {
                            // Ignore invalid headers
                        }
                    }
                }

                foreach (var header in response.Content.Headers)
                {
                    if (!IsRestrictedHeader(header.Key))
                    {
                        try
                        {
                            context.Response.Headers[header.Key] = header.Value.ToArray();
                        }
                        catch
                        {
                            // Ignore invalid headers
                        }
                    }
                }

                // Copy response body
                await response.Content.CopyToAsync(context.Response.Body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Proxy error for path: {context.Request.Path}");

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync($"{{\"error\": \"{ex.Message}\"}}");
                }
            }
        }

        private static bool IsRestrictedHeader(string headerName)
        {
            var restricted = new[]
            {
                "Host",
                "Content-Length",
                "Transfer-Encoding",
                "Connection",
                "Keep-Alive",
                "Upgrade",
                "Proxy-Connection",
                "Proxy-Authenticate",
                "Proxy-Authorization",
                "TE",
                "Trailers",
                "Upgrade-Insecure-Requests"
            };

            return restricted.Contains(headerName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
