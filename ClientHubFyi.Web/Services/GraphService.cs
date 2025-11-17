using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace ClientHubFyi.Web.Services;

public class GraphService : IGraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GraphService> _logger;

    public GraphService(GraphServiceClient graphClient, ILogger<GraphService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<User?> GetUserProfileAsync()
    {
        try
        {
            var user = await _graphClient.Me.GetAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile from Microsoft Graph");
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetCalendarEventsAsync(int daysAhead = 7)
    {
        try
        {
            var startDateTime = DateTime.UtcNow;
            var endDateTime = startDateTime.AddDays(daysAhead);

            var events = await _graphClient.Me.Calendar.CalendarView
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.StartDateTime = startDateTime.ToString("o");
                    requestConfiguration.QueryParameters.EndDateTime = endDateTime.ToString("o");
                    requestConfiguration.QueryParameters.Select = new[]
                    {
                        "subject",
                        "start",
                        "end",
                        "location",
                        "organizer",
                        "attendees",
                        "webLink",
                        "isOnlineMeeting",
                        "onlineMeetingUrl"
                    };
                    requestConfiguration.QueryParameters.Orderby = new[] { "start/dateTime" };
                    requestConfiguration.QueryParameters.Top = 50;
                });

            return events?.Value ?? Enumerable.Empty<Event>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting calendar events from Microsoft Graph");
            throw;
        }
    }

    public async Task<IEnumerable<TodoTask>> GetTasksAsync()
    {
        try
        {
            var taskLists = await GetTaskListsAsync();
            var allTasks = new List<TodoTask>();

            foreach (var taskList in taskLists)
            {
                if (taskList.Id != null)
                {
                    var tasks = await _graphClient.Me.Todo.Lists[taskList.Id].Tasks
                        .GetAsync(requestConfiguration =>
                        {
                            requestConfiguration.QueryParameters.Select = new[]
                            {
                                "title",
                                "status",
                                "importance",
                                "createdDateTime",
                                "lastModifiedDateTime",
                                "dueDateTime",
                                "completedDateTime",
                                "body"
                            };
                            requestConfiguration.QueryParameters.Filter = "status ne 'completed'";
                            requestConfiguration.QueryParameters.Top = 50;
                        });

                    if (tasks?.Value != null)
                    {
                        allTasks.AddRange(tasks.Value);
                    }
                }
            }

            return allTasks.OrderBy(t => t.DueDateTime?.DateTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks from Microsoft Graph");
            throw;
        }
    }

    public async Task<IEnumerable<TodoTaskList>> GetTaskListsAsync()
    {
        try
        {
            var taskLists = await _graphClient.Me.Todo.Lists
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[]
                    {
                        "displayName",
                        "isOwner",
                        "isShared"
                    };
                });

            return taskLists?.Value ?? Enumerable.Empty<TodoTaskList>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task lists from Microsoft Graph");
            throw;
        }
    }
}
