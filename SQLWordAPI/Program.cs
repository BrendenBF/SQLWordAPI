using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SQLWordAPI.MiddlewareExtensions;
using SQLWordAPI.Repositories;
using SQLWordAPI.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
        {
            // Adds a custom error response factory when ModelState is invalid
            options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.ProduceErrorResponse;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger XML Documentation : SQL Sanitise Restful Web API", Version = "v1" });
        });

        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                              builder =>
                              {
                                  builder.WithOrigins("*");
                              });
        }); 
         
        builder.Services.AddScoped<ISqlWordRepository, SqlWordRepository>();
        builder.Services.AddScoped<ISqlWordService, SqlWordService>();

        builder.Services.AddMemoryCache();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors(MyAllowSpecificOrigins);

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}