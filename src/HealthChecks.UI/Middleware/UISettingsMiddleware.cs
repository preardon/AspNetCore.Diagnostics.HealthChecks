using System.Text.Json;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Middleware
{
    internal class UISettingsMiddleware
    {
        private readonly object _uiOutputSettings;

        public UISettingsMiddleware(RequestDelegate next, IOptions<Settings> settings)
        {
            _ = next;
            _ = Guard.ThrowIfNull(settings);
            _uiOutputSettings = new
            {
                PollingInterval = settings.Value.EvaluationTimeInSeconds,
                HeaderText = settings.Value.HeaderText
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //TODO: switch to STJ and write directly into response body
            string content = JsonSerializer.Serialize(_uiOutputSettings, JsonSeraliserSettings.Options);
            context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

            await context.Response.WriteAsync(content);
        }
    }
}
