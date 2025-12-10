using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;

namespace SupersetABP.SupersetUsers
{
    public class UserCreatedSupersetSyncHandler :
      IDistributedEventHandler<EntityCreatedEto<IdentityUser>>,
      ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserCreatedSupersetSyncHandler> _logger;

        public UserCreatedSupersetSyncHandler(
            IHttpClientFactory httpClientFactory,
            UserManager<IdentityUser> userManager,
            ILogger<UserCreatedSupersetSyncHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task HandleEventAsync(EntityCreatedEto<IdentityUser> eventData)
        {
            try
            {
                _logger.LogInformation($"=== NEW USER CREATED, SYNCING TO SUPERSET: {eventData.Entity.UserName} ===");

                var user = await _userManager.FindByIdAsync(eventData.Entity.Id.ToString());

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {eventData.Entity.Id}");
                    return;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userRoleNames = userRoles.Select(r => r.ToLower()).ToArray();

                await SyncUserToSuperset(
                    user.UserName,
                    user.Email,
                    user.Name,
                    user.Surname,
                    userRoleNames
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to sync new user to Superset");
            }
        }

        private async Task SyncUserToSuperset(
            string username,
            string email,
            string firstName,
            string lastName,
            string[] roles)
        {
            try
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
                _logger.LogInformation($"Sending to Superset: {json}");

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://localhost:8088/api/v1/auth/abp-sso",
                    content
                );

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ User {username} synced to Superset successfully");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Failed to sync user {username}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error syncing user {username} to Superset");
            }
        }
    }

    /// <summary>
    /// User Updated Event - User güncellendiğinde
    /// </summary>
    public class UserUpdatedSupersetSyncHandler :
        IDistributedEventHandler<EntityUpdatedEto<IdentityUser>>,
        ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UserUpdatedSupersetSyncHandler> _logger;

        public UserUpdatedSupersetSyncHandler(
            IHttpClientFactory httpClientFactory,
            UserManager<IdentityUser> userManager,
            ILogger<UserUpdatedSupersetSyncHandler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task HandleEventAsync(EntityUpdatedEto<IdentityUser> eventData)
        {
            try
            {
                _logger.LogInformation($"=== USER UPDATED, SYNCING TO SUPERSET: {eventData.Entity.UserName} ===");

                var user = await _userManager.FindByIdAsync(eventData.Entity.Id.ToString());

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {eventData.Entity.Id}");
                    return;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userRoleNames = userRoles.Select(r => r.ToLower()).ToArray();

                await SyncUserToSuperset(
                    user.UserName,
                    user.Email,
                    user.Name,
                    user.Surname,
                    userRoleNames
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to sync updated user to Superset");
            }
        }

        private async Task SyncUserToSuperset(
            string username,
            string email,
            string firstName,
            string lastName,
            string[] roles)
        {
            try
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
                _logger.LogInformation($"Updating user in Superset: {json}");

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://localhost:8088/api/v1/auth/abp-sso",
                    content
                );

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"✅ User {username} updated in Superset successfully");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Failed to update user {username}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {username} in Superset");
            }
        }
    }
}
