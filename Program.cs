using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using MongoDB.Bson.Serialization;
using configs;
using DevCl.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using DevCL.Utils.Converters;

namespace DevCL;

internal class Program
{
    private static void Main(string[] args)
    {
        Env.Load();

        //converts class fields into camelCase for MongoDB
        ConventionRegistry.Register("camelCase", new ConventionPack {
            new CamelCaseElementNameConvention()
        }, _ => true);

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<MongoDbSettings>(setting =>
        {
            setting.ConnectionString = Env.GetString("DB_URL");
        });

        builder.Services.AddSingleton(sp => new JwtSecurityTokenHandler());
        builder.Services.AddSingleton<MongoDbContext>();
        builder.Services.AddSingleton<TaskService>();
        builder.Services.AddSingleton<TaskDocService>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddSingleton<AttributeService>();
        builder.Services.AddControllers().AddJsonOptions(opts => {
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
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

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new() {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new()
            {
                {
                    new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    new string[] {}
                }
            });
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope()) {
            Initialization.SeedAttributesDB(scope.ServiceProvider.GetRequiredService<AttributeService>());
        }
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowSpecificOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();

    }
}