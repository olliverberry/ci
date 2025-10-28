using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Models;

namespace TodoApi.Tests;

[TestFixture]
public class TodoItemsControllerTests
{
    [Test]
    public async Task GetTodoItems_ReturnsEmptyList_WhenNoItemsExist()
    {
        // Arrange
        using var context = GetInMemoryContext("GetTodoItems_Empty");
        var controller = new TodoItemsController(context);

        // Act
        var result = await controller.GetTodoItems();

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetTodoItems_ReturnsAllItems_WhenItemsExist()
    {
        // Arrange
        using var context = GetInMemoryContext("GetTodoItems_WithData");
        context.TodoItems.AddRange(
            new TodoItem { Id = 1, Name = "Test Item 1", IsComplete = false },
            new TodoItem { Id = 2, Name = "Test Item 2", IsComplete = true }
        );
        await context.SaveChangesAsync();

        var controller = new TodoItemsController(context);

        // Act
        var result = await controller.GetTodoItems();

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetTodoItem_ReturnsItem_WhenItemExists()
    {
        // Arrange
        using var context = GetInMemoryContext("GetTodoItem_Exists");
        var item = new TodoItem { Id = 1, Name = "Test Item", IsComplete = false };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        var controller = new TodoItemsController(context);

        // Act
        var result = await controller.GetTodoItem(1);

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(new
        {
            Id = 1,
            Name = "Test Item",
            IsComplete = false
        });
    }

    [Test]
    public async Task GetTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryContext("GetTodoItem_NotFound");
        var controller = new TodoItemsController(context);

        // Act
        var result = await controller.GetTodoItem(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task PostTodoItem_CreatesNewItem_AndReturnsCreatedAtAction()
    {
        // Arrange
        using var context = GetInMemoryContext("PostTodoItem_Create");
        var controller = new TodoItemsController(context);
        var newItem = new TodoItem { Name = "New Item", IsComplete = false };

        // Act
        var result = await controller.PostTodoItem(newItem);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.ActionName.Should().Be(nameof(TodoItemsController.GetTodoItem));

        var createdItem = createdResult.Value as TodoItem;
        createdItem.Should().NotBeNull();
        createdItem!.Name.Should().Be("New Item");
        createdItem.Id.Should().BeGreaterThan(0);

        // Verify item was saved to database
        var savedItem = await context.TodoItems.FindAsync(createdItem.Id);
        savedItem.Should().NotBeNull();
        savedItem!.Name.Should().Be("New Item");
    }

    [Test]
    public async Task PutTodoItem_UpdatesItem_WhenItemExists()
    {
        // Arrange
        using var context = GetInMemoryContext("PutTodoItem_Update");
        var item = new TodoItem { Id = 1, Name = "Original Name", IsComplete = false };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        // Detach the entity to simulate a fresh request
        context.Entry(item).State = EntityState.Detached;

        var controller = new TodoItemsController(context);
        var updatedItem = new TodoItem { Id = 1, Name = "Updated Name", IsComplete = true };

        // Act
        var result = await controller.PutTodoItem(1, updatedItem);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify item was updated in database
        var savedItem = await context.TodoItems.FindAsync(1L);
        savedItem.Should().NotBeNull();
        savedItem!.Name.Should().Be("Updated Name");
        savedItem.IsComplete.Should().BeTrue();
    }

    [Test]
    public async Task PutTodoItem_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        using var context = GetInMemoryContext("PutTodoItem_IdMismatch");
        var controller = new TodoItemsController(context);
        var item = new TodoItem { Id = 2, Name = "Test Item", IsComplete = false };

        // Act
        var result = await controller.PutTodoItem(1, item);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task PutTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryContext("PutTodoItem_NotFound");
        var controller = new TodoItemsController(context);
        var item = new TodoItem { Id = 999, Name = "Test Item", IsComplete = false };

        // Act
        var result = await controller.PutTodoItem(999, item);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task DeleteTodoItem_RemovesItem_WhenItemExists()
    {
        // Arrange
        using var context = GetInMemoryContext("DeleteTodoItem_Delete");
        var item = new TodoItem { Id = 1, Name = "Test Item", IsComplete = false };
        context.TodoItems.Add(item);
        await context.SaveChangesAsync();

        var controller = new TodoItemsController(context);

        // Act
        var result = await controller.DeleteTodoItem(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify item was removed from database
        var deletedItem = await context.TodoItems.FindAsync(1L);
        deletedItem.Should().BeNull();
    }

    [Test]
    public async Task DeleteTodoItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryContext("DeleteTodoItem_NotFound");
        var controller = new TodoItemsController(context);

        // Act
        var result = await controller.DeleteTodoItem(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task GetTodoItems_ReturnsCorrectItemCount_AfterMultipleOperations()
    {
        // Arrange
        using var context = GetInMemoryContext("GetTodoItems_MultipleOps");
        var controller = new TodoItemsController(context);

        // Act - Add multiple items
        await controller.PostTodoItem(new TodoItem { Name = "Item 1", IsComplete = false });
        await controller.PostTodoItem(new TodoItem { Name = "Item 2", IsComplete = false });
        await controller.PostTodoItem(new TodoItem { Name = "Item 3", IsComplete = true });

        var result = await controller.GetTodoItems();

        // Assert
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(3);
    }

    private static TodoContext GetInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new TodoContext(options);
    }
}
