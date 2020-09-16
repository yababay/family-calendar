using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace family_calendar
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var graphHelper = scope.ServiceProvider.GetRequiredService<IGraphHelper>();
                bool isConnected = graphHelper.IsConnected();
                if(isConnected){
                    graphHelper.ListCalendarEvents();
                }
                else _logger.LogInformation("It's not yet connected...");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        static string FormatDateTimeTimeZone(Microsoft.Graph.DateTimeTimeZone value)
        {
            // Get the timezone specified in the Graph value
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(value.TimeZone);
            // Parse the date/time string from Graph into a DateTime
            var dateTime = DateTime.Parse(value.DateTime);

            // Create a DateTimeOffset in the specific timezone indicated by Graph
            var dateTimeWithTZ = new DateTimeOffset(dateTime, timeZone.BaseUtcOffset)
                .ToLocalTime();

            return dateTimeWithTZ.ToString("g");
        }
    }
}
