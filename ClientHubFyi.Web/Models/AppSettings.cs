namespace ClientHubFyi.Web.Models;

public class FyiApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}

public class McpServerSettings
{
    public bool Enabled { get; set; }
    public int Port { get; set; }
    public string Name { get; set; } = string.Empty;
}
