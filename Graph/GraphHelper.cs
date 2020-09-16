using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace family_calendar
{
    internal class EventHolder {
        string subject;
        Date date;
    }

    public interface IGraphHelper {
        public bool IsConnected();
        public void ListCalendarEvents();
    }

    public class GraphHelper : IGraphHelper
    {

        public bool IsConnected(){
            return graphClient != null;
        }

        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        public static async Task<User> GetMeAsync()
        {
            try
            {
                // GET /me
                return await graphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }

        // <GetEventsSnippet>
        public static async Task<IEnumerable<Event>> GetEventsAsync()
        {
            try
            {
                // GET /me/events
                var resultPage = await graphClient.Me.Events.Request()
                    // Only return the fields used by the application
                    .Select(e => new {
                      e.Subject,
                      e.Organizer,
                      e.Start,
                      e.End
                    })
                    // Sort results by when they were created, newest first
                    .OrderBy("createdDateTime DESC")
                    .GetAsync();

                return resultPage.CurrentPage;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting events: {ex.Message}");
                return null;
            }
        }
        // </GetEventsSnippet>
        public List<EventHolder> ListCalendarEvents()
        {
            var events = GetEventsAsync().Result;
            List<EventHolder> eventsList = new List<EventHolder>();

            foreach (var calendarEvent in events)
            {
                var eh = new EventHolder(){
                    eh.subject = calendarEvent.Subject;
                    eh.date = calendarEvent.Start;
                };
                eventsList.add(eh);
                /*Console.WriteLine($"Subject: {calendarEvent.Subject}");
                Console.WriteLine($"  Organizer: {calendarEvent.Organizer.EmailAddress.Name}");
                Console.WriteLine($"  Start: {FormatDateTimeTimeZone(calendarEvent.Start)}");
                Console.WriteLine($"  End: {FormatDateTimeTimeZone(calendarEvent.End)}");*/
            }
            return eventsList;
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
