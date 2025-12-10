using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace SupersetABP.SupersetUsers
{
    public class SupersetUserAppService : ApplicationService, ISupersetUserAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IdentityUserManager _userManager; 
        private readonly IHttpClientFactory _httpClientFactory;

        public SupersetUserAppService(
            IIdentityUserRepository userRepository,
            IdentityUserManager userManager, 
            IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        public async Task<SupersetSyncResultDto> SyncAllUsersAsync()
        {
            var result = new SupersetSyncResultDto();

            try
            {
                Logger.LogInformation("=== SYNCING ALL USERS TO SUPERSET ===");

                var users = await _userRepository.GetListAsync();
                result.TotalUsers = users.Count;

                foreach (var user in users)
                {
                    try
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        var userRoleNames = userRoles.Select(r => r.ToLower()).ToArray();

                        await SyncUserToSuperset(
                            user.UserName,
                            user.Email,
                            user.Name,
                            user.Surname,
                            userRoleNames
                        );

                        result.SuccessCount++;
                        result.SyncedUsers.Add(user.UserName);
                    }
                    catch (Exception ex)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"{user.UserName}: {ex.Message}");
                        Logger.LogError(ex, $"Failed to sync user: {user.UserName}");
                    }
                }

                Logger.LogInformation($"Sync completed: {result.SuccessCount}/{result.TotalUsers} successful");
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Sync all users failed");
                throw;
            }
        }

        private async Task SyncUserToSuperset(
            string username,
            string email,
            string firstName,
            string lastName,
            string[] roles)
        {
            var userData = new
            {
                username = username,
                email = email ?? $"{username}@supersetdemo.com",
                first_name = firstName ?? username,
                last_name = lastName ?? "",
                roles = roles.Length > 0 ? roles : new[] { "user" }
            };

            var json = JsonSerializer.Serialize(userData);
            Logger.LogInformation($"Syncing user: {username}");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10); 

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "http://localhost:8088/api/v1/auth/abp-sso",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Superset API error ({response.StatusCode}): {error}");
            }
        }
    }
}