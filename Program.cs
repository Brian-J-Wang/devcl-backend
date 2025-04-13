using DotNetEnv;
using DevCL.Database;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using MongoDB.Bson.Serialization;
using System.Text.Json;

namespace DevCL;

internal class Program
{
    private static void Main(string[] args)
    {
        Env.Load();

        CLCollections.Init();


        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton(sp => new MongoClient(Env.GetString("DB_URL")));
        builder.Services.AddSingleton(sp => new JwtSecurityTokenHandler());
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

        builder.Services.AddAuthentication(cfg => {
            cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x => {
            x.RequireHttpsMetadata = false;
            x.SaveToken = false;
            x.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8
                    .GetBytes(Env.GetString("JWT_SECRET").ToArray())
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        BsonSerializer.RegisterSerializer(new ObjectDictionarySerializer());
        BsonSerializer.RegisterSerializer(new JsonElementSerializer());
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.UseHttpsRedirection();
        app.UseCors("AllowSpecificOrigins");
        app.MapControllers();

        app.Run();
    }
}