using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TodoContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
db.Database.Migrate();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
