using DotNetEnv;
using DevCL.Database;
using MongoDB.Driver;

namespace DevCL;

internal class Program
{
    private static void Main(string[] args)
    {
        Env.Load();

        CLCollections.Init();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton(sp => new MongoClient(Env.GetString("DB_URL")));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowSpecificOrigins", policy => {
                policy.WithOrigins("https://localhost:5173", "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowSpecificOrigins");
        app.MapControllers();

        app.Run();
    }
}