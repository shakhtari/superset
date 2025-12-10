using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupersetABP.SupersetUsers;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Users;

namespace SupersetABP.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [IgnoreAntiforgeryToken]
    public class SupersetController : AbpControllerBase
    {
        private readonly ILogger<SupersetController> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IHttpClientFactory _httpClientFactory;

        public SupersetController(
            ILogger<SupersetController> logger,
            ICurrentUser currentUser,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _currentUser = currentUser;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate()
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                {
                    _logger.LogWarning("❌ Unauthenticated user");
                    return Ok(new { success = false, error = "Not authenticated" });
                }

                var username = _currentUser.UserName;
                var email = _currentUser.Email ?? $"{username}@supersetabp.com";
                var firstName = _currentUser.Name ?? username;
                var lastName = _currentUser.SurName ?? "";
                var roles = _currentUser.Roles?.ToArray() ?? new string[0];

                _logger.LogInformation($"=== SYNC TO SUPERSET: {username} ===");

                // Normalize roles
                var normalizedRoles = roles.Select(r => r.ToLower()).ToArray();
                if (normalizedRoles.Length == 0)
                {
                    normalizedRoles = new[] { "user" };
                }

                var userData = new
                {
                    username = username,
                    email = email,
                    first_name = firstName,
                    last_name = lastName,
                    roles = normalizedRoles
                };

                var json = JsonSerializer.Serialize(userData);
                _logger.LogInformation($"Payload: {json}");

                // Call Superset SSO
                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://localhost:8088/api/v1/auth/abp-sso",
                    content
                );

                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ Superset response: {responseText}");

                    return Ok(new
                    {
                        success = true,
                        message = "User synced successfully"
                    });
                }
                else
                {
                    _logger.LogError($"❌ Superset error ({response.StatusCode}): {responseText}");

                    return Ok(new
                    {
                        success = false,
                        error = $"Superset sync failed: {responseText}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception in authenticate");
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                user = _currentUser.UserName,
                authenticated = _currentUser.IsAuthenticated,
                roles = _currentUser.Roles
            });
        }
    }
}