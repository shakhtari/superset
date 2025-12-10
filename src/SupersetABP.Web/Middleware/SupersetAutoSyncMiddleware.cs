using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Users;

namespace SupersetABP.Web.Middleware
{
    public class SupersetAutoSyncMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SupersetAutoSyncMiddleware> _logger;
        private const string SYNC_COOKIE_NAME = "superset_synced";

        public SupersetAutoSyncMiddleware(
            RequestDelegate next,
            ILogger<SupersetAutoSyncMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ICurrentUser currentUser)
        {
            await _next(context);
            if (currentUser.IsAuthenticated &&
                currentUser.Id.HasValue &&
                !context.Response.HasStarted)
            {
                var alreadySynced = context.Request.Cookies.ContainsKey(SYNC_COOKIE_NAME);

                if (!alreadySynced)
                {
                    // Fire-and-forget: Background sync
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _logger.LogInformation($"=== BACKGROUND SYNC STARTED: {currentUser.UserName} ===");

                            var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();

                            var username = currentUser.UserName;
                            var email = currentUser.Email;
                            var name = currentUser.Name;
                            var surname = currentUser.SurName;
                            var roles = currentUser.Roles?.ToArray() ?? new string[0];

                            var success = await SyncUserToSuperset(
                                username,
                                email,
                                name,
                                surname,
                                roles,
                                httpClientFactory,
                                _logger
                            );

                            if (success)
                            {
                                _logger.LogInformation($"✅ Background sync successful: {username}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Background sync failed");
                        }
                    });

                    // Cookie'yi hemen set et (tekrar denemesin diye)
                    context.Response.Cookies.Append(SYNC_COOKIE_NAME, "1", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddHours(24)
                    });
                }
            }

        }

        private static async Task<bool> SyncUserToSuperset(
            string username,
            string email,
            string firstName,
            string lastName,
            string[] roles,
            IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            try
            {
                var normalizedRoles = roles.Select(r => r.ToLower()).ToArray();

                var userData = new
                {
                    username = username ?? "unknown",
                    email = email ?? $"{username}@supersetdemo.com",
                    first_name = firstName ?? username ?? "User",
                    last_name = lastName ?? "",
                    roles = normalizedRoles.Length > 0 ? normalizedRoles : new[] { "user" }
                };

                var json = JsonSerializer.Serialize(userData);
                logger.LogInformation($"Sending to Superset: {json}");

                var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5); // Timeout ekle

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://localhost:8088/api/v1/auth/abp-sso",
                    content
                );

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation($"✅ Superset sync successful: {responseContent}");
                    return true;
                }
                else
                {
                    logger.LogWarning($"⚠️ Superset sync failed ({response.StatusCode}): {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SyncUserToSuperset");
                return false;
            }
        }
    }
}