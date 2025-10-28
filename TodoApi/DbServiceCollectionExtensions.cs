using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

public static class DbServiceCollectionExtensions
{
    public static IServiceCollection AddDb(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase("TodoList"));
        }
        else
        {
            services.AddDbContext<TodoContext>(options => options.UseNpgsql("@Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase"));
        }

        return services;
    }
}