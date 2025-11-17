namespace ClientHubFyi.Web.Services;

public interface IMcpServerService
{
    /// <summary>
    /// Search for FYI clients by name or code
    /// </summary>
    Task<string> SearchFyiClientsAsync(string query);

    /// <summary>
    /// Get comprehensive overview of an FYI client including jobs and documents
    /// </summary>
    Task<string> GetFyiClientOverviewAsync(string clientId);

    /// <summary>
    /// Get calendar events for the next N days
    /// </summary>
    Task<string> GetCalendarEventsAsync(int daysAhead = 7);
}
