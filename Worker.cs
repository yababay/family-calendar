using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Iot.Device.Media;

namespace family_calendar
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SoundConnectionSettings audioSettings = new SoundConnectionSettings();

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
                    List<EventHolder> eventHolders = graphHelper.ListCalendarEvents();
                    foreach(var eventHolder in eventHolders)
                    {
                        Console.WriteLine(eventHolder.Date.ToString("g"));
                        Console.WriteLine(eventHolder.Subject);
                        Console.WriteLine(eventHolder.Category);
                        using SoundDevice device = SoundDevice.Create(audioSettings);
                        string sound;
                        var themeToSound = GetSoundsDictionary();
                        if (themeToSound.TryGetValue(eventHolder.Category, out sound))
                        {
                            device.Play($"./Assets/{sound}.wav");
                        }
                    }
                }
                else _logger.LogInformation("It's not yet connected...");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private static string FormatDateTimeTimeZone(Microsoft.Graph.DateTimeTimeZone value)
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

        private static Dictionary<string, string> GetSoundsDictionary(){
            var jsonUtf8Bytes = System.IO.File.ReadAllBytes("./categories2sound.json");
            var readOnlySpan  = new ReadOnlySpan<byte>(jsonUtf8Bytes);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(readOnlySpan);
        }
    }
}
