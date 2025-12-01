using System.Text;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using WebAPI.Filters;
using WebAPI.Middleware;

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

var pgConnectionString = BuildPostgresConnectionString(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(pgConnectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ITempFileService, TempFileService>();
builder.Services.AddSingleton<IMongoMessageDbService, MongoMessageDbService>();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );

    options.OperationFilter<FileUploadOperationFilter>();
});

builder.Services.AddControllers();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var token = builder.Configuration["AppSettings:Token"];
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("JWT secret is not configured in AppSettings.");
        }
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(token)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        // Configure JWT for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chathub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowMyOrigin",
        policyBuilder =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            var originsFromEnv = builder.Configuration["CORS_ORIGINS"];
            if (!string.IsNullOrWhiteSpace(originsFromEnv))
            {
                allowedOrigins = originsFromEnv
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            if (allowedOrigins.Length == 0)
            {
                allowedOrigins = new[] { "http://localhost:3000" };
            }

            policyBuilder
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder.Services.AddSignalR();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowMyOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();

public partial class Program { }

static string BuildPostgresConnectionString(ConfigurationManager configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("DefaultConnection is not configured.");
    }

    var pgDatabaseUrl = configuration["DATABASE_URL"];
    if (string.IsNullOrEmpty(pgDatabaseUrl))
    {
        return connectionString;
    }

    var uri = new Uri(pgDatabaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var user = userInfo[0];
    var password = userInfo[1];

    return
        $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};User ID={user};Password={password};SslMode=Require;Trust Server Certificate=true;";
}
