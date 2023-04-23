using System.Text.Json;
using System.Text.RegularExpressions;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core
{
    public class UIWebHooksApiMiddleware
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UIWebHooksApiMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _ = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>();
            var sanitizedWebhooksResponse = settings.Value.Webhooks.Select(item => new
            {
                item.Name,
                Payload = string.IsNullOrEmpty(item.Payload) ? null : JsonSerializer.Serialize(Regex.Unescape(item.Payload), JsonSeraliserSettings.Options)
            });
            context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;
            var response = JsonSerializer.Serialize(sanitizedWebhooksResponse, JsonSeraliserSettings.Options);

            await context.Response.WriteAsync(response);
        }
    }
}
