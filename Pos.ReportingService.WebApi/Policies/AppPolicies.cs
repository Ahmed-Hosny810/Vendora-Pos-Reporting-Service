using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace Pos.ReportingService.WebApi.Policies
{
    public static class AppPolicies
    {
        public const string ViewReports = "Reports.View";
        public static IServiceCollection AddAppPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                AddRolePolicy(options, ViewReports, "TenantOwner", "Admin");
            });
            return services;
        }

        private static void RequireTenant(AuthorizationPolicyBuilder policy)
        {
            policy.AuthenticationSchemes.Add(
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

            policy.RequireAuthenticatedUser();

            RequireActiveSubscription(policy);

            policy.RequireClaim("user_type", "Tenant");

            policy.RequireAssertion(context =>
                Guid.TryParse(
                    context.User.FindFirstValue("tenant_id"),
                    out var tenantId)
                && tenantId != Guid.Empty
                && Guid.TryParse(
                    context.User.FindFirstValue(OpenIddictConstants.Claims.Subject)
                        ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out var userId)
                && userId != Guid.Empty);
        }

        private static void AddRolePolicy(
            AuthorizationOptions options,
            string policyName,
            params string[] roles)
        {
            options.AddPolicy(policyName, policy =>
            {
                RequireTenant(policy);
                policy.RequireRole(roles);
            });
        }

        private static void RequireActiveSubscription(AuthorizationPolicyBuilder policy)
        {
            policy.RequireClaim("business_access", "true");
            policy.RequireAssertion(context =>
            {
                var now = DateTimeOffset.UtcNow;

                return DateTimeOffset.TryParseExact(
                    context.User.FindFirst("subscription_access_valid_until")?.Value,
                    "O", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var validUntil)
                    && validUntil > now
                    && DateTimeOffset.TryParseExact(
                        context.User.FindFirst("subscription_ends_at")?.Value,
                        "O", CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out var subscriptionEndsAt)
                    && subscriptionEndsAt > now;
            });
        }
    }
}
