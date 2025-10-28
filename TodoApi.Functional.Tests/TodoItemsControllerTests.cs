using System.Net;
using FluentAssertions;
using TodoApi.Functional.Tests.Helpers;
using TodoApi.Models;

namespace TodoApi.Functional.Tests;

[TestFixture]
public class TodoItemsControllerTests
{
    private ApiClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _client = new ApiClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _client ??= new ApiClient();
        var todos = await _client.GetTodosAsync();
        await Parallel.ForEachAsync(todos, async (todo, _) =>
        {
            await _client.DeleteTodoAsync(todo.Id);
        });
    }

    [Test]
    public async Task GetTodos_ReturnsListOfTodoItems()
    {
        // Act
        var todos = await _client.GetTodosAsync();

        // Assert
        todos.Should().NotBeNull();
        todos.Should().BeAssignableTo<IEnumerable<TodoItem>>();
    }

    [Test]
    public async Task CreateTodo_CreatesNewTodoItem()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Name = "Test Todo Item",
            IsComplete = false
        };

        // Act
        var createdTodo = await _client.CreateTodoAsync(newTodo);

        // Assert
        createdTodo.Should().NotBeNull();
        createdTodo.Id.Should().BeGreaterThan(0);
        createdTodo.Name.Should().Be(newTodo.Name);
        createdTodo.IsComplete.Should().Be(newTodo.IsComplete);

        // Cleanup
        await _client.DeleteTodoAsync(createdTodo.Id);
    }

    [Test]
    public async Task GetTodoById_ReturnsCorrectTodo()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Name = "Todo to Find",
            IsComplete = false
        };
        var createdTodo = await _client.CreateTodoAsync(newTodo);

        // Act
        var retrievedTodo = await _client.GetTodoByIdAsync(createdTodo.Id);

        // Assert
        retrievedTodo.Should().NotBeNull();
        retrievedTodo!.Id.Should().Be(createdTodo.Id);
        retrievedTodo.Name.Should().Be(createdTodo.Name);
        retrievedTodo.IsComplete.Should().Be(createdTodo.IsComplete);

        // Cleanup
        await _client.DeleteTodoAsync(createdTodo.Id);
    }

    [Test]
    public async Task GetTodoById_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        const long nonExistentId = 999999;

        // Act
        var todo = await _client.GetTodoByIdAsync(nonExistentId);

        // Assert
        todo.Should().BeNull();
    }

    [Test]
    public async Task UpdateTodo_UpdatesExistingTodo()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Name = "Original Name",
            IsComplete = false
        };
        var createdTodo = await _client.CreateTodoAsync(newTodo);

        var updatedTodo = new TodoItem
        {
            Id = createdTodo.Id,
            Name = "Updated Name",
            IsComplete = true
        };

        // Act
        var statusCode = await _client.UpdateTodoAsync(createdTodo.Id, updatedTodo);
        var retrievedTodo = await _client.GetTodoByIdAsync(createdTodo.Id);

        // Assert
        statusCode.Should().Be(HttpStatusCode.NoContent);
        retrievedTodo.Should().NotBeNull();
        retrievedTodo!.Name.Should().Be("Updated Name");
        retrievedTodo.IsComplete.Should().BeTrue();

        // Cleanup
        await _client.DeleteTodoAsync(createdTodo.Id);
    }

    [Test]
    public async Task UpdateTodo_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Name = "Test Todo",
            IsComplete = false
        };
        var createdTodo = await _client.CreateTodoAsync(newTodo);

        var updateTodo = new TodoItem
        {
            Id = createdTodo.Id + 1, // Mismatched ID
            Name = "Updated Name",
            IsComplete = true
        };

        // Act
        var statusCode = await _client.UpdateTodoAsync(createdTodo.Id, updateTodo);

        // Assert
        statusCode.Should().Be(HttpStatusCode.BadRequest);

        // Cleanup
        await _client.DeleteTodoAsync(createdTodo.Id);
    }

    [Test]
    public async Task UpdateTodo_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        const long nonExistentId = 999999;
        var updateTodo = new TodoItem
        {
            Id = nonExistentId,
            Name = "Updated Name",
            IsComplete = true
        };

        // Act
        var statusCode = await _client.UpdateTodoAsync(nonExistentId, updateTodo);

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteTodo_RemovesTodo()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Name = "Todo to Delete",
            IsComplete = false
        };
        var createdTodo = await _client.CreateTodoAsync(newTodo);

        // Act
        var statusCode = await _client.DeleteTodoAsync(createdTodo.Id);
        var retrievedTodo = await _client.GetTodoByIdAsync(createdTodo.Id);

        // Assert
        statusCode.Should().Be(HttpStatusCode.NoContent);
        retrievedTodo.Should().BeNull();
    }

    [Test]
    public async Task DeleteTodo_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        const long nonExistentId = 999999;

        // Act
        var statusCode = await _client.DeleteTodoAsync(nonExistentId);

        // Assert
        statusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateAndDeleteMultipleTodos_WorksCorrectly()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Name = "First Todo", IsComplete = false },
            new() { Name = "Second Todo", IsComplete = true },
            new() { Name = "Third Todo", IsComplete = false }
        };

        // Act - Create
        var createdTodos = new List<TodoItem>();
        foreach (var todo in todos)
        {
            var created = await _client.CreateTodoAsync(todo);
            createdTodos.Add(created);
        }

        var allTodos = await _client.GetTodosAsync();

        // Assert
        createdTodos.Should().HaveCount(3);

        // Cleanup - Delete all
        foreach (var created in createdTodos)
        {
            var statusCode = await _client.DeleteTodoAsync(created.Id);
            statusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [Test]
    public async Task TodoItem_ShouldMatchExpectedSchema()
    {
        // Arrange
        var newTodo = new TodoItem
        {
            Name = "Schema Test Todo",
            IsComplete = true
        };

        // Act
        var createdTodo = await _client.CreateTodoAsync(newTodo);

        // Assert
        createdTodo.Should().NotBeNull();
        createdTodo.Should().BeOfType<TodoItem>();
        createdTodo.Should().Match<TodoItem>(t =>
            t.Id > 0 &&
            !string.IsNullOrEmpty(t.Name) &&
            t.IsComplete != null);

        // Cleanup
        await _client.DeleteTodoAsync(createdTodo.Id);
    }
}
