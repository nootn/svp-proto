using ClientHubFyi.Web.Models;

namespace ClientHubFyi.Web.Services;

public interface IFyiApiClient
{
    /// <summary>
    /// Authenticate with the FYI API and get an access token
    /// </summary>
    Task<FyiAuthToken> AuthenticateAsync();

    /// <summary>
    /// Search for clients by name or code
    /// </summary>
    Task<IReadOnlyList<FyiClient>> SearchClientsAsync(string query);

    /// <summary>
    /// Get detailed information about a specific client
    /// </summary>
    Task<FyiClientDetail> GetClientDetailAsync(string clientId);

    /// <summary>
    /// Get all jobs for a specific client
    /// </summary>
    Task<IReadOnlyList<FyiJob>> GetClientJobsAsync(string clientId);

    /// <summary>
    /// Get all documents for a specific job
    /// </summary>
    Task<IReadOnlyList<FyiDocument>> GetJobDocumentsAsync(string jobId);

    /// <summary>
    /// Get all tasks for a specific job
    /// </summary>
    Task<IReadOnlyList<FyiTask>> GetJobTasksAsync(string jobId);

    /// <summary>
    /// Get emails related to a client
    /// </summary>
    Task<IReadOnlyList<FyiEmail>> GetClientEmailsAsync(string clientId);
}
