using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace family_calendar
{
    public class EventHolder {
        public string Subject;
        public string Category = "default";
        public DateTimeOffset Date;
    }

    public interface IGraphHelper {
        public bool IsConnected();
        public List<EventHolder> ListCalendarEvents();
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
                      e.Start,
                      e.Categories,
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
                //Console.WriteLine(calendarEvent.Subject);
                DateTimeTimeZone start = calendarEvent.Start;
                DateTimeOffset startOffset = OffsetOfDateTimeTimeZone(start);
                //Console.WriteLine(startOffset.ToString("g"));
                DateTimeOffset current = new DateTimeOffset(DateTime.Now);
                DateTimeOffset plus10 = new DateTimeOffset(DateTime.Now).AddMinutes(10);
                if(startOffset.CompareTo(current) < 0){
                    //Console.WriteLine("The date is in the past.");
                    continue;
                }
                if(startOffset.CompareTo(plus10) > 0) {
                    //Console.WriteLine("The date is in the future.");
                    continue;
                }
                var catEnum = calendarEvent.Categories.GetEnumerator();
                if(!catEnum.MoveNext()){
                    //Console.WriteLine("No category.");
                    continue;
                }
                var eventHolder = new EventHolder();
                eventHolder.Subject = calendarEvent.Subject;
                eventHolder.Category = catEnum.Current;
                eventHolder.Date = startOffset;
                eventsList.Add(eventHolder);
            }
            return eventsList;
        }

        static DateTimeOffset OffsetOfDateTimeTimeZone(Microsoft.Graph.DateTimeTimeZone value)
        {
            // Get the timezone specified in the Graph value
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(value.TimeZone);
            // Parse the date/time string from Graph into a DateTime
            var dateTime = DateTime.Parse(value.DateTime);

            // Create a DateTimeOffset in the specific timezone indicated by Graph
            var dateTimeWithTZ = new DateTimeOffset(dateTime, timeZone.BaseUtcOffset)
                .ToLocalTime();

            return dateTimeWithTZ;
        }
    }
}
