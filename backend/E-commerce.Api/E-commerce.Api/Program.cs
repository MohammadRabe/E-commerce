
using E_commerce.Core;
using E_commerce.Data;
using E_commerce.Infrastructure;
using E_commerce.Infrastructure.Data;
using E_commerce.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using E_commerce.Api.Services;
using E_commerce.Data.Options;
using E_commerce.Api.Middleware;
using Microsoft.OpenApi;
using E_commerce.Api.Hubs;
using Serilog;
using Serilog.Debugging;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace E_commerce.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console()
                .CreateBootstrapLogger();
            SelfLog.Enable(TextWriter.Synchronized(Console.Error));

            try
            {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                var databaseConnection = context.Configuration.GetConnectionString("Default")
                    ?? throw new InvalidOperationException("Connection string 'Default' was not found.");
                var seqUrl = context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341";

                loggerConfiguration
                    .MinimumLevel.Error()
                    .MinimumLevel.Override("E_commerce.Service.Services.FawaterakPaymentService", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "E-commerce.Api")
                    .WriteTo.Console();
                    
            });
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
                policy.WithOrigins("https://e-commerce-sooq.vercel.app")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<E_commerce.Service.Abstraction.IOrderNotificationService, OrderNotificationService>();
            builder.Services.AddScoped<E_commerce.Service.Abstraction.ITokenService>(sp => sp.GetRequiredService<TokenService>());
            builder.Services.RegisterModuleInfra(builder.Configuration);
            builder.Services.RegisterModuleServices(builder.Configuration);
            builder.Services.RegisterModuleCore(builder.Configuration);
            builder.Services.RegisterModuleData(builder.Configuration);
            builder.Services.AddOptions<CloudinarySettings>()
                .Bind(builder.Configuration.GetSection("Cloudinary"));
            builder.Services.AddOptions<E_commerce.Data.Options.FawaterakSettings>()
                .Bind(builder.Configuration.GetSection("Fawaterak"))
                .Validate(settings =>
                    !string.IsNullOrWhiteSpace(settings.ClientId) &&
                    !string.IsNullOrWhiteSpace(settings.ClientSecret),
                    "Fawaterak credentials are missing. Set Fawaterak__ClientId and Fawaterak__ClientSecret using .NET user secrets or environment variables.")
                .Validate(settings => Uri.TryCreate(settings.TokenUrl, UriKind.Absolute, out _) &&
                    Uri.TryCreate(settings.ApiBaseUrl, UriKind.Absolute, out _) &&
                    Uri.TryCreate(settings.FrontendBaseUrl, UriKind.Absolute, out _),
                    "Fawaterak TokenUrl, ApiBaseUrl, and FrontendBaseUrl must be absolute URLs.");
            var jwt = builder.Configuration.GetSection("Jwt");
            var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments(OrderNotificationsHub.Route))
                                context.Token = accessToken;
                            return Task.CompletedTask;
                        }
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwt["Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


            // swagger 
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the raw access token only (do not include the 'Bearer ' prefix)."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });
            var app = builder.Build();

            await DatabaseSeeder.SeedAsync(app.Services);


                app.UseSwagger();

                app.UseSwaggerUI();
            // Configure the HTTP request pipeline.

            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
                    await next();
            });
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (context, _, exception) =>
                    exception is not null || context.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                        : context.Response.StatusCode >= 400
                            ? LogEventLevel.Warning
                            : LogEventLevel.Information;
            });
            app.UseExceptionHandler();
            app.UseCors("Frontend");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<OrderNotificationsHub>(OrderNotificationsHub.Route);

            await app.RunAsync();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "API terminated unexpectedly");
                throw;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
