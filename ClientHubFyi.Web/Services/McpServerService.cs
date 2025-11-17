using System.Text.Json;
using ClientHubFyi.Web.Models;

namespace ClientHubFyi.Web.Services;

public class McpServerService : IMcpServerService
{
    private readonly IFyiApiClient _fyiApi;
    private readonly IGraphService _graphService;
    private readonly ILogger<McpServerService> _logger;

    public McpServerService(
        IFyiApiClient fyiApi,
        IGraphService graphService,
        ILogger<McpServerService> logger)
    {
        _fyiApi = fyiApi;
        _graphService = graphService;
        _logger = logger;
    }

    public async Task<string> SearchFyiClientsAsync(string query)
    {
        try
        {
            _logger.LogInformation("MCP Tool: Searching FYI clients with query: {Query}", query);

            var clients = await _fyiApi.SearchClientsAsync(query);

            var result = new
            {
                success = true,
                query,
                count = clients.Count,
                clients = clients.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    code = c.Code,
                    email = c.Email,
                    phone = c.Phone,
                    type = c.Type
                })
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP tool: search_fyi_clients");
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    public async Task<string> GetFyiClientOverviewAsync(string clientId)
    {
        try
        {
            _logger.LogInformation("MCP Tool: Getting FYI client overview for ID: {ClientId}", clientId);

            var client = await _fyiApi.GetClientDetailAsync(clientId);

            var result = new
            {
                success = true,
                client = new
                {
                    id = client.Id,
                    name = client.Name,
                    code = client.Code,
                    email = client.Email,
                    phone = client.Phone,
                    type = client.Type,
                    contactsCount = client.Contacts?.Count ?? 0,
                    contacts = client.Contacts?.Select(c => new
                    {
                        name = $"{c.FirstName} {c.LastName}",
                        email = c.Email,
                        phone = c.Phone,
                        position = c.Position,
                        isPrimary = c.IsPrimary
                    }),
                    jobsCount = client.Jobs?.Count ?? 0,
                    jobs = client.Jobs?.Select(j => new
                    {
                        id = j.Id,
                        name = j.Name,
                        code = j.Code,
                        status = j.Status,
                        manager = j.Manager,
                        startDate = j.StartDate,
                        dueDate = j.DueDate,
                        documentsCount = j.Documents?.Count ?? 0,
                        documents = j.Documents?.Select(d => new
                        {
                            id = d.Id,
                            name = d.Name,
                            type = d.Type,
                            size = d.Size,
                            status = d.Status,
                            modifiedDate = d.ModifiedDate
                        }),
                        tasksCount = j.Tasks?.Count ?? 0,
                        tasks = j.Tasks?.Select(t => new
                        {
                            id = t.Id,
                            title = t.Title,
                            description = t.Description,
                            status = t.Status,
                            priority = t.Priority,
                            dueDate = t.DueDate,
                            assignedTo = t.AssignedTo
                        })
                    })
                }
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP tool: get_fyi_client_overview");
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    public async Task<string> GetCalendarEventsAsync(int daysAhead = 7)
    {
        try
        {
            _logger.LogInformation("MCP Tool: Getting calendar events for next {Days} days", daysAhead);

            var events = await _graphService.GetCalendarEventsAsync(daysAhead);

            var result = new
            {
                success = true,
                daysAhead,
                count = events.Count(),
                events = events.Select(e => new
                {
                    subject = e.Subject,
                    start = e.Start?.DateTime,
                    end = e.End?.DateTime,
                    location = e.Location?.DisplayName,
                    organizer = e.Organizer?.EmailAddress?.Name,
                    isOnlineMeeting = e.IsOnlineMeeting,
                    onlineMeetingUrl = e.OnlineMeetingUrl,
                    webLink = e.WebLink
                })
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MCP tool: get_calendar_events");
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}
