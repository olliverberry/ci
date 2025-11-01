using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TodoContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

using var scope = app.Services.CreateScope();
for (var i = 1; i <= 10; i++)
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
        db.Database.Migrate();
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        await Task.Delay((int)Math.Pow(2, i) * 1000);
        if (i == 10)
        {
            throw new Exception("Failed to migrate database");
        }
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
