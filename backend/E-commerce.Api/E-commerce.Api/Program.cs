
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

namespace E_commerce.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
                policy.WithOrigins(
                        "http://localhost:5173", "http://127.0.0.1:5173",
                        "http://localhost:5174", "http://127.0.0.1:5174")
                    .AllowAnyHeader()
                    .AllowAnyMethod()));
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<E_commerce.Service.Abstraction.ITokenService>(sp => sp.GetRequiredService<TokenService>());
            builder.Services.RegisterModuleInfra(builder.Configuration);
            builder.Services.RegisterModuleServices(builder.Configuration);
            builder.Services.RegisterModuleCore(builder.Configuration);
            builder.Services.RegisterModuleData(builder.Configuration);
            builder.Services.AddOptions<CloudinarySettings>()
                .Bind(builder.Configuration.GetSection("Cloudinary"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            var jwt = builder.Configuration.GetSection("Jwt");
            var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwt["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


            // swagger 
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            await DatabaseSeeder.SeedAsync(app.Services);


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI();
            }
            // Configure the HTTP request pipeline.

            app.UseExceptionHandler();
            app.UseCors("Frontend");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
