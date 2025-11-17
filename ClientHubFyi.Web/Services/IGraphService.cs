using Microsoft.Graph.Models;

namespace ClientHubFyi.Web.Services;

public interface IGraphService
{
    /// <summary>
    /// Get the current user's profile information
    /// </summary>
    Task<User?> GetUserProfileAsync();

    /// <summary>
    /// Get calendar events for the next specified number of days
    /// </summary>
    Task<IEnumerable<Event>> GetCalendarEventsAsync(int daysAhead = 7);

    /// <summary>
    /// Get the user's tasks from Microsoft To Do
    /// </summary>
    Task<IEnumerable<TodoTask>> GetTasksAsync();

    /// <summary>
    /// Get all task lists for the user
    /// </summary>
    Task<IEnumerable<TodoTaskList>> GetTaskListsAsync();
}
