using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ClientHubFyi.Web.Models;

namespace ClientHubFyi.Web.Services;

public class FyiApiClient : IFyiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly FyiApiSettings _settings;
    private readonly ILogger<FyiApiClient> _logger;
    private FyiAuthToken? _currentToken;
    private readonly SemaphoreSlim _authLock = new(1, 1);

    public FyiApiClient(
        HttpClient httpClient,
        IOptions<FyiApiSettings> settings,
        ILogger<FyiApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Configure base address
        if (!string.IsNullOrEmpty(_settings.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }
    }

    public async Task<FyiAuthToken> AuthenticateAsync()
    {
        await _authLock.WaitAsync();
        try
        {
            // Return cached token if still valid
            if (_currentToken != null && _currentToken.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
            {
                return _currentToken;
            }

            _logger.LogInformation("Authenticating with FYI API...");

            // FYI API authentication endpoint - adjust based on actual API docs
            var authRequest = new
            {
                apiKey = _settings.ApiKey,
                apiSecret = _settings.ApiSecret
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/token", authRequest);
            response.EnsureSuccessStatusCode();

            _currentToken = await response.Content.ReadFromJsonAsync<FyiAuthToken>()
                ?? throw new InvalidOperationException("Failed to deserialize auth token");

            _currentToken.ExpiresAt = DateTime.UtcNow.AddSeconds(_currentToken.ExpiresIn);

            _logger.LogInformation("Successfully authenticated with FYI API");
            return _currentToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with FYI API");
            throw;
        }
        finally
        {
            _authLock.Release();
        }
    }

    public async Task<IReadOnlyList<FyiClient>> SearchClientsAsync(string query)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Searching for clients with query: {Query}", query);

            // Adjust endpoint based on actual FYI API documentation
            var response = await _httpClient.GetAsync($"/api/clients/search?q={Uri.EscapeDataString(query)}");
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<FyiClient>>()
                ?? new List<FyiClient>();

            _logger.LogInformation("Found {Count} clients matching query", clients.Count);
            return clients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for clients");

            // Return mock data for demonstration purposes
            return GetMockClients(query);
        }
    }

    public async Task<FyiClientDetail> GetClientDetailAsync(string clientId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Getting client detail for ID: {ClientId}", clientId);

            var response = await _httpClient.GetAsync($"/api/clients/{clientId}");
            response.EnsureSuccessStatusCode();

            var client = await response.Content.ReadFromJsonAsync<FyiClientDetail>()
                ?? throw new InvalidOperationException($"Client {clientId} not found");

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client detail");

            // Return mock data for demonstration purposes
            return GetMockClientDetail(clientId);
        }
    }

    public async Task<IReadOnlyList<FyiJob>> GetClientJobsAsync(string clientId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Getting jobs for client: {ClientId}", clientId);

            var response = await _httpClient.GetAsync($"/api/clients/{clientId}/jobs");
            response.EnsureSuccessStatusCode();

            var jobs = await response.Content.ReadFromJsonAsync<List<FyiJob>>()
                ?? new List<FyiJob>();

            return jobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client jobs");

            // Return mock data for demonstration purposes
            return GetMockJobs(clientId);
        }
    }

    public async Task<IReadOnlyList<FyiDocument>> GetJobDocumentsAsync(string jobId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Getting documents for job: {JobId}", jobId);

            var response = await _httpClient.GetAsync($"/api/jobs/{jobId}/documents");
            response.EnsureSuccessStatusCode();

            var documents = await response.Content.ReadFromJsonAsync<List<FyiDocument>>()
                ?? new List<FyiDocument>();

            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job documents");

            // Return mock data for demonstration purposes
            return GetMockDocuments(jobId);
        }
    }

    public async Task<IReadOnlyList<FyiTask>> GetJobTasksAsync(string jobId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Getting tasks for job: {JobId}", jobId);

            var response = await _httpClient.GetAsync($"/api/jobs/{jobId}/tasks");
            response.EnsureSuccessStatusCode();

            var tasks = await response.Content.ReadFromJsonAsync<List<FyiTask>>()
                ?? new List<FyiTask>();

            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job tasks");

            // Return mock data for demonstration purposes
            return GetMockTasks(jobId);
        }
    }

    public async Task<IReadOnlyList<FyiEmail>> GetClientEmailsAsync(string clientId)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Getting emails for client: {ClientId}", clientId);

            var response = await _httpClient.GetAsync($"/api/clients/{clientId}/emails");
            response.EnsureSuccessStatusCode();

            var emails = await response.Content.ReadFromJsonAsync<List<FyiEmail>>()
                ?? new List<FyiEmail>();

            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client emails");

            // Return mock data for demonstration purposes
            return GetMockEmails(clientId);
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_currentToken == null || _currentToken.ExpiresAt <= DateTime.UtcNow.AddMinutes(5))
        {
            await AuthenticateAsync();
        }

        if (_currentToken != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(_currentToken.TokenType, _currentToken.AccessToken);
        }
    }

    #region Mock Data Methods - Remove when connecting to real API

    private IReadOnlyList<FyiClient> GetMockClients(string query)
    {
        _logger.LogWarning("Returning mock client data - FYI API not configured or unreachable");

        return new List<FyiClient>
        {
            new() { Id = "1", Name = "Acme Corporation", Code = "ACME001", Email = "info@acme.com", Phone = "555-0100", Type = "Company" },
            new() { Id = "2", Name = "Smith & Associates", Code = "SMTH002", Email = "contact@smith.com", Phone = "555-0200", Type = "Partnership" },
            new() { Id = "3", Name = "Johnson Enterprises", Code = "JOHN003", Email = "hello@johnson.com", Phone = "555-0300", Type = "Company" }
        }.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                     c.Code.Contains(query, StringComparison.OrdinalIgnoreCase))
         .ToList();
    }

    private FyiClientDetail GetMockClientDetail(string clientId)
    {
        _logger.LogWarning("Returning mock client detail - FYI API not configured or unreachable");

        return new FyiClientDetail
        {
            Id = clientId,
            Name = "Acme Corporation",
            Code = "ACME001",
            Email = "info@acme.com",
            Phone = "555-0100",
            Type = "Company",
            Jobs = GetMockJobs(clientId).ToList(),
            Contacts = new List<FyiContact>
            {
                new() { Id = "c1", ClientId = clientId, FirstName = "John", LastName = "Doe", Email = "john@acme.com", Phone = "555-0101", Position = "CEO", IsPrimary = true },
                new() { Id = "c2", ClientId = clientId, FirstName = "Jane", LastName = "Smith", Email = "jane@acme.com", Phone = "555-0102", Position = "CFO", IsPrimary = false }
            }
        };
    }

    private IReadOnlyList<FyiJob> GetMockJobs(string clientId)
    {
        _logger.LogWarning("Returning mock jobs data - FYI API not configured or unreachable");

        return new List<FyiJob>
        {
            new()
            {
                Id = "j1",
                ClientId = clientId,
                Name = "2024 Tax Return",
                Code = "TAX2024",
                Status = "In Progress",
                StartDate = DateTime.Now.AddMonths(-2),
                DueDate = DateTime.Now.AddMonths(1),
                Manager = "Sarah Johnson",
                Documents = GetMockDocuments("j1").ToList(),
                Tasks = GetMockTasks("j1").ToList()
            },
            new()
            {
                Id = "j2",
                ClientId = clientId,
                Name = "Q4 2024 Review",
                Code = "Q42024",
                Status = "Completed",
                StartDate = DateTime.Now.AddMonths(-4),
                DueDate = DateTime.Now.AddMonths(-1),
                Manager = "Michael Brown",
                Documents = GetMockDocuments("j2").ToList(),
                Tasks = GetMockTasks("j2").ToList()
            }
        };
    }

    private IReadOnlyList<FyiDocument> GetMockDocuments(string jobId)
    {
        _logger.LogWarning("Returning mock documents data - FYI API not configured or unreachable");

        return new List<FyiDocument>
        {
            new()
            {
                Id = "d1",
                JobId = jobId,
                Name = "Financial Statements.pdf",
                Type = "PDF",
                Size = 1024567,
                CreatedDate = DateTime.Now.AddDays(-30),
                ModifiedDate = DateTime.Now.AddDays(-5),
                CreatedBy = "John Doe",
                Status = "Final"
            },
            new()
            {
                Id = "d2",
                JobId = jobId,
                Name = "Supporting Documents.xlsx",
                Type = "Excel",
                Size = 524288,
                CreatedDate = DateTime.Now.AddDays(-25),
                ModifiedDate = DateTime.Now.AddDays(-3),
                CreatedBy = "Jane Smith",
                Status = "Draft"
            }
        };
    }

    private IReadOnlyList<FyiTask> GetMockTasks(string jobId)
    {
        _logger.LogWarning("Returning mock tasks data - FYI API not configured or unreachable");

        return new List<FyiTask>
        {
            new()
            {
                Id = "t1",
                JobId = jobId,
                Title = "Review financial statements",
                Description = "Complete review of all financial statements",
                Status = "In Progress",
                Priority = "High",
                DueDate = DateTime.Now.AddDays(7),
                AssignedTo = "Sarah Johnson"
            },
            new()
            {
                Id = "t2",
                JobId = jobId,
                Title = "Client sign-off",
                Description = "Obtain client signature on documents",
                Status = "Pending",
                Priority = "Medium",
                DueDate = DateTime.Now.AddDays(14),
                AssignedTo = "Michael Brown"
            }
        };
    }

    private IReadOnlyList<FyiEmail> GetMockEmails(string clientId)
    {
        _logger.LogWarning("Returning mock emails data - FYI API not configured or unreachable");

        return new List<FyiEmail>
        {
            new()
            {
                Id = "e1",
                Subject = "Q4 Financial Review Meeting",
                From = "john@acme.com",
                To = "sarah@firm.com",
                ReceivedDate = DateTime.Now.AddDays(-5),
                HasAttachments = true,
                RelatedClientId = clientId,
                RelatedJobId = "j1"
            },
            new()
            {
                Id = "e2",
                Subject = "Document Request",
                From = "jane@acme.com",
                To = "michael@firm.com",
                ReceivedDate = DateTime.Now.AddDays(-10),
                HasAttachments = false,
                RelatedClientId = clientId,
                RelatedJobId = "j2"
            }
        };
    }

    #endregion
}
