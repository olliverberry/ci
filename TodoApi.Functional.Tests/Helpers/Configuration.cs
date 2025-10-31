namespace TodoApi.Functional.Tests.Helpers;

/// <summary>
/// Singleton class that holds configuration options for functional tests.
/// </summary>
public sealed class Configuration
{
    private static readonly Lazy<Configuration> _instance = new(() => new Configuration());

    /// <summary>
    /// Gets the singleton instance of the Configuration class.
    /// </summary>
    public static Configuration Instance => _instance.Value;

    // Private constructor to prevent instantiation
    private Configuration()
    {
        // Load configuration from environment variables with fallback defaults
        var host = GetEnvironmentVariable("API_HOST") 
            ?? GetEnvironmentVariable("HOST_NAME")?.Split(':')[0] 
            ?? "localhost";
        
        var port = int.TryParse(GetEnvironmentVariable("API_PORT"), out var parsedPort)
            ? parsedPort
            : (GetEnvironmentVariable("HOST_NAME")?.Contains(':') == true
                ? int.Parse(GetEnvironmentVariable("HOST_NAME")!.Split(':')[1])
                : 8080);

        // Allow full URL override, otherwise construct from host and port
        ApiBaseUrl = GetEnvironmentVariable("API_BASE_URL") 
            ?? $"http://{host}:{port}";
        
        RequestTimeout = int.TryParse(GetEnvironmentVariable("API_REQUEST_TIMEOUT"), out var timeout) 
            ? timeout 
            : 30;

        RetryCount = int.TryParse(GetEnvironmentVariable("API_RETRY_COUNT"), out var retries)
            ? retries
            : 3;

        RetryDelaySeconds = int.TryParse(GetEnvironmentVariable("API_RETRY_DELAY_SECONDS"), out var delay)
            ? delay
            : 2;
    }

    /// <summary>
    /// Gets the base URL for the API under test.
    /// Default: http://localhost:8080
    /// Environment variables: API_BASE_URL (full override), API_HOST, API_PORT, or HOST_NAME
    /// </summary>
    public string ApiBaseUrl { get; }

    /// <summary>
    /// Gets the HTTP request timeout in seconds.
    /// Default: 30 seconds
    /// Environment variable: API_REQUEST_TIMEOUT
    /// </summary>
    public int RequestTimeout { get; }

    /// <summary>
    /// Gets the number of retry attempts for failed requests.
    /// Default: 3
    /// Environment variable: API_RETRY_COUNT
    /// </summary>
    public int RetryCount { get; }

    /// <summary>
    /// Gets the delay in seconds between retry attempts.
    /// Default: 2 seconds
    /// Environment variable: API_RETRY_DELAY_SECONDS
    /// </summary>
    public int RetryDelaySeconds { get; }

    private static string? GetEnvironmentVariable(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    /// <summary>
    /// Provides a summary of the current configuration for debugging purposes.
    /// </summary>
    public override string ToString()
    {
        return $"API Base URL: {ApiBaseUrl}, " +
               $"Request Timeout: {RequestTimeout}s, " +
               $"Retry Count: {RetryCount}, " +
               $"Retry Delay: {RetryDelaySeconds}s";
    }
}

