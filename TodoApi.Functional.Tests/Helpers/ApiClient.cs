using System.Net.Http.Json;
using System.Net;
using TodoApi.Models;

namespace TodoApi.Functional.Tests.Helpers;

public class ApiClient
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress =
            new Uri($"https://{Environment.GetEnvironmentVariable("HOST_NAME")}" ?? "https://localhost"),
    };

    public async Task<IEnumerable<TodoItem>> GetTodosAsync()
    {
        var response = await _httpClient.GetAsync("/api/TodoItems");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<TodoItem>>()
            ?? throw new InvalidOperationException("Failed to deserialize TodoItems");
    }

    public async Task<TodoItem?> GetTodoByIdAsync(long id)
    {
        var response = await _httpClient.GetAsync($"/api/TodoItems/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoItem>()
            ?? throw new InvalidOperationException("Failed to deserialize TodoItem");
    }

    public async Task<TodoItem> CreateTodoAsync(TodoItem item)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/TodoItems", item);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoItem>()
            ?? throw new InvalidOperationException("Failed to deserialize created TodoItem");
    }

    public async Task<HttpStatusCode> UpdateTodoAsync(long id, TodoItem item)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/TodoItems/{id}", item);
        return response.StatusCode;
    }

    public async Task<HttpStatusCode> DeleteTodoAsync(long id)
    {
        var response = await _httpClient.DeleteAsync($"/api/TodoItems/{id}");
        return response.StatusCode;
    }
}