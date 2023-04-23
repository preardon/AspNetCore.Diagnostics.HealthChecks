using System.Text.Json;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Middleware
{
    internal class UIApiEndpointMiddleware
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Settings _settings;

        public UIApiEndpointMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, IOptions<Settings> settings)
        {
            _ = next;
            _serviceScopeFactory = serviceScopeFactory;
            _settings = Guard.ThrowIfNull(settings?.Value);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<HealthChecksDb>();

            var healthChecks = await db.Configurations.ToListAsync();

            var healthChecksExecutions = new List<HealthCheckExecution>();

            foreach (var item in healthChecks.OrderBy(h => h.Id))
            {
                var execution = await db.Executions
                            .Include(le => le.Entries)
                            .Where(le => le.Name == item.Name)
                            .AsNoTracking()
                            .SingleOrDefaultAsync();

                if (execution != null)
                {
                    execution.History = await db.HealthCheckExecutionHistories
                        .Where(eh => EF.Property<int>(eh, "HealthCheckExecutionId") == execution.Id)
                        .OrderByDescending(eh => eh.On)
                        .Take(_settings.MaximumExecutionHistoriesPerEndpoint)
                        .ToListAsync();

                    healthChecksExecutions.Add(execution);
                }
            }

            var responseContent = JsonSerializer.Serialize(healthChecksExecutions, JsonSeraliserSettings.Options);
            context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

            await context.Response.WriteAsync(responseContent);
        }
    }
}
